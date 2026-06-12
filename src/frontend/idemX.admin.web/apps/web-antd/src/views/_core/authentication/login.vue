<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Button, Form, Input } from 'ant-design-vue';

import { useAuthStore } from '#/store';

defineOptions({ name: 'Login' });

const router = useRouter();
const authStore = useAuthStore();
const appTitle = import.meta.env.VITE_APP_TITLE;

const loading = computed(() => authStore.loginLoading);

const accountForm = ref({
  username: '',
  password: '',
});

const accountRules = {
  username: [{ required: true, message: '请输入用户名' }],
  password: [{ required: true, message: '请输入密码' }],
};

const accountFormRef = ref();

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

function handleForgetPassword() {
  router.push('/auth/forget-password');
}
</script>

<template>
  <div class="login-page">
    <div class="login-form-wrapper">
      <div class="login-form-title">
        <h3 class="title">{{ appTitle }} 👋</h3>
      </div>

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

.submit-btn {
  height: 44px;
  font-size: 16px;
  font-weight: 500;
  letter-spacing: 2px;
}
</style>
