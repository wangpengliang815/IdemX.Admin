import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

export interface SysMenuResp {
  id: string;
  parentId?: string;
  name: string;
  path: string;
  component: string;
  redirect: string;
  title: string;
  icon: string;
  sort: number;
  authority: string;
  roles: string;
  affixTab: boolean;
  isExternal: boolean;
  externalUrl: string;
  iframeUrl: string;
  keepAlive: boolean;
  menuType: number;
  hasChild?: boolean;
  badge: string;
  badgeType: string;
  badgeVariants: string;
  activeMenu: string;
  breadcrumbParentIcon: string;
  link: string;
  status: number;
  createTime?: string;
  updateTime?: string;
  createBy?: string;
  createByUsername?: string;
  children?: SysMenuResp[];
}

export interface SysMenuReq {
  parentId?: string;
  name: string;
  path: string;
  component: string;
  redirect: string;
  title: string;
  icon: string;
  sort: number;
  authority: string;
  roles: string;
  affixTab: boolean;
  isExternal: boolean;
  externalUrl: string;
  iframeUrl: string;
  keepAlive: boolean;
  menuType: number;
  badge: string;
  badgeType: string;
  badgeVariants: string;
  activeMenu: string;
  breadcrumbParentIcon: string;
  link: string;
  status: number;
}

export type SysMenuTreeSelectNode = SysMenuResp & {
  isLeaf?: boolean;
};

export function getDefaultSysMenuReq(parentId?: string): SysMenuReq {
  return {
    parentId,
    name: '',
    title: '',
    path: '',
    icon: '',
    component: '',
    authority: '',
    badge: '',
    badgeType: '',
    badgeVariants: 'default',
    sort: 0,
    status: 1,
    keepAlive: false,
    affixTab: false,
    isExternal: false,
    menuType: 0,
    redirect: '',
    roles: '',
    externalUrl: '',
    iframeUrl: '',
    activeMenu: '',
    breadcrumbParentIcon: '',
    link: '',
  };
}

export interface SysMenuImportButtonItemReq {
  controllerName: string;
  actionName: string;
  description: string;
}

export interface SysMenuApiEndpointResp {
  key: string;
  controllerTitle: string;
  controllerName: string;
  actionTitle: string;
  actionName: string;
  description: string;
}

export function toTreeSelectNodes(menus: SysMenuResp[]): SysMenuTreeSelectNode[] {
  return menus.map((menu) => {
    const node: SysMenuTreeSelectNode = { ...menu, isLeaf: !menu.hasChild };
    delete node.children;
    return node;
  });
}

export async function loadMenuTreeSelectChild(treeNode: SysMenuTreeSelectNode & { dataRef?: SysMenuTreeSelectNode }) {
  const node = treeNode.dataRef ?? treeNode;
  if (node.children?.length) return;

  const result = await getTreeNodesApi(node.id);
  const list = result.data;
  if (result.code !== 0 || list === undefined) {
    throw new Error(result.msg);
  }
  node.children = toTreeSelectNodes(list);
}

export function getListApi(): Promise<ApiResponse<SysMenuResp[]>> {
  return requestClient.get('/sysMenu/getList');
}

export function getTreeNodesApi(parentId: string): Promise<ApiResponse<SysMenuResp[]>> {
  const params = new URLSearchParams({ parentId });
  return requestClient.get(`/sysMenu/getTreeNodes?${params.toString()}`);
}

export function getButtonsApi(parentMenuId: string): Promise<ApiResponse<SysMenuResp[]>> {
  const params = new URLSearchParams({ parentMenuId });
  return requestClient.get(`/sysMenu/getButtons?${params.toString()}`);
}

export function getByIdApi(id: string): Promise<ApiResponse<SysMenuResp>> {
  return requestClient.get(`/sysMenu/getById/${id}`);
}

export function createApi(data: SysMenuReq): Promise<ApiResponse<void>> {
  return requestClient.post('/sysMenu/create', data);
}

export function editApi(id: string, data: SysMenuReq): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysMenu/edit/${id}`, data);
}

export function delApi(id: string): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysMenu/del/${id}`);
}

export function getApiEndpointsApi(): Promise<ApiResponse<SysMenuApiEndpointResp[]>> {
  return requestClient.get('/sysMenu/getApiEndpoints');
}

export function importButtonsApi(parentMenuId: string, items: SysMenuImportButtonItemReq[]): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysMenu/importButtons/${parentMenuId}`, items);
}
