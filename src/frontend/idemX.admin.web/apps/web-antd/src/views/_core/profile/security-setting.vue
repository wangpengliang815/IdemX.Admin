<script setup lang="ts">
import type { FormInstance, Rule } from 'ant-design-vue/es/form';

import type { SysUserProfileInfo } from '#/api/profile';

import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue';

import { createIconifyIcon } from '@vben/icons';
import { useUserStore } from '@vben/stores';

import { Button, Card, Form, FormItem, Input, InputPassword, message, Modal, Spin } from 'ant-design-vue';

import { editUserPasswordApi, editUserPhoneApi, sendChangePhoneSmsApi, sendChangePhoneSmsToNewApi, verifyChangePhoneSmsApi } from '#/api/profile';
import { useAuthStore } from '#/store/auth';

const userStore = useUserStore();
const authStore = useAuthStore();

const loading = ref(false);
const userData = ref<null | SysUserProfileInfo>(null);
const passwordModalOpen = ref(false);

const passwordFormRef = ref<FormInstance>();
const passwordSubmitting = ref(false);
const passwordFormState = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
});
const phoneModalOpen = ref(false);
const phoneFormRef = ref<FormInstance>();
const phoneSubmitting = ref(false);
const phoneStep = ref<1 | 2>(1);
const phoneCodeSending = ref(false);
const phoneCodeVerifying = ref(false);
const phoneCountdown = ref(0);
let phoneCountdownTimer: null | ReturnType<typeof setInterval> = null;
const newPhoneCountdown = ref(0);
let newPhoneCountdownTimer: null | ReturnType<typeof setInterval> = null;
const newPhoneCodeSending = ref(false);
// 更换手机号：先验证旧手机号验证码，再输入新手机号；进入第二步后不提供返回上一步（需关闭弹窗后重新进入）。
const phoneFormState = reactive({
  smsCode: '',
  phone: '',
  confirmPhone: '',
  newPhoneSmsCode: '',
});

const IconLock = createIconifyIcon('mdi:lock-outline');
const IconPhone = createIconifyIcon('mdi:cellphone');
const IconLockPlus = createIconifyIcon('mdi:lock-plus');
const IconLockCheck = createIconifyIcon('mdi:lock-check');
const IconMessage = createIconifyIcon('mdi:message-text-outline');

const passwordRules: Record<string, Rule[]> = {
  oldPassword: [{ required: true, message: '请输入旧密码', trigger: 'blur' }],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    {
      validator: async (_rule, value: string) => {
        if (!value) return;
        if (value.length < 8) throw new Error('密码必须至少8位');
        if (!/^(?=.*[a-z])(?=.*\d)[a-z\d]{8,}$/i.test(value)) {
          throw new Error('密码必须至少8位，且包含字母和数字');
        }
      },
      trigger: 'blur',
    },
  ],
  confirmPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    {
      validator: async (_rule, value: string) => {
        if (!value) return;
        if (value !== passwordFormState.newPassword) {
          throw new Error('两次输入的密码不一致');
        }
      },
      trigger: 'blur',
    },
  ],
};

const phoneRules: Record<string, Rule[]> = {
  phone: [
    { required: true, message: '请输入新手机号', trigger: 'blur' },
    {
      validator: async (_rule, value: string) => {
        if (!value) return;
        if (!/^1[3-9]\d{9}$/.test(value)) {
          throw new Error('请输入有效的手机号');
        }
        if (value === userData.value?.phone) {
          throw new Error('新手机号不能与当前手机号一致');
        }
      },
      trigger: 'blur',
    },
  ],
  confirmPhone: [
    { required: true, message: '请再次输入新手机号', trigger: 'blur' },
    {
      validator: async (_rule, value: string) => {
        if (!value) return;
        if (value !== phoneFormState.phone) {
          throw new Error('两次输入的手机号不一致');
        }
      },
      trigger: 'blur',
    },
  ],
};

const maskedPhone = computed(() => {
  const phone = userData.value?.phone;
  if (!phone) return '未绑定';
  const s = String(phone);
  if (s.length >= 11) return `${s.slice(0, 3)}****${s.slice(-4)}`;
  return s;
});

const phoneDesc = computed(() => {
  return maskedPhone.value === '未绑定' ? '当前绑定：未绑定' : `当前绑定：${maskedPhone.value}`;
});

function resetPasswordForm() {
  passwordFormState.oldPassword = '';
  passwordFormState.newPassword = '';
  passwordFormState.confirmPassword = '';
  passwordFormRef.value?.resetFields();
}

async function openPasswordModal() {
  passwordModalOpen.value = true;
  await nextTick();
  resetPasswordForm();
}

function changePhone() {
  if (!userData.value?.phone) {
    message.error('当前账号未绑定手机号');
    return;
  }
  resetPhoneForm();
  phoneModalOpen.value = true;
}

function resetPhoneForm() {
  phoneStep.value = 1;
  phoneFormState.smsCode = '';
  phoneFormState.phone = '';
  phoneFormState.confirmPhone = '';
  phoneFormState.newPhoneSmsCode = '';
  phoneFormRef.value?.resetFields();
  stopNewPhoneCountdown();
}

function stopPhoneCountdown() {
  if (phoneCountdownTimer) {
    clearInterval(phoneCountdownTimer);
    phoneCountdownTimer = null;
  }
  phoneCountdown.value = 0;
}

function stopNewPhoneCountdown() {
  if (newPhoneCountdownTimer) {
    clearInterval(newPhoneCountdownTimer);
    newPhoneCountdownTimer = null;
  }
  newPhoneCountdown.value = 0;
}

function startPhoneCountdown() {
  stopPhoneCountdown();
  phoneCountdown.value = 60;
  // 前端仅负责发送按钮的 60 秒节流展示，真正的频控仍以后端短信服务为准。
  phoneCountdownTimer = setInterval(() => {
    phoneCountdown.value--;
    if (phoneCountdown.value <= 0) {
      stopPhoneCountdown();
    }
  }, 1000);
}

function closePhoneModal() {
  phoneModalOpen.value = false;
  stopPhoneCountdown();
  stopNewPhoneCountdown();
  resetPhoneForm();
}

const phoneCodeText = computed(() => {
  return phoneCountdown.value > 0 ? `${phoneCountdown.value}秒后重新获取` : '获取验证码';
});

const newPhoneCodeText = computed(() => {
  return newPhoneCountdown.value > 0 ? `${newPhoneCountdown.value}秒后重新获取` : '获取验证码';
});

async function handleSendPhoneCode() {
  if (phoneCodeSending.value || phoneCountdown.value > 0) {
    return;
  }

  phoneCodeSending.value = true;
  try {
    // 第一步：向当前已绑定手机号发码，先确认本人仍能控制旧手机号。
    const result = await sendChangePhoneSmsApi();
    if (result.code === 0) {
      message.success(result.msg);
      startPhoneCountdown();
    } else {
      message.error(result.msg);
    }
  } finally {
    phoneCodeSending.value = false;
  }
}

function startNewPhoneCountdown() {
  stopNewPhoneCountdown();
  newPhoneCountdown.value = 60;
  newPhoneCountdownTimer = setInterval(() => {
    newPhoneCountdown.value--;
    if (newPhoneCountdown.value <= 0) {
      stopNewPhoneCountdown();
    }
  }, 1000);
}

async function handleSendNewPhoneCode() {
  if (newPhoneCodeSending.value || newPhoneCountdown.value > 0) return;

  const p = (phoneFormState.phone ?? '').trim();
  const c = (phoneFormState.confirmPhone ?? '').trim();
  if (!/^1[3-9]\d{9}$/.test(p)) {
    message.error('请先输入有效的新手机号');
    return;
  }
  if (p !== c) {
    message.error('两次输入的新手机号须一致后再获取验证码');
    return;
  }

  newPhoneCodeSending.value = true;
  try {
    const result = await sendChangePhoneSmsToNewApi({ phone: p });
    if (result.code === 0) {
      message.success(result.msg);
      startNewPhoneCountdown();
    } else {
      message.error(result.msg);
    }
  } finally {
    newPhoneCodeSending.value = false;
  }
}

async function handleVerifyPhoneCode() {
  const code = (phoneFormState.smsCode ?? '').trim();
  if (!code) {
    message.error('请输入验证码');
    return;
  }
  if (code.length !== 6) {
    message.error('验证码为6位数字');
    return;
  }

  phoneCodeVerifying.value = true;
  try {
    // 仅在点击「下一步」时校验格式并请求后端；最终换绑提交时后端会再次核销验证码。
    const result = await verifyChangePhoneSmsApi({
      smsCode: code,
    });
    if (result.code === 0) {
      message.success(result.msg);
      phoneStep.value = 2;
    } else {
      message.error(result.msg);
    }
  } finally {
    phoneCodeVerifying.value = false;
  }
}

async function handlePhoneSubmit() {
  try {
    await phoneFormRef.value?.validateFields(['phone', 'confirmPhone']);
  } catch {
    return;
  }

  const newSms = (phoneFormState.newPhoneSmsCode ?? '').trim();
  if (!newSms) {
    message.error('请输入新手机号收到的验证码');
    return;
  }
  if (newSms.length !== 6) {
    message.error('新手机号验证码为6位数字');
    return;
  }

  phoneSubmitting.value = true;
  try {
    // 第二步：旧号验证码 + 新号验证码一并提交，后端依次核销并写库。
    const result = await editUserPhoneApi({
      phone: phoneFormState.phone.trim(),
      smsCode: phoneFormState.smsCode.trim(),
      newPhoneSmsCode: newSms,
    });
    if (result.code === 0) {
      message.success(result.msg);
      const refreshed = await authStore.getUserInfo({ force: true });
      if (refreshed) {
        userData.value = refreshed as unknown as SysUserProfileInfo;
      } else if (userData.value) {
        userData.value.phone = phoneFormState.phone;
      }
      closePhoneModal();
    } else {
      message.error(result.msg);
    }
  } finally {
    phoneSubmitting.value = false;
  }
}

function closePasswordModal() {
  passwordModalOpen.value = false;
  resetPasswordForm();
}

async function handlePasswordSubmit() {
  const valid = await passwordFormRef.value?.validate().then(
    () => true,
    () => false,
  );
  if (!valid) return;

  passwordSubmitting.value = true;
  try {
    const result = await editUserPasswordApi({
      oldPassword: passwordFormState.oldPassword,
      password: passwordFormState.newPassword,
    });

    if (result.code === 0) {
      if (result.msg) message.success(result.msg);
      closePasswordModal();
      setTimeout(async () => {
        await authStore.logout();
      }, 1500);
    } else {
      message.error(result.msg || '修改失败');
    }
  } finally {
    passwordSubmitting.value = false;
  }
}

onMounted(async () => {
  if (userStore.userInfo) {
    userData.value = userStore.userInfo as unknown as SysUserProfileInfo;
    return;
  }

  loading.value = true;
  try {
    const data = await authStore.getUserInfo();
    if (data) {
      userData.value = data as unknown as SysUserProfileInfo;
    } else {
      message.error('获取用户信息失败');
    }
  } finally {
    loading.value = false;
  }
});

onBeforeUnmount(() => {
  stopPhoneCountdown();
  stopNewPhoneCountdown();
});
</script>
<template>
  <Spin :spinning="loading">
    <div class="p-3 md:p-5">
      <div class="space-y-4">
        <Card :bordered="false" :body-style="{ padding: '20px' }">
          <div class="flex items-center justify-between gap-4">
            <div class="flex items-center gap-4">
              <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-50 text-blue-600">
                <component :is="IconLock" class="text-xl" />
              </div>
              <div class="min-w-0">
                <div class="text-base font-semibold text-gray-900">登录密码</div>
                <div class="mt-1 text-sm text-gray-500">定期更换密码有助于保护账号安全</div>
              </div>
            </div>
            <Button size="middle" @click="openPasswordModal">修改密码</Button>
          </div>
        </Card>

        <Card :bordered="false" :body-style="{ padding: '20px' }">
          <div class="flex items-center justify-between gap-4">
            <div class="flex items-center gap-4">
              <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-green-50 text-green-600">
                <component :is="IconPhone" class="text-xl" />
              </div>
              <div class="min-w-0">
                <div class="text-base font-semibold text-gray-900">绑定手机</div>
                <div class="mt-1 text-sm text-gray-500">
                  {{ phoneDesc }}
                </div>
              </div>
            </div>
            <Button size="middle" @click="changePhone">更换手机</Button>
          </div>
        </Card>
      </div>

      <Modal :open="passwordModalOpen" centered width="600px" :footer="null" destroy-on-close @cancel="closePasswordModal">
        <template #title>修改密码</template>
        <div class="pt-2">
          <div class="w-full max-w-[560px]">
            <Form ref="passwordFormRef" :model="passwordFormState" :rules="passwordRules" :label-col="{ span: 5 }" :wrapper-col="{ span: 19 }">
              <FormItem label="旧密码" name="oldPassword">
                <InputPassword v-model:value="passwordFormState.oldPassword" placeholder="请输入旧密码" :maxlength="100" allow-clear>
                  <template #prefix>
                    <component :is="IconLock" />
                  </template>
                </InputPassword>
              </FormItem>

              <FormItem label="新密码" name="newPassword">
                <InputPassword v-model:value="passwordFormState.newPassword" placeholder="请输入新密码（至少8位，包含字母和数字）" :maxlength="100" allow-clear>
                  <template #prefix>
                    <component :is="IconLockPlus" />
                  </template>
                </InputPassword>
              </FormItem>

              <FormItem label="确认密码" name="confirmPassword">
                <InputPassword v-model:value="passwordFormState.confirmPassword" placeholder="请再次输入新密码" :maxlength="100" allow-clear>
                  <template #prefix>
                    <component :is="IconLockCheck" />
                  </template>
                </InputPassword>
              </FormItem>

              <FormItem :wrapper-col="{ offset: 5, span: 19 }">
                <div class="flex justify-end">
                  <Button type="primary" :loading="passwordSubmitting" @click="handlePasswordSubmit"> 更新密码 </Button>
                </div>
              </FormItem>
            </Form>
          </div>
        </div>
      </Modal>

      <Modal :open="phoneModalOpen" centered width="600px" :footer="null" destroy-on-close @cancel="closePhoneModal">
        <template #title>更换手机号</template>
        <div class="pt-2">
          <div class="w-full max-w-[560px]">
            <Form ref="phoneFormRef" :model="phoneFormState" :rules="phoneRules" :label-col="{ span: 5 }" :wrapper-col="{ span: 19 }">
              <FormItem label="当前手机号">
                <Input class="change-phone-row-input" :value="maskedPhone" disabled />
              </FormItem>

              <template v-if="phoneStep === 1">
                <FormItem label="验证码" name="smsCode">
                  <Input
                    v-model:value="phoneFormState.smsCode"
                    class="change-phone-sms-with-addon"
                    placeholder="请输入当前手机号收到的验证码"
                    :maxlength="6"
                    allow-clear
                  >
                    <template #prefix>
                      <component :is="IconMessage" />
                    </template>
                    <template #addonAfter>
                      <Button
                        type="default"
                        html-type="button"
                        class="change-phone-sms-addon-btn"
                        :disabled="phoneCountdown > 0"
                        :loading="phoneCodeSending"
                        @click.stop="handleSendPhoneCode"
                      >
                        {{ phoneCodeText }}
                      </Button>
                    </template>
                  </Input>
                </FormItem>

                <FormItem :wrapper-col="{ offset: 5, span: 19 }">
                  <div class="text-xs text-gray-500">请先验证当前绑定手机号，再绑定新手机号。验证码 10 分钟内有效。</div>
                  <div class="mt-4 flex justify-end">
                    <Button type="primary" :loading="phoneCodeVerifying" @click="handleVerifyPhoneCode"> 下一步 </Button>
                  </div>
                </FormItem>
              </template>

              <template v-else>
                <FormItem label="新手机号" name="phone">
                  <Input v-model:value="phoneFormState.phone" class="change-phone-row-input" placeholder="请输入新手机号" :maxlength="11" allow-clear />
                </FormItem>

                <FormItem label="确认手机号" name="confirmPhone">
                  <Input
                    v-model:value="phoneFormState.confirmPhone"
                    class="change-phone-row-input"
                    placeholder="请再次输入新手机号"
                    :maxlength="11"
                    allow-clear
                  />
                </FormItem>

                <FormItem label="新号验证码" name="newPhoneSmsCode">
                  <Input
                    v-model:value="phoneFormState.newPhoneSmsCode"
                    class="change-phone-sms-with-addon"
                    placeholder="请输入新手机号收到的验证码"
                    :maxlength="6"
                    allow-clear
                  >
                    <template #prefix>
                      <component :is="IconMessage" />
                    </template>
                    <template #addonAfter>
                      <Button
                        type="default"
                        html-type="button"
                        class="change-phone-sms-addon-btn"
                        :disabled="newPhoneCountdown > 0"
                        :loading="newPhoneCodeSending"
                        @click.stop="handleSendNewPhoneCode"
                      >
                        {{ newPhoneCodeText }}
                      </Button>
                    </template>
                  </Input>
                </FormItem>

                <FormItem :wrapper-col="{ offset: 5, span: 19 }">
                  <div class="text-xs text-gray-500">请先保证新、旧手机号两处验证码均在有效期内；提交后将依次核销并完成换绑。</div>
                  <div class="mt-4 flex justify-end">
                    <Button type="primary" :loading="phoneSubmitting" @click="handlePhoneSubmit"> 更新手机号 </Button>
                  </div>
                </FormItem>
              </template>
            </Form>
          </div>
        </div>
      </Modal>
    </div>
  </Spin>
</template>

<style scoped lang="less">
/* 弹窗内各步输入区与表单项内容区等宽 */
.change-phone-row-input {
  width: 100%;
}

/* 与注册页手机号「获取验证码」addon 对齐；并让 input-group 横向占满，避免比上一行单独 Input 显窄 */
.change-phone-sms-with-addon {
  width: 100%;
  max-width: 100%;

  &:deep(.ant-input-wrapper.ant-input-group) {
    display: flex;
    width: 100%;
    align-items: stretch;
  }

  &:deep(.ant-input-wrapper.ant-input-group > .ant-input-affix-wrapper) {
    flex: 1 1 auto;
    min-width: 0;
  }

  &:deep(.ant-input-wrapper.ant-input-group .ant-input-affix-wrapper) {
    vertical-align: middle;
  }

  &:deep(.ant-input-group-addon:last-child) {
    position: relative;
    flex: 0 0 140px;
    width: 140px;
    max-width: 140px;
    padding: 0;
    overflow: visible;
    vertical-align: middle;
  }

  &:deep(.change-phone-sms-addon-btn.ant-btn) {
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    width: 100%;
    height: auto;
    margin: 0;
    border-top-left-radius: 0;
    border-bottom-left-radius: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #fff;
    background: #1677ff;
    border-color: #1677ff;

    &:not(:disabled):hover {
      color: #fff;
      background: #0958d9;
      border-color: #0958d9;
    }
  }

  &:deep(.change-phone-sms-addon-btn.ant-btn:disabled),
  &:deep(.change-phone-sms-addon-btn.ant-btn.ant-btn-disabled) {
    color: rgba(255, 255, 255, 0.88);
    background: #91caff;
    border-color: #91caff;
  }
}
</style>
