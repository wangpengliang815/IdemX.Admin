<script setup lang="ts">
import type { FormInstance, Rule } from 'ant-design-vue/es/form';
import type { UploadFile } from 'ant-design-vue/es/upload/interface';

import type { SysUserProfileInfo } from '#/api/profile';

import { computed, onMounted, reactive, ref, watch } from 'vue';

import { createIconifyIcon } from '@vben/icons';
import { preferences } from '@vben/preferences';
import { useUserStore } from '@vben/stores';

import { Avatar, Button, Card, Col, Form, FormItem, Input, message, Row, Select, Spin, Upload } from 'ant-design-vue';
import { storeToRefs } from 'pinia';

import { editUserInfoApi, uploadAvatarApi } from '#/api/profile';
import { useAuthStore } from '#/store/auth';
import { useEnumOptions } from '#/utils';

const userStore = useUserStore();
const authStore = useAuthStore();
const { avatarNonce } = storeToRefs(authStore);

const loading = ref(false);
const uploading = ref(false);
const submitting = ref(false);
const userData = ref<null | SysUserProfileInfo>(null);
const formRef = ref<FormInstance>();

const IconCamera = createIconifyIcon('mdi:camera-outline');
const IconLoading = createIconifyIcon('mdi:loading');

const AVATAR_SIZE = 72;

const sexOptions = useEnumOptions('UserSexType');

const formState = reactive({
  nickName: '',
  realName: '',
  userName: '',
  phone: '',
  email: '',
  wechatNo: '',
  sex: 0,
  avatar: '',
});

const avatarUrl = computed(() => {
  const raw = formState.avatar || preferences.app.defaultAvatar;
  if (!raw || typeof raw !== 'string' || raw.startsWith('data:') || raw.startsWith('blob:')) {
    return raw;
  }
  if (!raw.startsWith('http://') && !raw.startsWith('https://')) {
    return raw;
  }
  const t = avatarNonce.value;
  if (!t) return raw;
  return raw.includes('?') ? `${raw}&t=${t}` : `${raw}?t=${t}`;
});

const rules: Record<string, Rule[]> = {
  email: [
    {
      validator: async (_rule, value: string) => {
        if (!value) return;
        const reg = /^[\w.+-]+@[\w-]+(?:\.[\w-]+)+$/;
        if (!reg.test(value)) throw new Error('邮箱格式不正确');
      },
      trigger: 'blur',
    },
  ],
};

function updateLocalUserData(data: SysUserProfileInfo) {
  userData.value = data;
  formState.nickName = data.nickName!;
  formState.realName = data.realName;
  formState.userName = data.userName;
  formState.phone = data.phone!;
  formState.email = data.email!;
  formState.wechatNo = data.wechatNo!;
  formState.sex = data.sex;
  formState.avatar = data.avatar!;
}

async function loadUserInfo() {
  loading.value = true;
  try {
    const data = await authStore.getUserInfo();
    if (data) {
      updateLocalUserData(data as SysUserProfileInfo);
    } else {
      message.error('获取用户信息失败');
    }
  } finally {
    loading.value = false;
  }
}

onMounted(async () => {
  await loadUserInfo();
});

watch(
  () => userStore.userInfo,
  (val) => {
    if (val) updateLocalUserData(val as unknown as SysUserProfileInfo);
  },
  { deep: true },
);

function beforeUpload(file: File) {
  const isImage = file.type.startsWith('image/');
  if (!isImage) {
    message.error('只能上传图片文件');
    return false;
  }
  const isLt3M = file.size / 1024 / 1024 < 3;
  if (!isLt3M) {
    message.error('图片大小不能超过 3MB');
    return false;
  }
  return true;
}

async function customRequest(options: Record<string, any>) {
  const { file, onSuccess, onError, onProgress } = options;
  const blob = file instanceof Blob ? file : ((file as unknown as UploadFile).originFileObj as Blob | undefined);
  if (!blob) {
    message.error('文件无效');
    onError?.(new Error('文件无效'));
    return;
  }

  uploading.value = true;

  try {
    onProgress?.({ percent: 50 });

    const result = await uploadAvatarApi(blob);

    onProgress?.({ percent: 80 });

    if (result.code === 0) {
      onProgress?.({ percent: 100 });

      if (result.msg) {
        message.success(result.msg);
      }
      const refreshed = await authStore.getUserInfo({ force: true });
      if (refreshed) {
        updateLocalUserData(refreshed as SysUserProfileInfo);
      }
      onSuccess?.(result, file);
    } else {
      const errorMsg = result.msg || '头像上传失败';
      message.error(errorMsg);
      onError?.(new Error(errorMsg));
    }
  } catch (error: unknown) {
    const err = error as { message?: string; msg?: string; response?: { data?: { msg?: string } } };
    const errorMsg = err?.response?.data?.msg || err?.msg || err?.message;
    if (errorMsg) {
      message.error(errorMsg);
    }
    onError?.(error);
  } finally {
    uploading.value = false;
  }
}

function handleAvatarChange(info: { file: { status?: string } }) {
  const { file } = info;
  if (file.status === 'uploading') {
    uploading.value = true;
  } else if (file.status === 'done' || file.status === 'removed' || file.status === 'error') {
    uploading.value = false;
  }
}

async function handleSubmit() {
  const id = userData.value?.id;
  if (!id) {
    message.error('用户信息不完整');
    return;
  }

  const valid = await formRef.value?.validate().then(
    () => true,
    () => false,
  );
  if (!valid) return;

  submitting.value = true;
  try {
    const result = await editUserInfoApi(id, {
      nickName: formState.nickName,
      email: formState.email,
      wechatNo: formState.wechatNo,
      sex: formState.sex,
    });

    if (result.code === 0) {
      message.success(result.msg);
      const refreshed = await authStore.getUserInfo({ force: true });
      if (refreshed) {
        updateLocalUserData(refreshed as SysUserProfileInfo);
      }
    } else {
      message.error(result.msg || '保存失败');
    }
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <Spin :spinning="loading">
    <div class="mx-auto w-full p-4">
      <Card :bordered="false" :style="{ borderRadius: '16px', padding: '16px' }">
        <div class="mb-4 flex items-start gap-6">
          <div
            class="relative shrink-0"
            :style="{
              width: `${AVATAR_SIZE}px`,
              height: `${AVATAR_SIZE}px`,
            }"
          >
            <Avatar :size="AVATAR_SIZE" :src="avatarUrl" class="bg-primary/10" />
            <div class="absolute bottom-0 right-0">
              <Upload :before-upload="beforeUpload" :custom-request="customRequest" :show-upload-list="false" accept="image/*" @change="handleAvatarChange">
                <Button
                  shape="circle"
                  size="middle"
                  type="primary"
                  :disabled="uploading"
                  class="h-[30px] w-[30px] min-w-[30px] text-xs shadow-[0_4px_12px_rgba(24,144,255,0.3)]"
                >
                  <template #icon>
                    <component :is="uploading ? IconLoading : IconCamera" class="text-[20px]" />
                  </template>
                </Button>
              </Upload>
            </div>
          </div>
          <div class="">
            <div class="flex items-center gap-2 text-lg font-medium text-gray-900">
              {{ formState.userName }}
            </div>
            <div v-if="formState.email" class="mt-1 text-sm text-gray-500">{{ formState.email }}</div>
          </div>
        </div>

        <Form ref="formRef" :model="formState" :rules="rules" size="middle" layout="vertical" class="setting-form">
          <Row :gutter="16">
            <Col :span="12">
              <FormItem label="用户名">
                <Input v-model:value="formState.userName" disabled class="w-full" />
              </FormItem>
            </Col>
            <Col :span="12">
              <FormItem label="昵称">
                <Input v-model:value="formState.nickName" placeholder="请输入昵称" class="w-full" />
              </FormItem>
            </Col>
            <Col :span="12">
              <FormItem label="真实姓名">
                <Input v-model:value="formState.realName" disabled class="w-full" />
              </FormItem>
            </Col>
            <Col :span="12">
              <FormItem label="手机号码">
                <Input v-model:value="formState.phone" disabled class="w-full" />
              </FormItem>
            </Col>
            <Col :span="12">
              <FormItem label="邮箱地址" name="email">
                <Input v-model:value="formState.email" placeholder="请输入邮箱地址" class="w-full" />
              </FormItem>
            </Col>
            <Col :span="12">
              <FormItem label="微信号">
                <Input v-model:value="formState.wechatNo" placeholder="请输入微信号" class="w-full" />
              </FormItem>
            </Col>

            <Col :span="12">
              <FormItem label="性别">
                <Select v-model:value="formState.sex" :options="sexOptions" placeholder="请选择性别" class="w-full" />
              </FormItem>
            </Col>
          </Row>

          <div class="mt-6 flex justify-end gap-3">
            <Button size="middle" type="primary" :loading="submitting" @click="handleSubmit"> 保存修改 </Button>
          </div>
        </Form>
      </Card>
    </div>
  </Spin>
</template>
