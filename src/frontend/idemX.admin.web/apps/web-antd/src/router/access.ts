import type { ComponentRecordType, GenerateMenuAndRoutesOptions } from '@vben/types';

import { generateAccessible } from '@vben/access';
import { preferences } from '@vben/preferences';
import { useUserStore } from '@vben/stores';

import type { SysUserProfileInfo } from '#/api/profile';

import { getAllMenusApi } from '#/api/core';
import { BasicLayout, IFrameView } from '#/layouts';

const forbiddenComponent = () => import('#/views/_core/fallback/forbidden.vue');

function resolveMenuRoleCode(info: null | SysUserProfileInfo | undefined, menuRoleCode?: string): string {
  if (menuRoleCode) return menuRoleCode;
  if (info?.isAdmin) return 'admin';
  const code = info?.roles?.[0]?.roleCode;
  return code ?? 'admin';
}

async function generateAccess(options: GenerateMenuAndRoutesOptions, menuRoleCode?: string) {
  const pageMap: ComponentRecordType = import.meta.glob('../views/**/*.vue');

  const layoutMap: ComponentRecordType = {
    BasicLayout,
    IFrameView,
  };

  const info = useUserStore().userInfo as null | SysUserProfileInfo | undefined;
  const roleCode = resolveMenuRoleCode(info, menuRoleCode);

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
