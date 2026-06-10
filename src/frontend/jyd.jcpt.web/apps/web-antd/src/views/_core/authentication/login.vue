<script lang="ts" setup>
import { computed, onBeforeUnmount, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Button, Form, Input, message, Tabs } from 'ant-design-vue';

import { sendSmsCodeApi } from '#/api/core/auth';
import { useAuthStore } from '#/store';

defineOptions({ name: 'Login' });

const router = useRouter();
const authStore = useAuthStore();
const appTitle = import.meta.env.VITE_APP_TITLE;

// 当前激活的登录方式：account=账号密码，phone=手机号验证码
const activeTab = ref<'account' | 'phone'>('account');

// 登录请求统一使用 authStore 的 loading 状态
const loading = computed(() => authStore.loginLoading);

// 短信验证码倒计时（秒）
const countdown = ref(0);
let timer: NodeJS.Timeout | null = null;

// 账号密码表单数据
const accountForm = ref({
  username: '',
  password: '',
});

// 手机号表单数据
const phoneForm = ref({
  phoneNumber: '',
  smsCode: '',
});

const accountRules = {
  username: [{ required: true, message: '请输入用户名' }],
  password: [{ required: true, message: '请输入密码' }],
};

const phoneRules = {
  phoneNumber: [
    { required: true, message: '请输入手机号' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号' },
  ],
  smsCode: [
    { required: true, message: '请输入验证码' },
    { len: 6, message: '验证码为6位数字' },
  ],
};

const accountFormRef = ref();
const phoneFormRef = ref();

// 发送验证码按钮文字：倒计时中显示秒数，否则显示"获取验证码"
const sendCodeText = computed(() => {
  return countdown.value > 0 ? `${countdown.value}秒` : '获取验证码';
});

// 是否可以发送验证码：倒计时结束且手机号格式正确
const canSendCode = computed(() => {
  return countdown.value === 0 && /^1[3-9]\d{9}$/.test(phoneForm.value.phoneNumber);
});

/**
 * 使用账号密码进行登录
 * 具体登录流程由 authStore.authLoginByPassword 负责
 */
async function handleAccountLogin() {
  try {
    await accountFormRef.value?.validate();
  } catch {
    return;
  }
  await authStore.authLoginByPassword({
    userName: accountForm.value.username,
    password: accountForm.value.password,
  });
}

/**
 * 使用手机号和验证码进行登录
 * 具体登录流程由 authStore.authLoginByPhone 负责
 */
async function handlePhoneLogin() {
  try {
    await phoneFormRef.value?.validate();
  } catch {
    return;
  }
  await authStore.authLoginByPhone({
    phoneNumber: phoneForm.value.phoneNumber,
    smsCode: phoneForm.value.smsCode,
  });
}

/**
 * 发送手机号登录使用的短信验证码
 * 验证通过后调用后端发送验证码接口并启动倒计时
 */
async function handleSendCode() {
  if (!canSendCode.value) return;

  try {
    await phoneFormRef.value?.validateFields(['phoneNumber']);

    const result = await sendSmsCodeApi({
      phoneNumber: phoneForm.value.phoneNumber,
      scene: 'login',
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
 * 跳转到忘记密码页面
 */
function handleForgetPassword() {
  router.push('/auth/forget-password');
}

/**
 * 跳转到注册页面
 */
function handleRegister() {
  router.push('/auth/register');
}

onBeforeUnmount(() => {
  if (timer) {
    clearInterval(timer);
  }
});
</script>

<template>
  <div class="login-page">
    <div class="login-form-wrapper">
      <div class="login-form-title">
        <h3 class="title">{{ appTitle }} 👋</h3>
      </div>

      <Tabs v-model:active-key="activeTab" class="login-tabs">
        <!-- 账号密码登录 -->
        <Tabs.TabPane key="account">
          <template #tab>
            <span class="tab-label">
              <span class="tab-icon">🔑</span>
              <span>账号密码登录</span>
            </span>
          </template>

          <Form ref="accountFormRef" :model="accountForm" :rules="accountRules" class="login-form" validate-trigger="">
            <Form.Item name="username">
              <Input v-model:value="accountForm.username" size="large" placeholder="请输入用户名" autocomplete="username">
                <template #prefix>
                  <span class="input-icon">👤</span>
                </template>
              </Input>
            </Form.Item>

            <Form.Item name="password">
              <Input.Password
                v-model:value="accountForm.password"
                size="large"
                placeholder="请输入密码"
                autocomplete="current-password"
                @press-enter="handleAccountLogin"
              >
                <template #prefix>
                  <span class="input-icon">🔒</span>
                </template>
              </Input.Password>
            </Form.Item>

            <div class="form-actions">
              <a class="forgot-link" @click="handleForgetPassword">忘记密码?</a>
            </div>

            <Button type="primary" size="large" block :loading="loading" class="submit-btn" @click="handleAccountLogin"> 登 录 </Button>
          </Form>
        </Tabs.TabPane>

        <!-- 手机号登录 -->
        <Tabs.TabPane key="phone">
          <template #tab>
            <span class="tab-label">
              <span class="tab-icon">📱</span>
              <span>手机号登录</span>
            </span>
          </template>

          <Form ref="phoneFormRef" :model="phoneForm" :rules="phoneRules" class="login-form" validate-trigger="">
            <Form.Item name="phoneNumber">
              <Input v-model:value="phoneForm.phoneNumber" size="large" placeholder="请输入手机号" autocomplete="tel">
                <template #prefix>
                  <span class="input-icon">📱</span>
                </template>
              </Input>
            </Form.Item>

            <Form.Item name="smsCode">
              <Input
                class="login-sms-with-addon"
                v-model:value="phoneForm.smsCode"
                size="large"
                placeholder="请输入验证码"
                :maxlength="6"
                autocomplete="one-time-code"
                @press-enter="handlePhoneLogin"
              >
                <template #prefix>
                  <span class="input-icon">✉️</span>
                </template>
                <template #addonAfter>
                  <Button type="default" size="large" :disabled="!canSendCode" class="login-sms-addon-btn" @click="handleSendCode">
                    {{ sendCodeText }}
                  </Button>
                </template>
              </Input>
            </Form.Item>

            <div class="form-actions">
              <a class="forgot-link" @click="handleForgetPassword">忘记密码?</a>
            </div>

            <Button type="primary" size="large" block :loading="loading" class="submit-btn" @click="handlePhoneLogin"> 登 录 </Button>
          </Form>
        </Tabs.TabPane>
      </Tabs>

      <div class="form-footer">
        <span>还没有账号？</span>
        <a @click="handleRegister">立即注册</a>
      </div>
    </div>
  </div>
</template>

<style scoped lang="less">
.login-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100%;
  padding: 20px;
}

.login-form-wrapper {
  width: 100%;
  max-width: 420px;
}

.login-form-title {
  margin-bottom: 32px;
  text-align: left;

  .title {
    margin: 0 0 8px;
    font-size: 24px;
    font-weight: 600;
    color: var(--ant-color-text);
  }

  .desc {
    margin: 0;
    font-size: 14px;
    color: var(--ant-color-text-secondary);
  }
}

.login-tabs {
  :deep(.ant-tabs-nav) {
    margin-bottom: 32px;
  }

  :deep(.ant-tabs-tab) {
    font-size: 15px;
    padding: 12px 0;
  }

  .tab-label {
    display: flex;
    align-items: center;
    gap: 6px;

    .tab-icon {
      font-size: 16px;
    }
  }
}

.login-form {
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

.form-actions {
  margin-bottom: 24px;
  text-align: right;

  .forgot-link {
    font-size: 14px;
    color: var(--ant-color-primary);
    cursor: pointer;

    &:hover {
      color: var(--ant-color-primary-hover);
    }
  }
}

/* addonAfter 为 table-cell：相对定位 + 按钮 absolute；与左侧同高需 middle 对齐，并抵消 input-group 外框 1px 边框 */
.login-form .login-sms-with-addon {
  width: 100%;

  &:deep(.ant-input-wrapper.ant-input-group .ant-input-affix-wrapper) {
    vertical-align: middle;
  }

  &:deep(.ant-input-group-addon:last-child) {
    position: relative;
    width: 110px;
    padding: 0;
    overflow: visible;
    vertical-align: middle;
  }

  &:deep(.login-sms-addon-btn.ant-btn) {
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
    /* 固定高对比色，与主题 token 解耦 */
    color: #fff;
    background: #1677ff;
    border-color: #1677ff;

    &:not(:disabled):hover {
      color: #fff;
      background: #0958d9;
      border-color: #0958d9;
    }
  }

  &:deep(.login-sms-addon-btn.ant-btn:disabled),
  &:deep(.login-sms-addon-btn.ant-btn.ant-btn-disabled) {
    color: rgba(255, 255, 255, 0.88);
    background: #91caff;
    border-color: #91caff;
  }
}

.submit-btn {
  height: 44px;
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
