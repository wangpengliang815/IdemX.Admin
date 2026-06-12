import type { RequestClientOptions } from '@vben/request';

import { LOGIN_PATH } from '@vben/constants';
import { useAppConfig } from '@vben/hooks';
import { preferences } from '@vben/preferences';
import { defaultResponseInterceptor, errorMessageResponseInterceptor, RequestClient } from '@vben/request';
import { useAccessStore } from '@vben/stores';

import { message } from 'ant-design-vue';

import { useAuthStore } from '#/store';

/**
 * 通用接口响应结构（与后端 CustomApiResponse 对齐）
 * code 0 成功，非 0 失败；data 为业务数据，total 分页时使用
 */
export interface ApiResponse<T = unknown> {
  code: number;
  msg: string;
  data?: T;
  total?: number;
}

const AUTH_CODES = new Set([1401, 1403]);

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);

function createRequestClient(baseURL: string, options?: RequestClientOptions) {
  const client = new RequestClient({
    ...options,
    baseURL,
  });

  let isShowingAuthError = false;

  async function doReAuthenticate() {
    const accessStore = useAccessStore();
    const authStore = useAuthStore();

    const currentPath = window.location.pathname;
    const isLoginPage = currentPath === LOGIN_PATH || currentPath.startsWith('/auth');

    accessStore.setAccessToken(null);

    if (isLoginPage) {
      return;
    }

    if (preferences.app.loginExpiredMode === 'modal' && accessStore.isAccessChecked) {
      accessStore.setLoginExpired(true);
    } else {
      await authStore.logout();
    }
  }

  function formatToken(token: null | string) {
    return token ? `Bearer ${token}` : null;
  }

  function getBizCode(error: unknown): number | undefined {
    const err = error as { response?: { data?: { code?: number } } };
    return err?.response?.data?.code;
  }

  client.addRequestInterceptor({
    fulfilled: async (config) => {
      const accessStore = useAccessStore();

      config.headers.Authorization = formatToken(accessStore.accessToken);
      config.headers['Accept-Language'] = preferences.app.locale;
      return config;
    },
  });

  // 鉴权/权限：仅认 HTTP 200 + 业务 code（与 ApiResponseHandler、生产异常中间件一致）
  client.addResponseInterceptor({
    fulfilled: (response) => {
      const responseData = response?.data ?? {};
      const code = responseData?.code;
      if (code === 1401 || code === 1403) {
        return Promise.reject(Object.assign(new Error(responseData?.msg ?? ''), { response, config: response.config }));
      }
      return response;
    },
  });

  client.addResponseInterceptor(
    defaultResponseInterceptor({
      codeField: 'code',
      dataField: 'data',
      successCode: 0,
    }),
  );

  client.addResponseInterceptor({
    rejected: async (error) => {
      const code = getBizCode(error);
      if (code === 1401) {
        if (!isShowingAuthError) {
          isShowingAuthError = true;
          const errorMessage = (error as { response?: { data?: { msg?: string } } })?.response?.data?.msg;
          if (errorMessage) {
            message.error(errorMessage);
          }
          await doReAuthenticate();
          isShowingAuthError = false;
        }
        throw error;
      }
      if (code === 1403) {
        const errorMessage = (error as { response?: { data?: { msg?: string } } })?.response?.data?.msg;
        if (errorMessage) {
          message.error(errorMessage);
        }
        throw error;
      }
      throw error;
    },
  });

  client.addResponseInterceptor(
    errorMessageResponseInterceptor((msg: string, error) => {
      const code = getBizCode(error);
      if (code !== undefined && AUTH_CODES.has(code)) {
        return;
      }
      const responseData = (error as { response?: { data?: { msg?: string; message?: string } } })?.response?.data ?? {};
      const errorMessage = responseData?.msg ?? responseData?.message ?? msg;
      message.error(errorMessage);
    }),
  );

  return client;
}

/**
 * 创建请求客户端
 * responseReturn 响应返回类型配置:
 * - 'raw': 返回完整的 axios 响应对象（包括 axios 包装）
 * - 'body': 返回响应体（后端返回的完整 JSON 对象）
 * - 'data': 返回后端响应中 dataField 指定的字段（默认是 data 字段）
 */
export const requestClient = createRequestClient(apiURL, {
  responseReturn: 'body',
});

/** multipart：request 为 JSON 字符串，files 为表单文件字段 */
export function postMultipartRequest<T = unknown>(url: string, request: unknown, files: Record<string, File[]>) {
  const formData = new FormData();
  formData.append('request', JSON.stringify(request));

  for (const [field, list] of Object.entries(files)) {
    for (const file of list) formData.append(field, file);
  }

  return requestClient.post<T>(url, formData, {
    transformRequest: [
      (body, headers) => {
        if (body instanceof FormData) delete (headers as Record<string, unknown>)['Content-Type'];
        return body;
      },
    ],
  });
}
