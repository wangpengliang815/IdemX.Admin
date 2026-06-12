import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

export interface LoginByPasswordReq {
  userName: string;
  password: string;
}

export interface SmsSendCodeReq {
  phoneNumber: string;
}

export interface ResetPasswordByPhoneReq {
  phoneNumber: string;
  smsCode: string;
  newPassword: string;
}

/** 用户名密码登录，成功时 data 为 JWT 字符串 */
export function loginByPasswordApi(data: LoginByPasswordReq): Promise<ApiResponse<string>> {
  return requestClient.post('/auth/loginByPassword', data);
}

/** 忘记密码：向已绑定手机号发送验证码 */
export function sendForgotPasswordSmsApi(data: Pick<SmsSendCodeReq, 'phoneNumber'>): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/sendForgotPasswordSms', data);
}

/** 通过手机短信验证码重置登录密码 */
export function resetPasswordByPhoneApi(data: ResetPasswordByPhoneReq): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/resetPasswordByPhone', data);
}

/** 退出登录 */
export function logoutApi(): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/logout');
}
