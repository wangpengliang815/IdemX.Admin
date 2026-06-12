import type { ApiResponse } from '#/api/request';
import type { SysRoleResp } from '#/api/system/role';

import { requestClient } from '#/api/request';

export interface UserBriefResp {
  id: string;
  userName: string;
  nickName: string;
  phone: string;
}

export interface SysUserResp {
  id: string;
  userName: string;
  nickName?: string;
  avatar?: string;
  sex: number;
  phone?: string;
  email?: string;
  status: number;
  idCardNumber?: string;
  roles?: SysRoleResp[];
  createTime?: string;
  updateTime?: string;
}

export interface SysUserPageQueryReq {
  page?: number;
  pageSize?: number;
  userName?: string;
  status?: number;
  roleId?: string;
}

export interface SysUserReq {
  userName: string;
  phone: string;
  sex: number;
  /** 后端按逗号解析；当前业务一人一角色，仅传单个 id */
  roleIds?: string;
  password?: string;
}

/** 库表可多角色；产品约定一人一角色，提交单个 roleId */
export function toSingleRoleIds(roleId: string | undefined): string | undefined {
  if (!roleId) return undefined;
  return roleId;
}

export function getDefaultSysUserReq(): SysUserReq {
  return {
    userName: '',
    phone: '',
    sex: 0,
    roleIds: undefined,
    password: '',
  };
}

export function getPageListApi(params?: SysUserPageQueryReq): Promise<ApiResponse<SysUserResp[]>> {
  return requestClient.post('/sysUser/getPageList', params ?? {});
}

export function searchBriefApi(keyword: string): Promise<ApiResponse<UserBriefResp[]>> {
  return requestClient.get('/sysUser/searchBrief', { params: { keyword } });
}

export function getByIdApi(id: string): Promise<ApiResponse<SysUserResp>> {
  return requestClient.get(`/sysUser/getById/${id}`);
}

export function createApi(data: SysUserReq): Promise<ApiResponse<void>> {
  return requestClient.post('/sysUser/create', data);
}

export function editApi(id: string, data: SysUserReq): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysUser/edit/${id}`, data);
}

export function delApi(id: string): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysUser/del/${id}`);
}

export function setStatusApi(id: string, status: number): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysUser/setStatus/${id}`, status);
}
