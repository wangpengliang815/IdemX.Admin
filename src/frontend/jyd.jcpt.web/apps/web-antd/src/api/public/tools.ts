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

/** 工商查询载荷（阿里云返回体子集，供页面预填） */
export interface BusinessInfoPayload {
  creditCode?: string;
  dom?: string;
  entname?: string;
  entStatusNameBi?: string;
  frname?: string;
  opscope?: string;
}

/** 与后端 BusinessQueryResponse 对齐（camelCase） */
export interface BusinessQueryPayload {
  code?: number;
  msg?: string;
  taskNo?: string;
  success?: boolean;
  data?: BusinessInfoPayload;
}

/** 与后端 ParticipantConfigOptions 对齐 */
export interface ParticipantConfigResp {
  participantType: number;
  name: string;
  defaultAllotPercentage: number;
  dutyDescription: string;
  orderBy: number;
}

/** 与后端 LogisticsCarrierResp 对齐 */
export interface LogisticsCarrierResp {
  code: string;
  name: string;
}

export async function getEnums(): Promise<ApiResponse<AllEnumsMap>> {
  return requestClient.get('/tools/getEnum');
}

export async function getAreaByCode(params: { level: number; parentCode: string }): Promise<SysAreaItemResp[]> {
  const response = await requestClient.post<ApiResponse<SysAreaItemResp[]>>('/tools/getAreaByCode', params);
  if (response.code !== 0) throw new Error(response.msg);
  return response.data as SysAreaItemResp[];
}

export async function getBusinessInfoApi(creditCode: string): Promise<ApiResponse<BusinessQueryPayload>> {
  return requestClient.post<ApiResponse<BusinessQueryPayload>>('/tools/getBusinessInfo', {
    data: creditCode,
  });
}

export async function getParticipantConfigApi(): Promise<ApiResponse<ParticipantConfigResp[]>> {
  return requestClient.get('/tools/getParticipantConfig');
}

export function getLogisticsCarrierListApi(): Promise<ApiResponse<LogisticsCarrierResp[]>> {
  return requestClient.get('/tools/getLogisticsCarrierList');
}

export type LogisticsCarrier = LogisticsCarrierResp;

export interface LogisticsCarrierOption {
  label: string;
  value: string;
}

let logisticsCarriersCache: LogisticsCarrier[] | null = null;

/** 快递100 国内运输商（带前端内存缓存，与后端 IMemoryCache 互补） */
export async function loadLogisticsCarriers(): Promise<LogisticsCarrier[]> {
  if (logisticsCarriersCache) return logisticsCarriersCache;
  const res = await getLogisticsCarrierListApi();
  if (res.code !== 0) throw new Error(res.msg);
  if (!res.data) throw new Error(res.msg);
  logisticsCarriersCache = res.data;
  return logisticsCarriersCache;
}

export function toLogisticsCarrierOptions(carriers: LogisticsCarrier[]): LogisticsCarrierOption[] {
  return carriers.map((item) => ({
    label: item.name,
    value: item.code,
  }));
}

export function getLogisticsCarrierName(carriers: LogisticsCarrier[], code: string): string | undefined {
  return carriers.find((item) => item.code === code)?.name;
}

export function filterLogisticsCarrier(input: string, option?: { label?: unknown; value?: unknown }): boolean {
  const q = input.trim().toLowerCase();
  if (!q) return true;
  const label = String(option?.label ?? '').toLowerCase();
  const value = String(option?.value ?? '').toLowerCase();
  return label.includes(q) || value.includes(q);
}

/** 与后端 LogisticsTrackQueryReq 对齐 */
export interface LogisticsTrackQueryReq {
  companyCode: string;
  trackingNo: string;
  phone?: string;
}

/** 与后端 LogisticsTrackNodeResp 对齐 */
export interface LogisticsTrackNodeResp {
  time: string;
  context: string;
  status?: string;
}

/** 与后端 LogisticsTrackResp 对齐 */
export interface LogisticsTrackResp {
  companyCode: string;
  companyName?: string;
  trackingNo: string;
  state?: string;
  stateLabel?: string;
  tracks: LogisticsTrackNodeResp[];
}

/** 与后端 ConfirmShipmentReq 物流字段校验一致，查询前拦截脏数据 */
export function isValidLogisticsCompanyCode(code: string): boolean {
  const c = code.trim().toLowerCase();
  if (!c || c.length > 32) return false;
  return /^[a-z0-9-]+$/.test(c);
}

export function isValidLogisticsTrackingNo(raw: string): boolean {
  const no = raw.replaceAll(/\s/g, '');
  if (no.length < 6 || no.length > 50) return false;
  if (!/^[\da-z-]+$/i.test(no)) return false;
  if (no.startsWith('-') || no.endsWith('-')) return false;
  return true;
}

/** 与后端 TrackCacheMinutes 默认一致（≥ 快递100 文档要求的 30 分钟） */
const LOGISTICS_TRACK_CACHE_MS = 40 * 60 * 1000;

const logisticsTrackCache = new Map<string, { expireAt: number; response: ApiResponse<LogisticsTrackResp> }>();

function buildLogisticsTrackCacheKey(data: LogisticsTrackQueryReq): string {
  return `${data.companyCode.trim().toLowerCase()}|${data.trackingNo.trim()}|${(data.phone ?? '').trim()}`;
}

export async function getLogisticsTrackApi(data: LogisticsTrackQueryReq): Promise<ApiResponse<LogisticsTrackResp>> {
  const key = buildLogisticsTrackCacheKey(data);
  const hit = logisticsTrackCache.get(key);
  if (hit && hit.expireAt > Date.now()) return hit.response;

  const response = await requestClient.post<ApiResponse<LogisticsTrackResp>>('/tools/getLogisticsTrack', data);
  if (response.code === 0 && response.data) {
    logisticsTrackCache.set(key, { expireAt: Date.now() + LOGISTICS_TRACK_CACHE_MS, response });
  }

  return response;
}
