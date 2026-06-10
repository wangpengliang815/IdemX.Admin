import type { Router } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { startProgress, stopProgress } from '@vben/utils';

import { accessRoutes, coreRouteNames } from '#/router/routes';
import { useAuthStore, useEnumsStore, usePartnerContextStore } from '#/store';

import { trackBaiduPageview } from '#/utils/baidu-tongji';

import { generateAccess } from './access';

/** Root 下仅保留的 core 子路由名称（与 core.ts 一致） */
const ROOT_CORE_CHILD_NAMES = new Set(['About', 'Profile']);

/** 当前菜单视角：admin 用 admin，否则用 partner 视角（buyer/cp）。守卫里显式传入，避免异步里读 store 拿到旧值。 */
function getMenuRoleCode(userStore: ReturnType<typeof useUserStore>): 'admin' | 'buyer' | 'cp' {
  return String(userStore.userInfo?.userName ?? '').toLowerCase() === 'admin' ? 'admin' : usePartnerContextStore().mode;
}

/** 清理已存在的动态路由，防止重复注册 */
function cleanupExistingRoutes(router: Router, existingRoutes: any[]) {
  if (existingRoutes.length === 0) return;
  existingRoutes.forEach((route: any) => {
    if (route.name) {
      try {
        router.removeRoute(route.name);
      } catch {
        /* 路由可能已不存在 */
      }
    }
  });
}

/** 视角切换后重置 Root.children 仅含 core，否则 generateMenus 从 router 取 path 会拿到旧路由 path */
function resetRootToCoreChildren(router: Router) {
  const root = router.getRoutes().find((r) => r.path === '/');
  if (!root?.children?.length) return;
  const coreOnly = root.children.filter((c: any) => ROOT_CORE_CHILD_NAMES.has(c.name as string));
  if (coreOnly.length === root.children.length) return;
  root.children = coreOnly;
  if (root.name) {
    try {
      router.removeRoute(root.name);
    } catch {
      // ignore
    }
  }
  router.addRoute(root);
}

/**
 * 确保枚举数据已加载
 * 在用户登录后加载枚举数据，用于下拉框、标签等组件
 * @returns Promise<void>
 */
async function ensureEnumsLoaded() {
  const enumsStore = useEnumsStore();
  // Pinia 对 setup store 返回的 ref 在 store 上会解包，勿用 .value
  const enumMap = enumsStore.enums;
  if (Object.keys(enumMap).length === 0) {
    await enumsStore.loadEnums();
  }
}

/** 按 menuRoleCode 拉菜单并注册动态路由 */
async function generateAndRegisterRoutes(
  router: Router,
  accessStore: ReturnType<typeof useAccessStore>,
  userStore: ReturnType<typeof useUserStore>,
  authStore: ReturnType<typeof useAuthStore>,
  menuRoleCode: 'admin' | 'buyer' | 'cp',
) {
  const userInfo = userStore.userInfo || (await authStore.getUserInfo());
  const userRoles = userInfo?.roles ?? [];

  const { accessibleMenus, accessibleRoutes } = await generateAccess(
    {
      roles: userRoles,
      router,
      routes: accessRoutes,
    },
    menuRoleCode,
  );

  // 保存菜单信息和路由信息到 Store
  accessStore.setAccessMenus(accessibleMenus);
  accessStore.setAccessRoutes(accessibleRoutes);
  accessStore.setIsAccessChecked(true);

  return { accessibleMenus, accessibleRoutes, userInfo };
}

/**
 * 通用守卫配置
 * 处理页面加载进度条和页面加载状态记录
 * @param router - Vue Router 实例
 */
function setupCommonGuard(router: Router) {
  const loadedPaths = new Set<string>();

  router.beforeEach((to) => {
    to.meta.loaded = loadedPaths.has(to.path);

    // 页面加载进度条
    if (!to.meta.loaded && preferences.transition.progress) {
      startProgress();
    }
    return true;
  });

  router.afterEach((to) => {
    // 记录页面是否加载,如果已经加载，后续的页面切换动画等效果不在重复执行
    loadedPaths.add(to.path);

    trackBaiduPageview();

    // 关闭页面加载进度条
    if (preferences.transition.progress) {
      stopProgress();
    }
  });
}

/**
 * 权限访问守卫配置
 * @param router
 */
function setupAccessGuard(router: Router) {
  router.beforeEach(async (to, from) => {
    const accessStore = useAccessStore();
    const userStore = useUserStore();
    const authStore = useAuthStore();

    // 基本路由，这些路由不需要进入权限拦截
    // 登录页、ignoreAccess 核心路由可匿名；其余核心路由（如首页）仍需要 token
    if (coreRouteNames.includes(to.name as string)) {
      const isAuthRoute = to.path.startsWith('/auth') || to.path === LOGIN_PATH;
      const allowAnonymous = isAuthRoute || to.meta.ignoreAccess;

      if (!allowAnonymous && !accessStore.accessToken) {
        return {
          path: LOGIN_PATH,
          query: to.fullPath === preferences.app.defaultHomePath ? {} : { redirect: encodeURIComponent(to.fullPath) },
          replace: true,
        };
      }

      if (to.path === LOGIN_PATH && accessStore.accessToken) {
        // 如果 token 存在但没有用户信息，说明 token 可能无效
        // 清除 token 并允许访问登录页，避免重定向循环
        if (!userStore.userInfo) {
          accessStore.setAccessToken(null);
          return true;
        }
        // 有有效的 token 和用户信息，重定向到首页（落地页以 preferences 为准，避免后端 homePath 覆盖静态默认页）
        return decodeURIComponent((to.query?.redirect as string) || preferences.app.defaultHomePath);
      }
      // 如果已登录但动态路由还未生成，需要生成动态路由（刷新页面时）
      // 注意：登录页不需要生成动态路由，避免触发无效请求
      if (accessStore.accessToken && !accessStore.isAccessChecked && !isAuthRoute) {
        await ensureEnumsLoaded();
        // 先拉取用户信息再算菜单视角，避免刷新后 userInfo 未恢复导致管理员误用 buyer 菜单
        if (!userStore.userInfo) {
          await authStore.getUserInfo();
        }
        cleanupExistingRoutes(router, accessStore.accessRoutes || []);
        resetRootToCoreChildren(router);
        await generateAndRegisterRoutes(router, accessStore, userStore, authStore, getMenuRoleCode(userStore));

        // 如果访问的是根路径，生成路由后重定向到默认首页（与 core Root.redirect 一致，不用后端 homePath）
        if (to.path === '/') {
          return {
            path: preferences.app.defaultHomePath,
            replace: true,
          };
        }
      }
      return true;
    }

    // accessToken 检查
    if (!accessStore.accessToken) {
      // 明确声明忽略权限访问权限，则可以访问
      if (to.meta.ignoreAccess) {
        return true;
      }

      // 没有访问权限，跳转登录页面
      if (to.fullPath !== LOGIN_PATH) {
        return {
          path: LOGIN_PATH,
          // 如不需要，直接删除 query
          query: to.fullPath === preferences.app.defaultHomePath ? {} : { redirect: encodeURIComponent(to.fullPath) },
          // 携带当前跳转的页面，登录后重新跳转该页面
          replace: true,
        };
      }
      return to;
    }

    // 是否已经生成过动态路由
    if (accessStore.isAccessChecked) {
      return true;
    }

    await ensureEnumsLoaded();
    // 先拉取用户信息再算菜单视角，避免刷新后 userInfo 未恢复导致管理员误用 buyer 菜单
    if (!userStore.userInfo) {
      await authStore.getUserInfo();
    }
    cleanupExistingRoutes(router, accessStore.accessRoutes || []);
    resetRootToCoreChildren(router);
    await generateAndRegisterRoutes(router, accessStore, userStore, authStore, getMenuRoleCode(userStore));

    // 计算重定向路径
    // 优先级：from.query.redirect > 访问默认首页则保持默认首页 > 否则当前路径
    const redirectPath = (from.query.redirect ?? (to.path === preferences.app.defaultHomePath ? preferences.app.defaultHomePath : to.fullPath)) as string;

    return {
      ...router.resolve(decodeURIComponent(redirectPath)),
      replace: true,
    };
  });
}

/**
 * 主动重建菜单与路由（视角切换时调用，不依赖路由跳转触发守卫）
 */
async function rebuildMenus(router: Router) {
  const accessStore = useAccessStore();
  const userStore = useUserStore();
  const authStore = useAuthStore();

  await ensureEnumsLoaded();
  if (!userStore.userInfo) {
    await authStore.getUserInfo();
  }
  cleanupExistingRoutes(router, accessStore.accessRoutes || []);
  resetRootToCoreChildren(router);
  await generateAndRegisterRoutes(router, accessStore, userStore, authStore, getMenuRoleCode(userStore));
}

/**
 * 项目守卫配置
 * @param router
 */
function createRouterGuard(router: Router) {
  /** 通用 */
  setupCommonGuard(router);
  /** 权限访问 */
  setupAccessGuard(router);
}

export { createRouterGuard, rebuildMenus };
