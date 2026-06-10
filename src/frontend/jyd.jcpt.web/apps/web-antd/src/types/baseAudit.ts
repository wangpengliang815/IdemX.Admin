/*
统一审核模型
*/
export interface BaseAuditReq {
  id: string;
  auditRemark: string;
  isApproved: boolean;
}
