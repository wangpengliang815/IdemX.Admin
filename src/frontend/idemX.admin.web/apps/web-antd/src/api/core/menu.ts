import type { SysMenuResp } from '#/api/system/menu';

import { getMenusApi } from '#/api/profile';

/**
 * 将后端返回的菜单数据映射为Vben路由格式
 * @param backendMenus - 后端返回的菜单数组
 * @param parentPath - 父级路径，用于路径转换
 * @param usedNames - 已使用的路由名称集合，用于生成唯一名称
 * @returns 转换后的Vben路由配置数组
 */
function transformMenuData(backendMenus: SysMenuResp[], parentPath?: string, usedNames: Set<string> = new Set()): any[] {
  return backendMenus.map((m) => {
    // 路径转换：将绝对路径转换为相对路径
    let path = m.path;
    if (path && path.startsWith('/') && parentPath) {
      const parentPathWithoutSlash = parentPath.replace(/\/$/, '');
      if (path.startsWith(`${parentPathWithoutSlash}/`)) {
        path = path.slice(Math.max(0, parentPathWithoutSlash.length + 1));
      } else if (path !== parentPathWithoutSlash) {
        const pathParts = path.split('/').filter(Boolean);
        const parentParts = parentPathWithoutSlash.split('/').filter(Boolean);
        path = pathParts.length > parentParts.length ? pathParts.slice(parentParts.length).join('/') : pathParts[pathParts.length - 1] || path;
      }
    }

    // 生成唯一的路由名称
    let routeName = m.name;
    if (usedNames.has(routeName)) {
      const pathParts = (parentPath ? `${parentPath}/${path}` : path).split('/').filter(Boolean);
      routeName = pathParts.map((part: string) => part.charAt(0).toUpperCase() + part.slice(1)).join('');

      // 如果生成的名称仍然重复，添加数字后缀
      let suffix = 1;
      let finalName = routeName;
      while (usedNames.has(finalName)) {
        finalName = `${routeName}${suffix}`;
        suffix++;
      }
      routeName = finalName;
    }
    usedNames.add(routeName);

    const result: any = {
      path,
      name: routeName,
      component: m.component || undefined,
      redirect: m.redirect || undefined,
      meta: {
        title: m.title,
        icon: m.icon || 'mdi:menu',
        affixTab: m.affixTab || false,
        keepAlive: m.keepAlive || false,
        externalLink: m.isExternal && m.externalUrl ? m.externalUrl : null,
        iframeUrl: m.iframeUrl || null,
        badge: m.badge,
        badgeType: m.badgeType,
        badgeVariants: m.badgeVariants,
      },
    };

    const currentFullPath = parentPath ? `${parentPath.replace(/\/$/, '')}/${path}` : path;

    // 递归处理子菜单
    if (m.children && m.children.length > 0) {
      result.children = transformMenuData(m.children, currentFullPath, usedNames);
      // 父级未配置 redirect 时自动指向第一个子级完整路径，避免面包屑点父级 404
      if (!result.redirect && result.children.length > 0) {
        const firstPath = result.children[0].path as string;
        const full = currentFullPath ? `${currentFullPath.replace(/\/$/, '')}/${firstPath.startsWith('/') ? firstPath.slice(1) : firstPath}` : firstPath;
        result.redirect = full.replaceAll(/\/+/g, '/');
      }
    }

    // 如果菜单项没有组件且没有子菜单，但有空的子菜单数据，设置为布局组件
    if (!result.component && (!result.children || result.children.length === 0) && m.children && Array.isArray(m.children) && m.children.length === 0) {
      result.component = 'BasicLayout';
    }

    return result;
  });
}

/**
 * 获取所有菜单并转换为 Vben 路由格式
 */
export async function getAllMenusApi(roleCode: string) {
  const response = await getMenusApi(roleCode);
  if (response.code !== 0) {
    throw new Error(response.msg);
  }

  return transformMenuData(response.data!);
}
