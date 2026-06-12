import type { ApiResponse } from '#/api/request';
import type { SysMenuResp } from '#/api/system/menu';
import type { SysUserResp } from '#/api/system/user';

import { requestClient } from '#/api/request';

/** 当前登录用户详情（getUserInfo 返回，含角色与管理员标记） */
export type SysUserProfileInfo = SysUserResp & {
  isAdmin: boolean;
  sysOrgId?: string;
  sysOrgName?: string;
};

export interface SysUserEditInfoReq {
  sex: number;
  nickName: string;
  email: string;
}

export interface UserEditPasswordReq {
  oldPassword: string;
  password: string;
}

export interface UserEditPhoneReq {
  phone: string;
  smsCode: string;
  newPhoneSmsCode: string;
}

export interface UserSendChangePhoneNewSmsReq {
  phone: string;
}

export interface UserVerifyChangePhoneReq {
  smsCode: string;
}

/** 获取当前登录用户详情 */
export function getUserInfoApi(): Promise<ApiResponse<SysUserProfileInfo>> {
  return requestClient.get('/userProfile/getUserInfo');
}

/** 按角色编码获取可见菜单树 */
export function getMenusApi(roleCode: string): Promise<ApiResponse<SysMenuResp[]>> {
  return requestClient.get('/userProfile/getMenus', { params: { roleCode } });
}

/** 上传头像（multipart 字段 file） */
export function uploadAvatarApi(file: Blob): Promise<ApiResponse<boolean>> {
  const formData = new FormData();
  formData.append('file', file);

  return requestClient.post('/userProfile/uploadAvatar', formData, {
    transformRequest: [
      (body, headers) => {
        if (body instanceof FormData) delete (headers as Record<string, unknown>)['Content-Type'];
        return body;
      },
    ],
  });
}

/** 编辑当前用户基本资料 */
export function editUserInfoApi(id: string, data: SysUserEditInfoReq): Promise<ApiResponse<boolean>> {
  return requestClient.post(`/userProfile/editUserInfo/${id}`, data);
}

/** 修改登录密码 */
export function editUserPasswordApi(data: UserEditPasswordReq): Promise<ApiResponse<boolean>> {
  return requestClient.post('/userProfile/editUserPassword', data);
}

/** 向当前绑定手机号发送换绑验证码 */
export function sendChangePhoneSmsApi(): Promise<ApiResponse<void>> {
  return requestClient.post('/userProfile/sendChangePhoneSms');
}

/** 向换绑目标手机号发送验证码 */
export function sendChangePhoneSmsToNewApi(data: UserSendChangePhoneNewSmsReq): Promise<ApiResponse<void>> {
  return requestClient.post('/userProfile/sendChangePhoneSmsToNew', data);
}

/** 校验当前绑定手机号收到的换绑验证码 */
export function verifyChangePhoneSmsApi(data: UserVerifyChangePhoneReq): Promise<ApiResponse<void>> {
  return requestClient.post('/userProfile/verifyChangePhoneSms', data);
}

/** 更换绑定手机号 */
export function editUserPhoneApi(data: UserEditPhoneReq): Promise<ApiResponse<void>> {
  return requestClient.post('/userProfile/editUserPhone', data);
}
