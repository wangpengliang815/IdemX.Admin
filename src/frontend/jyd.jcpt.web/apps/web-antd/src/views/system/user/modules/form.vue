<script lang="ts" setup>
import type { SysRoleResp } from '#/api/system/role';
import type { SysUserReq, SysUserResp } from '#/api/system/user';

import { computed, nextTick, reactive, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import * as roleApi from '#/api/system/role';
import * as userApi from '#/api/system/user';
import { useEnumOptions } from '#/utils';

import { useFormSchema } from '../data';

const emit = defineEmits<{ success: [] }>();

const formData = ref<SysUserResp | undefined>();
const roleOptions = ref<SysRoleResp[]>([]);

const sexOptions = useEnumOptions('UserSexType');

const [Form, formApi] = useVbenForm(
  reactive({
    commonConfig: {
      componentProps: {
        class: 'w-full',
      },
    },
    layout: 'horizontal',
    labelWidth: 120,
    schema: computed(() => useFormSchema(roleOptions.value, sexOptions.value, !!formData.value?.id)),
    scrollToFirstError: true,
    showDefaultActions: false,
    wrapperClass: 'grid-cols-1',
  }),
);

function buildDrawerValues(raw?: SysUserResp): Record<string, unknown> {
  if (raw) {
    // 库表可多角色；当前仅支持单角色，回显取列表第一项
    return {
      userName: raw.userName,
      realName: raw.realName,
      phone: raw.phone!,
      sex: raw.sex,
      roleId: raw.roles?.[0]?.id,
      password: '',
    };
  }
  return userApi.getDefaultSysUserReq() as unknown as Record<string, unknown>;
}

const [Drawer, drawerApi] = useVbenDrawer({
  onConfirm: handleSubmit,
  onOpenChange: async (isOpen) => {
    if (!isOpen) return;
    await loadRoleOptions();
    const raw = drawerApi.getData() as SysUserResp | undefined;
    formData.value = raw;
    await nextTick();
    await formApi.resetValidate();
    await formApi.setValues(buildDrawerValues(raw));
  },
});

async function loadRoleOptions() {
  const roleResult = await roleApi.getListApi();
  if (roleResult.code !== 0) {
    message.error(roleResult.msg);
    return;
  }
  roleOptions.value = roleResult.data as SysRoleResp[];
}

async function handleSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;

  drawerApi.lock();
  try {
    const rawValues = await formApi.getValues<Record<string, unknown>>();
    const roleId = rawValues.roleId as string | undefined;
    const values: SysUserReq = {
      userName: rawValues.userName as string,
      realName: rawValues.realName as string,
      phone: rawValues.phone as string,
      sex: rawValues.sex as number,
      roleIds: userApi.toSingleRoleIds(roleId),
      password: rawValues.password as string | undefined,
    };
    const previousRoleId = formData.value?.roles?.[0]?.id;
    const result = formData.value?.id ? await userApi.editApi(formData.value.id, values) : await userApi.createApi(values);
    if (result.code === 0) {
      message.success(result.msg);
      if (formData.value?.id && previousRoleId && roleId && previousRoleId !== roleId) {
        message.info('角色已变更，该用户需重新登录后权限才会生效');
      }
      drawerApi.close();
      emit('success');
    } else {
      message.error(result.msg);
    }
  } finally {
    drawerApi.unlock();
  }
}
</script>

<template>
  <Drawer class="w-full max-w-[600px]" :title="formData?.id ? `编辑用户 - ${formData.realName}` : '新增用户'">
    <Form class="mx-auto" />
  </Drawer>
</template>
