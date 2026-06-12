import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

export interface SysRoleResp {
  id: string;
  roleName: string;
  roleCode: string;
  memo: string;
  createTime?: string;
  updateTime?: string;
}

export interface SysRoleReq {
  roleName: string;
  roleCode: string;
  memo?: string;
}

export interface SysRoleMenuMapTreeNodeResp {
  key: string;
  parentId?: string;
  title: string;
  menuType: number;
  sort: number;
  children: SysRoleMenuMapTreeNodeResp[];
}

export interface SysRoleMenuMapResp {
  treeData: SysRoleMenuMapTreeNodeResp[];
  checkedKeys: string[];
  halfCheckedKeys: string[];
}

export interface SysRolePageQueryReq {
  page?: number;
  pageSize?: number;
  roleName?: string;
  roleCode?: string;
}

export function getDefaultSysRoleReq(): SysRoleReq {
  return { roleName: '', roleCode: '', memo: '' };
}

export function getListApi(): Promise<ApiResponse<SysRoleResp[]>> {
  return requestClient.get('/sysRole/getList');
}

export function getPageListApi(params?: SysRolePageQueryReq): Promise<ApiResponse<SysRoleResp[]>> {
  return requestClient.post('/sysRole/getPageList', params ?? {});
}

export function getByIdApi(id: string): Promise<ApiResponse<SysRoleResp>> {
  return requestClient.get(`/sysRole/getById/${id}`);
}

export function createApi(data: SysRoleReq): Promise<ApiResponse<void>> {
  return requestClient.post('/sysRole/create', data);
}

export function editApi(id: string, data: SysRoleReq): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysRole/edit/${id}`, data);
}

export function delApi(id: string): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysRole/del/${id}`);
}

export function getRoleMenuMapApi(id: string): Promise<ApiResponse<SysRoleMenuMapResp>> {
  return requestClient.get(`/sysRole/getRoleMenuMap/${id}`);
}

export function setRoleMenuMapApi(id: string, menuIds: string[]): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysRole/setRoleMenuMap/${id}`, menuIds);
}
