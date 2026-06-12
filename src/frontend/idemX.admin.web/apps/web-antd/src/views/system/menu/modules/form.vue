<script lang="ts" setup>
import type { SysMenuReq, SysMenuResp } from '#/api/system/menu';

import { computed, nextTick, reactive, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import * as menuApi from '#/api/system/menu';
import { componentKeys } from '#/router/routes';

import { useFormSchema } from '../data';

const emit = defineEmits<{ success: [] }>();

const formData = ref<SysMenuResp | undefined>();
const menuList = ref<menuApi.SysMenuTreeSelectNode[]>([]);
const componentOptions = computed(() => componentKeys.map((v) => ({ value: v })));

const [Form, formApi] = useVbenForm(
  reactive({
    commonConfig: {
      componentProps: {
        class: 'w-full',
      },
    },
    layout: 'horizontal',
    labelWidth: 120,
    schema: computed(() => useFormSchema(menuList.value, componentOptions.value)),
    scrollToFirstError: true,
    showDefaultActions: false,
    wrapperClass: 'grid-cols-1',
  }),
);

function buildDrawerValues(raw?: SysMenuResp, appendParentId?: string): SysMenuReq {
  if (raw) {
    return {
      parentId: raw.parentId,
      name: raw.name,
      title: raw.title,
      path: raw.path,
      icon: raw.icon,
      component: raw.component,
      authority: raw.authority,
      badge: raw.badge,
      badgeType: raw.badgeType,
      badgeVariants: raw.badgeVariants,
      sort: raw.sort,
      status: raw.status,
      keepAlive: raw.keepAlive,
      affixTab: raw.affixTab,
      isExternal: raw.isExternal,
      menuType: 0,
      redirect: raw.redirect,
      roles: raw.roles,
      externalUrl: raw.externalUrl,
      iframeUrl: raw.iframeUrl,
      activeMenu: raw.activeMenu,
      breadcrumbParentIcon: raw.breadcrumbParentIcon,
      link: raw.link,
    };
  }
  return menuApi.getDefaultSysMenuReq(appendParentId);
}

const [Drawer, drawerApi] = useVbenDrawer({
  onConfirm: handleSubmit,
  onOpenChange: async (isOpen) => {
    if (!isOpen) return;
    const listRes = await menuApi.getListApi();
    if (listRes.code === 0) {
      menuList.value = menuApi.toTreeSelectNodes(listRes.data!);
    } else {
      message.error(listRes.msg);
      menuList.value = [];
    }
    const data = drawerApi.getData();
    const isEdit = !!data && typeof data === 'object' && 'id' in data;
    const parentId = !isEdit && !!data && typeof data === 'object' && 'parentId' in data ? (data as { parentId?: string }).parentId : undefined;
    formData.value = isEdit ? (data as SysMenuResp) : undefined;
    await nextTick();
    await formApi.resetValidate();
    await formApi.setValues(buildDrawerValues(formData.value, parentId));
  },
});

async function handleSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  drawerApi.lock();
  try {
    const values = await formApi.getValues<SysMenuReq>();
    const payload: SysMenuReq = {
      ...values,
      parentId: values.parentId === '' ? undefined : values.parentId,
    };
    const result = formData.value?.id ? await menuApi.editApi(formData.value.id, payload) : await menuApi.createApi(payload);
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
  <Drawer class="w-full max-w-[600px]" :title="formData?.id ? `编辑菜单 - ${formData.title}` : '新增菜单'">
    <Form class="mx-auto" />
  </Drawer>
</template>
