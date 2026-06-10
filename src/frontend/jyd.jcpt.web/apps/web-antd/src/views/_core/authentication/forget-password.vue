<script lang="ts" setup>
import { computed, onBeforeUnmount, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Button, Form, Input, message } from 'ant-design-vue';

import { resetPasswordByPhoneApi, sendForgotPasswordSmsApi } from '#/api/core/auth';

defineOptions({ name: 'ForgetPassword' });

const router = useRouter();

const loading = ref(false);
const countdown = ref(0);
let timer: NodeJS.Timeout | null = null;

const formData = ref({
  phone: '',
  smsCode: '',
  password: '',
  confirmPassword: '',
});

const rules = {
  phone: [
    { required: true, message: '请输入手机号' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号' },
  ],
  smsCode: [
    { required: true, message: '请输入短信验证码' },
    { len: 6, message: '验证码为6位数字' },
  ],
  password: [
    { required: true, message: '请输入新密码' },
    {
      validator: (_rule: any, value: string) => {
        if (!value) return Promise.resolve();
        if (value.length < 8) {
          return Promise.reject(new Error('密码至少需要8位字符'));
        }
        if (!/(?=.*[a-z])(?=.*\d)/i.test(value)) {
          return Promise.reject(new Error('密码必须包含字母和数字'));
        }
        return Promise.resolve();
      },
    },
  ],
  confirmPassword: [
    { required: true, message: '请再次输入新密码' },
    {
      validator: (_rule: any, value: string) => {
        if (!value) return Promise.resolve();
        if (value !== formData.value.password) {
          return Promise.reject(new Error('两次输入的密码不一致'));
        }
        return Promise.resolve();
      },
    },
  ],
};

const formRef = ref();

const sendCodeText = computed(() => {
  return countdown.value > 0 ? `${countdown.value}秒后重新获取` : '获取验证码';
});

const canSendCode = computed(() => {
  return countdown.value === 0 && /^1[3-9]\d{9}$/.test(formData.value.phone);
});

async function handleSendCode() {
  if (!canSendCode.value) return;

  try {
    await formRef.value?.validateFields(['phone']);

    const result = await sendForgotPasswordSmsApi({
      phoneNumber: formData.value.phone,
    });

    if (result.code === 0) {
      message.success(result.msg);
      startCountdown();
    } else {
      message.error(result.msg);
    }
  } catch (error: any) {
    if (error.errorFields) {
      message.error('请输入正确的手机号');
    }
  }
}

function startCountdown() {
  countdown.value = 60;
  timer = setInterval(() => {
    countdown.value--;
    if (countdown.value <= 0) {
      clearInterval(timer!);
      timer = null;
    }
  }, 1000);
}

async function handleSubmit() {
  try {
    await formRef.value?.validate();
  } catch {
    return;
  }

  loading.value = true;

  try {
    const result = await resetPasswordByPhoneApi({
      newPassword: formData.value.password,
      phoneNumber: formData.value.phone,
      smsCode: formData.value.smsCode,
    });

    if (result.code === 0) {
      message.success(result.msg);
      setTimeout(() => {
        router.push('/auth/login');
      }, 1500);
    } else {
      message.error(result.msg);
    }
  } catch (error: any) {
    if (error?.message) {
      message.error(error.message);
    }
  } finally {
    loading.value = false;
  }
}

function handleBackLogin() {
  router.push('/auth/login');
}

onBeforeUnmount(() => {
  if (timer) {
    clearInterval(timer);
  }
});
</script>

<template>
  <div class="forget-page">
    <div class="forget-form-wrapper">
      <div class="forget-form-title">
        <div class="title-row">
          <h3 class="title">忘记密码?</h3>
          <span class="title-emoji" aria-hidden="true">🤦</span>
        </div>
        <p class="subtitle">请输入绑定的手机号获取短信验证码，验证通过后可设置新密码。</p>
      </div>

      <Form ref="formRef" :model="formData" :rules="rules" class="forget-form" validate-trigger="">
        <Form.Item name="phone">
          <Input class="forget-phone-with-addon" v-model:value="formData.phone" size="large" placeholder="请输入手机号" autocomplete="tel">
            <template #prefix>
              <span class="input-icon">📱</span>
            </template>
            <template #addonAfter>
              <Button type="default" size="large" :disabled="!canSendCode" class="forget-phone-addon-btn" @click="handleSendCode">
                {{ sendCodeText }}
              </Button>
            </template>
          </Input>
        </Form.Item>

        <Form.Item name="smsCode">
          <Input v-model:value="formData.smsCode" size="large" placeholder="请输入短信验证码" :maxlength="6" autocomplete="one-time-code">
            <template #prefix>
              <span class="input-icon">✉️</span>
            </template>
          </Input>
        </Form.Item>

        <Form.Item name="password">
          <Input.Password
            v-model:value="formData.password"
            size="large"
            placeholder="请输入新密码（最少8位，包含字母和数字）"
            :maxlength="100"
            autocomplete="new-password"
          >
            <template #prefix>
              <span class="input-icon">🔒</span>
            </template>
          </Input.Password>
        </Form.Item>

        <Form.Item name="confirmPassword">
          <Input.Password v-model:value="formData.confirmPassword" size="large" placeholder="请再次输入新密码" :maxlength="100" autocomplete="new-password">
            <template #prefix>
              <span class="input-icon">🔒</span>
            </template>
          </Input.Password>
        </Form.Item>

        <Button type="primary" size="large" block :loading="loading" class="submit-btn" @click="handleSubmit"> 重置密码 </Button>
      </Form>

      <div class="form-footer">
        <span>想起密码了？</span>
        <a @click="handleBackLogin">返回登录</a>
      </div>
    </div>
  </div>
</template>

<style scoped lang="less">
.forget-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100%;
  padding: 20px;
}

.forget-form-wrapper {
  width: 100%;
  max-width: 420px;
}

.forget-form-title {
  margin-bottom: 28px;
  text-align: center;

  .title-row {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    margin-bottom: 12px;
  }

  .title {
    margin: 0;
    font-size: 24px;
    font-weight: 600;
    color: var(--ant-color-text);
  }

  .title-emoji {
    font-size: 26px;
    line-height: 1;
  }

  .subtitle {
    margin: 0;
    font-size: 14px;
    line-height: 1.6;
    color: var(--ant-color-text-secondary);
  }
}

.forget-form {
  :deep(.ant-form-item) {
    margin-bottom: 20px;
  }

  :deep(.ant-input-affix-wrapper) {
    padding: 12px 15px;
    font-size: 14px;
  }

  :deep(.ant-input) {
    font-size: 14px;
  }

  :deep(.ant-input-password) {
    padding: 12px 15px;
  }

  .input-icon {
    display: inline-block;
    margin-right: 8px;
    font-size: 18px;
    line-height: 1;
  }
}

.forget-form .forget-phone-with-addon {
  width: 100%;

  &:deep(.ant-input-wrapper.ant-input-group .ant-input-affix-wrapper) {
    vertical-align: middle;
  }

  &:deep(.ant-input-group-addon:last-child) {
    position: relative;
    width: 140px;
    padding: 0;
    overflow: visible;
    vertical-align: middle;
  }

  &:deep(.forget-phone-addon-btn.ant-btn) {
    position: absolute;
    top: -1px;
    right: 0;
    bottom: -1px;
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

  &:deep(.forget-phone-addon-btn.ant-btn:disabled),
  &:deep(.forget-phone-addon-btn.ant-btn.ant-btn-disabled) {
    color: rgba(255, 255, 255, 0.88);
    background: #91caff;
    border-color: #91caff;
  }
}

.submit-btn {
  height: 44px;
  margin-top: 8px;
  font-size: 16px;
  font-weight: 500;
  letter-spacing: 2px;
}

.form-footer {
  margin-top: 24px;
  text-align: center;
  font-size: 14px;
  color: var(--ant-color-text-secondary);

  a {
    margin-left: 4px;
    color: var(--ant-color-primary);
    font-weight: 500;
    cursor: pointer;

    &:hover {
      color: var(--ant-color-primary-hover);
    }
  }
}
</style>
