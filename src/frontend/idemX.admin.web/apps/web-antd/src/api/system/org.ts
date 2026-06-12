import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

export interface SysOrgResp {
  id: string;
  name: string;
  parentId?: string;
  hasChild?: boolean;
  children?: SysOrgResp[];
  sort: number;
  createTime?: string;
  updateTime?: string;
  createBy?: string;
}

export interface SysOrgReq {
  name: string;
  parentId?: string;
  sort: number;
}

export type SysOrgTreeSelectNode = SysOrgResp & {
  isLeaf?: boolean;
};

export function getDefaultSysOrgReq(): SysOrgReq {
  return {
    name: '',
    parentId: undefined,
    sort: 0,
  };
}

export function toTreeSelectNodes(orgs: SysOrgResp[]): SysOrgTreeSelectNode[] {
  return orgs.map((org) => {
    const node: SysOrgTreeSelectNode = { ...org, isLeaf: !org.hasChild };
    delete node.children;
    return node;
  });
}

export async function loadOrgTreeSelectChild(treeNode: SysOrgTreeSelectNode & { dataRef?: SysOrgTreeSelectNode }) {
  const node = treeNode.dataRef ?? treeNode;
  if (node.children?.length) return;

  const result = await getTreeNodesApi(node.id);
  const list = result.data;
  if (result.code !== 0 || list === undefined) {
    throw new Error(result.msg);
  }
  node.children = toTreeSelectNodes(list);
}

export function getListApi(): Promise<ApiResponse<SysOrgResp[]>> {
  return requestClient.get('/sysOrg/getList');
}

export function getTreeNodesApi(parentId: string): Promise<ApiResponse<SysOrgResp[]>> {
  const params = new URLSearchParams({ parentId });
  return requestClient.get(`/sysOrg/getTreeNodes?${params.toString()}`);
}

export function getByIdApi(id: string): Promise<ApiResponse<SysOrgResp>> {
  return requestClient.get(`/sysOrg/getById/${id}`);
}

export function createApi(data: SysOrgReq): Promise<ApiResponse<void>> {
  return requestClient.post('/sysOrg/create', data);
}

export function editApi(id: string, data: SysOrgReq): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysOrg/edit/${id}`, data);
}

export function delApi(id: string): Promise<ApiResponse<void>> {
  return requestClient.post(`/sysOrg/del/${id}`);
}
