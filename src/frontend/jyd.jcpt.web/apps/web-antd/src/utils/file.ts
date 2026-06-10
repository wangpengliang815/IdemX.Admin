import type { UploadFile } from 'ant-design-vue';

/**
 * 将 File 对象转换为 Base64 字符串
 */
export function getBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.addEventListener('load', () => {
      resolve(reader.result as string);
    });
    reader.addEventListener('error', () => {
      reject(new Error('文件读取失败'));
    });
  });
}

type UploadFileWithPreview = UploadFile & { preview?: string };

/**
 * 从 Upload 列表收集已上传或可展示的 http(s) 地址（优先 file.url，其次 onSuccess 写入的 response.url）。
 * 用于商品图等「先调上传接口再提交」的场景，不读本地文件、不生成 Base64。
 */
export function collectHttpImageUrlsFromUploadList(fileList: UploadFile[]): string[] {
  const urls: string[] = [];
  for (const file of fileList) {
    const res = file.response as { url?: string } | undefined;
    const candidate = (typeof file.url === 'string' ? file.url : res?.url ?? '').trim();
    if (candidate.startsWith('http://') || candidate.startsWith('https://'))
      urls.push(candidate);
  }
  return urls;
}

/**
 * 将 UploadFile[] 转为 URL 字符串数组（新文件取 base64，已有文件取 url 或 preview）
 */
export async function processFileList(fileList: UploadFile[]): Promise<string[]> {
  const urls: string[] = [];
  for (const file of fileList) {
    const raw = file as UploadFileWithPreview;
    const urlLike = raw.url ?? raw.preview;
    if (urlLike && (urlLike.startsWith('http://') || urlLike.startsWith('https://'))) {
      urls.push(urlLike);
      continue;
    }
    if (file.originFileObj) {
      urls.push(await getBase64(file.originFileObj));
      continue;
    }
    if (urlLike) urls.push(urlLike);
  }
  return urls;
}
