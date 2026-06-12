import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

/** 与后端 SysAreaItemResp 对齐 */
export interface SysAreaItemResp {
  code: string;
  name: string;
  level: number;
  parentCode: string;
}

/** 与后端 AreaTreeNodeResp 对齐（camelCase） */
export interface AreaTreeNodeResp {
  label: string;
  value: string;
  children?: AreaTreeNodeResp[] | null;
}

/** 与后端 EnumOptionItemResp 对齐 */
export interface EnumOptionItemResp {
  value: number;
  label: string;
}

export type AllEnumsMap = Record<string, EnumOptionItemResp[]>;

export async function getEnums(): Promise<ApiResponse<AllEnumsMap>> {
  return requestClient.get('/tools/getEnum');
}

export async function getAreaByCode(params: { level: number; parentCode: string }): Promise<SysAreaItemResp[]> {
  const response = await requestClient.post<ApiResponse<SysAreaItemResp[]>>('/tools/getAreaByCode', params);
  if (response.code !== 0) throw new Error(response.msg);
  return response.data as SysAreaItemResp[];
}

export async function getAreaTree(): Promise<AreaTreeNodeResp[]> {
  const response = await requestClient.post<ApiResponse<AreaTreeNodeResp[]>>('/tools/getAreaTree');
  if (response.code !== 0) throw new Error(response.msg);
  return response.data as AreaTreeNodeResp[];
}
