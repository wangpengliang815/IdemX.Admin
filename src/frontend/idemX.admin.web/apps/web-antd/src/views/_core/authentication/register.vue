<script lang="ts" setup>
import { computed, onBeforeUnmount, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Button, Checkbox, Form, Input, message, Modal } from 'ant-design-vue';

import { checkPhoneExistsApi, registerApi, sendSmsCodeApi } from '#/api/core/auth';

import agreementRaw from './user-register-agreement.txt?raw';

defineOptions({ name: 'Register' });

const router = useRouter();

// 注册接口请求的 loading 状态
const loading = ref(false);

// 短信验证码倒计时（秒）
const countdown = ref(0);
let timer: NodeJS.Timeout | null = null;
const agreementVisible = ref(false);

/** 用户注册协议（正文见 user-register-agreement.txt，按空行分段展示） */
const agreementBlocks = computed(() =>
  agreementRaw
    .split(/\n\s*\n/)
    .map((s) => s.trim())
    .filter(Boolean),
);

function agreementBlockClass(text: string, index: number) {
  if (index === 0) return 'agreement-article-title';
  if (index === 1 || index === 2) return 'agreement-article-meta';
  if (text.length <= 22 && !/[。.；：，]/.test(text)) return 'agreement-article-heading';
  return 'agreement-article-paragraph';
}

const formData = ref({
  username: '',
  password: '',
  confirmPassword: '',
  phone: '',
  smsCode: '',
  agreePolicy: false,
});

const rules = {
  username: [
    { required: true, message: '请输入用户名' },
    {
      validator: (_rule: any, value: string) => {
        if (!value) return Promise.resolve();
        if (value.length < 5) {
          return Promise.reject(new Error('用户名至少需要5位字符'));
        }
        if (!/^\w+$/.test(value)) {
          return Promise.reject(new Error('用户名只能包含英文、数字和下划线'));
        }
        return Promise.resolve();
      },
    },
  ],
  password: [
    { required: true, message: '请输入登录密码' },
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
    { required: true, message: '请确认登录密码' },
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
  phone: [
    { required: true, message: '请输入手机号' },
    {
      pattern: /^1[3-9]\d{9}$/,
      message: '请输入正确的手机号',
    },
  ],
  smsCode: [{ required: true, message: '请输入短信验证码' }],
  agreePolicy: [
    {
      validator: (_rule: any, value: boolean) => {
        if (!value) {
          return Promise.reject(new Error('请阅读并同意《用户注册协议》'));
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

/**
 * 发送注册短信验证码
 * 已注册的手机号会提示直接登录并跳转到登录页面
 */
async function handleSendCode() {
  if (!canSendCode.value) return;

  try {
    await formRef.value?.validateFields(['phone']);

    const checkResult = await checkPhoneExistsApi({
      phoneNumber: formData.value.phone,
    });

    if (checkResult.code !== 0) {
      message.error(checkResult.msg || '手机号校验失败，请稍后重试');
      return;
    }

    if (checkResult.data === true) {
      message.warning('该手机号已注册，请直接登录');
      setTimeout(() => {
        router.push('/auth/login');
      }, 1500);
      return;
    }

    const result = await sendSmsCodeApi({
      phoneNumber: formData.value.phone,
      scene: 'register',
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

/**
 * 启动短信验证码倒计时
 * 从 60 秒开始递减，到 0 时自动停止
 */
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

/**
 * 提交注册表单并调用后端注册接口
 */
async function handleRegister() {
  try {
    await formRef.value?.validate();
  } catch {
    return;
  }

  loading.value = true;

  try {
    const result = await registerApi({
      UserName: formData.value.username,
      Password: formData.value.password,
      Phone: formData.value.phone,
      SmsCode: formData.value.smsCode,
    });

    if (result.code === 0) {
      message.success('注册成功，正在跳转到登录页面...');
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

/**
 * 跳转到登录页面
 */
function handleLogin() {
  router.push('/auth/login');
}

function openUserAgreement(event: MouseEvent) {
  event.preventDefault();
  event.stopPropagation();
  agreementVisible.value = true;
}

/**
 * 组件销毁前清理短信验证码倒计时定时器
 */
onBeforeUnmount(() => {
  if (timer) {
    clearInterval(timer);
  }
});
</script>

<template>
  <div class="register-page">
    <div class="register-form-wrapper">
      <div class="register-form-title">
        <h3 class="title">注册账号</h3>
      </div>

      <Form ref="formRef" :model="formData" :rules="rules" class="register-form" validate-trigger="">
        <Form.Item name="username">
          <Input v-model:value="formData.username" size="large" placeholder="请输入用户名（5-20位英文、数字、下划线）" :maxlength="20" autocomplete="username">
            <template #prefix>
              <span class="input-icon">👤</span>
            </template>
          </Input>
        </Form.Item>

        <Form.Item name="password">
          <Input.Password
            v-model:value="formData.password"
            size="large"
            placeholder="请输入登录密码（最少8位，包含字母和数字）"
            :maxlength="100"
            autocomplete="new-password"
          >
            <template #prefix>
              <span class="input-icon">🔒</span>
            </template>
          </Input.Password>
        </Form.Item>

        <Form.Item name="confirmPassword">
          <Input.Password v-model:value="formData.confirmPassword" size="large" placeholder="请确认登录密码" :maxlength="100" autocomplete="new-password">
            <template #prefix>
              <span class="input-icon">🔒</span>
            </template>
          </Input.Password>
        </Form.Item>

        <Form.Item name="phone">
          <Input class="register-phone-with-addon" v-model:value="formData.phone" size="large" placeholder="请输入手机号" autocomplete="tel">
            <template #prefix>
              <span class="input-icon">📱</span>
            </template>
            <template #addonAfter>
              <Button type="default" size="large" :disabled="!canSendCode" class="register-phone-addon-btn" @click="handleSendCode">
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

        <Form.Item name="agreePolicy">
          <Checkbox v-model:checked="formData.agreePolicy">
            <span class="agreement-text">
              我已阅读并同意
              <a class="agreement-link" href="" @click="openUserAgreement">《用户注册协议》</a>
            </span>
          </Checkbox>
        </Form.Item>

        <Button type="primary" size="large" block :loading="loading" class="submit-btn" @click="handleRegister"> 立即注册 </Button>
      </Form>

      <div class="form-footer">
        <span>已有账号？</span>
        <a @click="handleLogin">立即登录</a>
      </div>
    </div>

    <Modal v-model:open="agreementVisible" title="用户注册协议" :footer="null" width="960px" :mask-closable="true">
      <div class="agreement-modal-body">
        <article class="agreement-article">
          <p v-for="(text, i) in agreementBlocks" :key="i" :class="agreementBlockClass(text, i)">
            {{ text }}
          </p>
        </article>
      </div>
    </Modal>
  </div>
</template>

<style scoped lang="less">
.register-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100%;
  padding: 20px;
}

.register-form-wrapper {
  width: 100%;
  max-width: 420px;
}

.register-form-title {
  margin-bottom: 32px;
  text-align: center;

  .title {
    margin: 0;
    font-size: 24px;
    font-weight: 600;
    color: var(--ant-color-text);
  }
}

.register-form {
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

.register-form .register-phone-with-addon {
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

  &:deep(.register-phone-addon-btn.ant-btn) {
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

  &:deep(.register-phone-addon-btn.ant-btn:disabled),
  &:deep(.register-phone-addon-btn.ant-btn.ant-btn-disabled) {
    color: rgba(255, 255, 255, 0.88);
    background: #91caff;
    border-color: #91caff;
  }
}

:deep(.ant-checkbox-wrapper) {
  .agreement-text {
    font-size: 14px;
    color: var(--ant-color-text);

    .agreement-link {
      color: var(--ant-color-primary);
      text-decoration: none;

      &:hover {
        color: var(--ant-color-primary-hover);
        text-decoration: underline;
      }
    }
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

.agreement-modal-body {
  max-height: 70vh;
  overflow-y: auto;
  padding: 4px;
}

.agreement-article {
  padding: 20px 24px 32px;
  color: var(--ant-color-text);
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: var(--ant-border-radius-lg);
}

.agreement-article-title {
  margin: 0 0 16px;
  font-size: 20px;
  font-weight: 600;
  line-height: 1.45;
  text-align: center;
}

.agreement-article-meta {
  margin: 0 0 8px;
  font-size: 13px;
  line-height: 1.55;
  color: var(--ant-color-text-secondary);
  text-align: center;
}

.agreement-article-meta:last-of-type {
  margin-bottom: 20px;
}

.agreement-article-heading {
  margin: 22px 0 10px;
  font-size: 15px;
  font-weight: 600;
  line-height: 1.5;
  color: var(--ant-color-text);
}

.agreement-article-paragraph {
  margin: 0 0 12px;
  font-size: 14px;
  line-height: 1.8;
  text-align: justify;
}
</style>
