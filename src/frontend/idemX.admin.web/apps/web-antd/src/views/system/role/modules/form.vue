<script lang="ts" setup>
import type { SysRoleReq, SysRoleResp } from '#/api/system/role';

import { computed, nextTick, reactive, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import * as roleApi from '#/api/system/role';

import { useFormSchema } from '../data';

const emit = defineEmits<{ success: [] }>();

const formData = ref<SysRoleResp | undefined>();

const [Form, formApi] = useVbenForm(
  reactive({
    commonConfig: {
      componentProps: {
        class: 'w-full',
      },
    },
    layout: 'horizontal',
    labelWidth: 120,
    schema: computed(() => useFormSchema()),
    scrollToFirstError: true,
    showDefaultActions: false,
    wrapperClass: 'grid-cols-1',
  }),
);

function buildDrawerValues(raw?: SysRoleResp): SysRoleReq {
  if (raw) {
    return {
      roleName: raw.roleName,
      roleCode: raw.roleCode,
      memo: raw.memo,
    };
  }
  return roleApi.getDefaultSysRoleReq();
}

const [Drawer, drawerApi] = useVbenDrawer({
  onConfirm: handleSubmit,
  onOpenChange: async (isOpen) => {
    if (!isOpen) return;
    const raw = drawerApi.getData() as SysRoleResp | undefined;
    formData.value = raw;
    await nextTick();
    await formApi.resetValidate();
    await formApi.setValues(buildDrawerValues(raw));
  },
});

async function handleSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;

  drawerApi.lock();
  try {
    const values = await formApi.getValues<SysRoleReq>();
    const result = formData.value?.id ? await roleApi.editApi(formData.value.id, values) : await roleApi.createApi(values);
    if (result.code === 0) {
      message.success(result.msg);
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
  <Drawer class="w-full max-w-[600px]" :title="formData?.id ? `编辑角色 - ${formData.roleName}` : '新增角色'">
    <Form class="mx-auto" />
  </Drawer>
</template>
