import type { EsignSignFlowFileDownloadUrlResp } from '#/api/esign/esign';

/** 是否可按 PDF 在弹窗 iframe 中预览 */
export function isLikelyPdfDownload(url: string, fileName?: null | string): boolean {
  const n = fileName?.trim().toLowerCase() ?? '';
  if (n.endsWith('.pdf')) return true;
  return url.trim().toLowerCase().includes('.pdf');
}

/** 主文件 → 附属材料 → 证书，取首个可弹窗预览的 PDF */
export function pickFirstPreviewableSignedDownloadItem(data: EsignSignFlowFileDownloadUrlResp): null | {
  fileName?: null | string;
  url: string;
} {
  const main = data.files;
  if (Array.isArray(main)) {
    for (const f of main) {
      const u = f.downloadUrl?.trim();
      if (u && isLikelyPdfDownload(u, f.fileName)) return { url: u, fileName: f.fileName };
    }
  }
  const atts = data.attachments;
  if (Array.isArray(atts)) {
    for (const a of atts) {
      const u = a.downloadUrl?.trim();
      if (u && isLikelyPdfDownload(u, a.fileName)) return { url: u, fileName: a.fileName };
    }
  }
  const cert = data.certificateDownloadUrl?.trim();
  if (cert && isLikelyPdfDownload(cert, null)) return { url: cert, fileName: '证书报告' };
  return null;
}
