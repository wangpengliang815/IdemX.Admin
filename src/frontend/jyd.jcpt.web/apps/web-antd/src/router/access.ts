import type { ComponentRecordType, GenerateMenuAndRoutesOptions } from '@vben/types';

import { generateAccessible } from '@vben/access';
import { preferences } from '@vben/preferences';
import { useUserStore } from '@vben/stores';

import type { SysUserProfileInfo } from '#/api/profile';

import { getAllMenusApi } from '#/api/core';
import { BasicLayout, IFrameView } from '#/layouts';
import { usePartnerContextStore } from '#/store';

const forbiddenComponent = () => import('#/views/_core/fallback/forbidden.vue');

type MenuRoleCode = 'admin' | 'buyer' | 'cp';

async function generateAccess(options: GenerateMenuAndRoutesOptions, menuRoleCode?: MenuRoleCode) {
  const pageMap: ComponentRecordType = import.meta.glob('../views/**/*.vue');

  const layoutMap: ComponentRecordType = {
    BasicLayout,
    IFrameView,
  };

  const info = useUserStore().userInfo as null | SysUserProfileInfo | undefined;
  const roleCode: MenuRoleCode =
    menuRoleCode ??
    (info?.isAdmin
      ? 'admin'
      : (() => {
          const code = info?.roles?.[0]?.roleCode?.toLowerCase();
          return code === 'admin' || code === 'buyer' || code === 'cp' ? code : usePartnerContextStore().mode;
        })());

  const result = await generateAccessible(preferences.app.accessMode, {
    ...options,
    fetchMenuListAsync: async () => getAllMenusApi(roleCode),
    forbiddenComponent,
    layoutMap,
    pageMap,
  });

  return result;
}

export { generateAccess };
