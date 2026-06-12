import type { LoginByPasswordReq } from '#/api/core/auth';

import { nextTick, ref } from 'vue';
import { useRouter } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { resetAllStores, useAccessStore, useUserStore } from '@vben/stores';

import { message } from 'ant-design-vue';
import { defineStore } from 'pinia';

import { loginByPasswordApi, logoutApi } from '#/api/core';
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
  const avatarNonce = ref(0);

  let getUserInfoInFlight: null | Promise<any | null> = null;

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

  async function logout(redirect: boolean = true) {
    try {
      await logoutApi();
    } catch {
      // 即使后端接口失败，前端也必须执行登出逻辑
    }

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

    resetAllStores();
    accessStore.setLoginExpired(false);

    try {
      const currentPath = router?.currentRoute?.value?.fullPath || window.location.pathname;
      if (router?.replace) {
        await router.replace({
          path: LOGIN_PATH,
          query: redirect
            ? {
                redirect: encodeURIComponent(currentPath),
              }
            : {},
        });
      } else {
        const redirectParam = redirect ? `?redirect=${encodeURIComponent(currentPath)}` : '';
        window.location.href = `${LOGIN_PATH}${redirectParam}`;
      }
    } catch {
      const currentPath = window.location.pathname;
      const redirectParam = redirect ? `?redirect=${encodeURIComponent(currentPath)}` : '';
      window.location.href = `${LOGIN_PATH}${redirectParam}`;
    }
  }

  async function getUserInfo(options?: GetUserInfoOptions): Promise<any | null> {
    const force = options?.force === true;
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

  function $reset() {
    loginLoading.value = false;
  }

  return {
    $reset,
    avatarNonce,
    authLoginByPassword,
    getUserInfo,
    loginLoading,
    logout,
  };
});
