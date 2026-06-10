import type { ApiResponse } from '#/api/request';

import { requestClient } from '#/api/request';

export interface LoginByPasswordReq {
  userName: string;
  password: string;
}

export interface LoginByPhoneReq {
  phoneNumber: string;
  smsCode: string;
}

export interface SmsSendCodeReq {
  phoneNumber: string;
  scene?: 'login' | 'register';
}

export interface SysUserRegReq {
  UserName: string;
  Password: string;
  Phone: string;
  RealName: string;
  SmsCode: string;
}

export interface ResetPasswordByPhoneReq {
  phoneNumber: string;
  smsCode: string;
  newPassword: string;
}

export interface CheckPhoneExistsReq {
  phoneNumber: string;
}

/** 用户名密码登录，成功时 data 为 JWT 字符串 */
export function loginByPasswordApi(data: LoginByPasswordReq): Promise<ApiResponse<string>> {
  return requestClient.post('/auth/loginByPassword', data);
}

/** 手机号验证码登录，成功时 data 为 JWT 字符串 */
export function loginByPhoneApi(data: LoginByPhoneReq): Promise<ApiResponse<string>> {
  return requestClient.post('/auth/loginByPhone', data);
}

/** 发送短信验证码，无业务 data */
export function sendSmsCodeApi(data: SmsSendCodeReq): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/sendSmsCode', data);
}

/** 忘记密码：向已注册手机号发送验证码 */
export function sendForgotPasswordSmsApi(data: Pick<SmsSendCodeReq, 'phoneNumber'>): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/sendForgotPasswordSms', data);
}

/** 通过手机短信验证码重置登录密码 */
export function resetPasswordByPhoneApi(data: ResetPasswordByPhoneReq): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/resetPasswordByPhone', data);
}

/** 用户注册，成功无 data */
export function registerApi(data: SysUserRegReq): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/register', data);
}

/** 退出登录 */
export function logoutApi(): Promise<ApiResponse<void>> {
  return requestClient.post('/auth/logout');
}

/** 检查手机号是否已注册 */
export function checkPhoneExistsApi(data: CheckPhoneExistsReq): Promise<ApiResponse<boolean>> {
  return requestClient.post('/auth/checkPhoneExists', data);
}
