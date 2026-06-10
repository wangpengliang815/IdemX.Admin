import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

export interface SysRecordLoginResp {
  id: string;
  userName: string;
  os: string;
  browser: string;
  operType: number;
  comments: string;
  loginSource: string;
  createTime?: string;
}

export interface SysRecordLoginPageQueryReq {
  page?: number;
  pageSize?: number;
  userName?: string;
  startTime?: string;
  endTime?: string;
}

export interface SysRecordNlogResp {
  id: number;
  logDate?: string;
  logLevel: string;
  logType: string;
  logTitle: string;
  logger: string;
  message: string;
  exception: string;
  machineName: string;
  machineIp: string;
  netRequestMethod: string;
  netRequestUrl: string;
  netUserIsauthenticated: string;
  netUserAuthtype: string;
  netUserIdentity: string;
}

export interface SysRecordNlogPageQueryReq {
  page?: number;
  pageSize?: number;
  logLevel?: string;
  startTime?: string;
  endTime?: string;
}

export function getLoginPageListApi(params?: SysRecordLoginPageQueryReq): Promise<ApiResponse<SysRecordLoginResp[]>> {
  return requestClient.post('/sysRecord/getLoginPageList', params ?? {});
}

export function clearLoginDataApi(): Promise<ApiResponse<void>> {
  return requestClient.post('/sysRecord/clearLoginData');
}

export function getNLogPageListApi(params?: SysRecordNlogPageQueryReq): Promise<ApiResponse<SysRecordNlogResp[]>> {
  return requestClient.post('/sysRecord/getNLogPageList', params ?? {});
}

export function clearNLogDataApi(): Promise<ApiResponse<void>> {
  return requestClient.post('/sysRecord/clearNLogData');
}
