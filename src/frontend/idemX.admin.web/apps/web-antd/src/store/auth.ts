import type { LoginByPasswordReq, LoginByPhoneReq } from '#/api/core/auth';

import { nextTick, ref } from 'vue';
import { useRouter } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { resetAllStores, useAccessStore, useUserStore } from '@vben/stores';

import { message } from 'ant-design-vue';
import { defineStore } from 'pinia';

import { loginByPasswordApi, loginByPhoneApi, logoutApi } from '#/api/core';
import { getUserInfoApi } from '#/api/profile';

export type GetUserInfoOptions = {
  /** 为 true 时忽略本地已缓存的 userInfo，强制请求后端（头像/资料保存、角色变更后使用） */
  force?: boolean;
};

export const useAuthStore = defineStore('auth', () => {
  const accessStore = useAccessStore();
  const userStore = useUserStore();
  const router = useRouter();

  const loginLoading = ref(false);
  // 头像缓存穿透版本号（统一在 getUserInfo 成功时更新）
  const avatarNonce = ref(0);

  /** 并发去重：多处在同一时刻拉用户信息时只发一次 HTTP */
  let getUserInfoInFlight: null | Promise<any | null> = null;

  /**
   * 异步处理登录操作
   * Asynchronously handle the login process
   * @param params 登录表单数据
   */
  async function authLoginByPassword(params: LoginByPasswordReq, onSuccess?: () => Promise<void> | void) {
    try {
      loginLoading.value = true;
      const loginResult = await loginByPasswordApi(params);

      if (loginResult.code !== 0) {
        message.error(loginResult.msg);
        return;
      }

      const accessToken = loginResult.data;

      if (accessToken) {
        // 先清空旧 token 再设新 token，避免生产环境从缓存恢复的旧 token 造成竞态（清理缓存后正常的根因）
        accessStore.setAccessToken(null);
        await nextTick();
        accessStore.setAccessToken(accessToken);

        await getUserInfo({ force: true });

        const successMsg = loginResult.msg || '登录成功';
        if (accessStore.loginExpired) {
          accessStore.setLoginExpired(false);
          message.success(successMsg);
        } else {
          if (onSuccess) {
            await onSuccess?.();
          } else {
            // 先提示再跳转，避免跳转后组件卸载导致不提示
            message.success(successMsg);
            accessStore.setIsAccessChecked(false);
            await nextTick();
            await router.push('/');
          }
        }
      } else {
        message.error(loginResult.msg);
      }
    } finally {
      loginLoading.value = false;
    }
  }

  /**
   * 异步处理手机验证码登录操作
   * Asynchronously handle the phone login process
   * @param params 登录表单数据
   */
  async function authLoginByPhone(params: LoginByPhoneReq, onSuccess?: () => Promise<void> | void) {
    try {
      loginLoading.value = true;
      const loginResult = await loginByPhoneApi(params);

      if (loginResult.code !== 0) {
        message.error(loginResult.msg);
        return;
      }

      const accessToken = loginResult.data;

      if (accessToken) {
        accessStore.setAccessToken(null);
        await nextTick();
        accessStore.setAccessToken(accessToken);

        await getUserInfo({ force: true });

        const successMsg = loginResult.msg || '登录成功';
        if (accessStore.loginExpired) {
          accessStore.setLoginExpired(false);
          message.success(successMsg);
        } else {
          if (onSuccess) {
            await onSuccess?.();
          } else {
            message.success(successMsg);
            accessStore.setIsAccessChecked(false);
            await nextTick();
            await router.push('/');
          }
        }
      } else {
        message.error(loginResult.msg);
      }
    } finally {
      loginLoading.value = false;
    }
  }

  /**
   * 异步处理登出操作
   * Asynchronously handle the logout process
   * @param redirect 是否跳转到登录页（默认 true）
   */
  async function logout(redirect: boolean = true) {
    try {
      await logoutApi();
    } catch {
      // 不做任何处理，即使后端接口失败，前端也必须执行登出逻辑
    }

    // 清理动态路由（防止再次登录时路由重复注册）
    const existingRoutes = accessStore.accessRoutes || [];
    if (existingRoutes.length > 0) {
      existingRoutes.forEach((route: any) => {
        if (route.name) {
          try {
            router.removeRoute(route.name);
          } catch {}
        }
      });
    }

    // 重置所有 Pinia Store（清理用户状态、权限状态等）
    resetAllStores();
    accessStore.setLoginExpired(false);

    // 跳转回登录页
    try {
      const currentPath = router?.currentRoute?.value?.fullPath || window.location.pathname;
      if (router?.replace) {
        await router.replace({
          path: LOGIN_PATH,
          query: redirect
            ? {
                // 记录当前页面路径，以便登录后自动跳转回来
                redirect: encodeURIComponent(currentPath),
              }
            : {},
        });
      } else {
        // 如果 router 未初始化，直接使用 window.location 跳转
        const redirectParam = redirect ? `?redirect=${encodeURIComponent(currentPath)}` : '';
        window.location.href = `${LOGIN_PATH}${redirectParam}`;
      }
    } catch {
      // 如果路由跳转失败，使用 window.location 作为兜底方案
      const currentPath = window.location.pathname;
      const redirectParam = redirect ? `?redirect=${encodeURIComponent(currentPath)}` : '';
      window.location.href = `${LOGIN_PATH}${redirectParam}`;
    }
  }

  /**
   * 获取用户信息并写入 userStore。
   * 默认若 Pinia 中已有 userInfo 则不再请求后端；路由守卫等并发调用会合并为单次请求。
   */
  async function getUserInfo(options?: GetUserInfoOptions): Promise<any | null> {
    const force = options?.force === true;
    // 若有进行中的拉取（含 force），先合并，避免与「读缓存」分支交错返回旧数据
    if (getUserInfoInFlight) {
      return getUserInfoInFlight;
    }

    if (!force && userStore.userInfo) {
      return userStore.userInfo;
    }

    getUserInfoInFlight = (async () => {
      const response = await getUserInfoApi();
      if (response.code === 0 && response.data) {
        userStore.setUserInfo(response.data as any);
        avatarNonce.value = Date.now();
        return response.data;
      }
      return null;
    })();

    try {
      return await getUserInfoInFlight;
    } finally {
      getUserInfoInFlight = null;
    }
  }

  /**
   * 重置登录状态
   * Reset login state
   */
  function $reset() {
    loginLoading.value = false;
  }

  return {
    $reset,
    avatarNonce,
    authLoginByPassword,
    authLoginByPhone,
    getUserInfo,
    loginLoading,
    logout,
  };
});
