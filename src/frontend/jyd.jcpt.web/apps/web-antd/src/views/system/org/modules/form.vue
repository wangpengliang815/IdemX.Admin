<script lang="ts" setup>
import type { SysOrgReq, SysOrgResp } from '#/api/system/org';

import { computed, nextTick, reactive, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import * as orgApi from '#/api/system/org';

import { useFormSchema } from '../data';

const emit = defineEmits<{ success: [] }>();

const formData = ref<SysOrgResp | undefined>();
const orgList = ref<orgApi.SysOrgTreeSelectNode[]>([]);

const [Form, formApi] = useVbenForm(
  reactive({
    commonConfig: {
      componentProps: {
        class: 'w-full',
      },
    },
    layout: 'horizontal',
    labelWidth: 100,
    schema: computed(() => useFormSchema(orgList.value)),
    scrollToFirstError: true,
    showDefaultActions: false,
    wrapperClass: 'grid-cols-1',
  }),
);

async function loadOrgTreeData() {
  const listRes = await orgApi.getListApi();
  if (listRes.code === 0) {
    orgList.value = orgApi.toTreeSelectNodes(listRes.data!);
    return;
  }
  message.error(listRes.msg);
  orgList.value = [];
}

function buildDrawerValues(raw?: SysOrgResp): SysOrgReq {
  if (raw) {
    return {
      parentId: raw.parentId,
      name: raw.name,
      sort: raw.sort,
    };
  }
  return orgApi.getDefaultSysOrgReq();
}

const [Drawer, drawerApi] = useVbenDrawer({
  onConfirm: handleSubmit,
  onOpenChange: async (isOpen) => {
    if (!isOpen) return;
    await loadOrgTreeData();
    const raw = drawerApi.getData() as SysOrgResp | undefined;
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
    const values = await formApi.getValues<SysOrgReq>();
    const payload: SysOrgReq = {
      name: values.name,
      sort: values.sort,
      parentId: values.parentId,
    };
    const result = formData.value?.id ? await orgApi.editApi(formData.value.id, payload) : await orgApi.createApi(payload);
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
  <Drawer class="w-full max-w-[600px]" :title="formData?.id ? `编辑组织机构 - ${formData.name}` : '新增组织机构'">
    <Form class="mx-auto" />
  </Drawer>
</template>
