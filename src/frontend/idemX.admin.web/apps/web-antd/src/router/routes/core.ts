import type { RouteRecordRaw } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';

import { $t } from '#/locales';

const BasicLayout = () => import('#/layouts/basic.vue');
const AuthPageLayout = () => import('#/layouts/auth.vue');

/**
 * 全局 404 页面
 * 作为最后兜底路由，匹配所有未声明路径
 */
const fallbackNotFoundRoute: RouteRecordRaw = {
  component: () => import('#/views/_core/fallback/not-found.vue'),
  meta: {
    hideInBreadcrumb: true,
    hideInMenu: true,
    hideInTab: true,
    title: '404',
  },
  name: 'FallbackNotFound',
  path: '/:path(.*)*',
};

/**
 * 应用核心静态路由骨架
 * - Root：基础布局与默认首页
 * - Authentication：登录、找回密码等认证页面
 * 业务模块路由由后端菜单动态挂载
 */
const coreRoutes: RouteRecordRaw[] = [
  {
    component: BasicLayout,
    meta: {
      hideInBreadcrumb: true,
      title: 'Root',
    },
    name: 'Root',
    path: '/',
    redirect: preferences.app.defaultHomePath,
    children: [
      {
        name: 'About',
        path: '/about',
        component: () => import('#/views/_core/about/index.vue'),
        meta: {
          hideInMenu: false,
          title: '关于',
          icon: 'mdi:information-outline',
        },
      },
      {
        name: 'Profile',
        path: '/profile',
        component: () => import('#/views/_core/profile/layout.vue'),
        meta: {
          hideInMenu: true,
          title: $t('page.auth.profile'),
        },
        children: [
          {
            name: 'ProfileHome',
            path: '',
            component: () => import('#/views/_core/profile/index.vue'),
            meta: {
              hideInMenu: true,
              title: '个人中心',
            },
          },
          {
            name: 'ProfileBasic',
            path: 'basic',
            component: () => import('#/views/_core/profile/base-setting.vue'),
            meta: {
              hideInMenu: true,
              title: '个人资料',
            },
          },
          {
            name: 'ProfileSecurity',
            path: 'security',
            component: () => import('#/views/_core/profile/security-setting.vue'),
            meta: {
              hideInMenu: true,
              title: '账号安全',
            },
          },
          {
            name: 'ProfileReleaseLog',
            path: 'release-log',
            component: () => import('#/views/_core/profile/release-log.vue'),
            meta: { hideInMenu: true, title: '版本更新日志' },
          },
        ],
      },
    ],
  },
  {
    component: AuthPageLayout,
    meta: {
      hideInTab: true,
      title: 'Authentication',
    },
    name: 'Authentication',
    path: '/auth',
    redirect: LOGIN_PATH,
    children: [
      {
        name: 'Login',
        path: 'login',
        component: () => import('#/views/_core/authentication/login.vue'),
        meta: {
          hideInMenu: true,
          title: $t('page.auth.login'),
        },
      },
      {
        name: 'ForgetPassword',
        path: 'forget-password',
        component: () => import('#/views/_core/authentication/forget-password.vue'),
        meta: {
          hideInMenu: true,
          title: $t('page.auth.forgetPassword'),
        },
      },
    ],
  },
];

export { coreRoutes, fallbackNotFoundRoute };
