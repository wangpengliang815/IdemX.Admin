<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRoleResp } from '#/api/system/role';
import type { SysUserPageQueryReq, SysUserResp } from '#/api/system/user';

import { nextTick, onMounted, ref, watch } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';

import { Button, message, Space, Switch, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import * as roleApi from '#/api/system/role';
import * as userApi from '#/api/system/user';
import { useEnumOptions } from '#/utils';

import { useGridFormSchema, useTableColumns } from './data';
import UserForm from './modules/form.vue';

const roleOptions = ref<SysRoleResp[]>([]);
const stateOptions = useEnumOptions('UserStatus');
const sexTagOptions = useEnumOptions('UserSexType', { 1: 'blue', 2: 'pink', 3: 'default' });

async function loadRoleOptions() {
  const roleResult = await roleApi.getListApi();
  if (roleResult.code !== 0) {
    message.error(roleResult.msg);
    return;
  }
  roleOptions.value = roleResult.data as SysRoleResp[];
}

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: UserForm,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions: {
    schema: useGridFormSchema(() => gridApi.query(), stateOptions.value, roleOptions.value),
    submitOnChange: false,
    showCollapseButton: false,
  },
  gridOptions: {
    columns: useTableColumns(onActionClick, sexTagOptions.value),
    height: '100%',
    keepSource: true,
    headerRowHeight: 48,
    pagerConfig: {
      autoHidden: false,
      pageSize: 30,
      pageSizes: [30, 50, 100],
    },
    proxyConfig: {
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }, formValues: Record<string, unknown>) => {
          const params: SysUserPageQueryReq = {
            page: page.currentPage,
            pageSize: page.pageSize,
            roleId: formValues?.roleId as string | undefined,
            userName: formValues?.userName as string | undefined,
            status: formValues?.status as number | undefined,
          };

          const res = await userApi.getPageListApi(params);
          if (res.code !== 0) {
            message.error(res.msg);
            return { data: [], total: 0 };
          }
          return { data: res.data!, total: res.total! };
        },
      },
      response: { result: 'data', total: 'total' },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, zoom: true },
  } as VxeTableGridOptions<SysUserResp>,
});

watch(roleOptions, () => {
  const schema = useGridFormSchema(() => gridApi.query(), stateOptions.value, roleOptions.value);
  if (schema) nextTick(() => gridApi.formApi?.updateSchema(schema));
}, { deep: true });

async function onActionClick({ code, row }: { code: string; row: SysUserResp }) {
  switch (code) {
    case 'delete': {
      const result = await userApi.delApi(row.id);
      if (result.code === 0) {
        message.success(result.msg);
        gridApi.query();
      } else {
        message.error(result.msg);
      }
      break;
    }
    case 'edit': {
      formDrawerApi.setData(row).open();
      break;
    }
  }
}

async function onStateChange(checked: boolean, row: SysUserResp) {
  const newState = checked ? 0 : 1;
  const result = await userApi.setStatusApi(row.id, newState);
  if (result.code === 0) {
    message.success(result.msg);
    row.status = newState;
  } else {
    message.error(result.msg);
    row.status = checked ? 1 : 0;
  }
}

onMounted(async () => {
  await loadRoleOptions();
  gridApi.query();
});
</script>

<template>
  <Page auto-content-height content-class="flex h-full min-h-0 flex-col overflow-hidden p-4">
    <FormDrawer @success="() => gridApi.query()" />

    <div class="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
      <Grid>
        <template #toolbar-tools>
          <Button type="primary" @click="formDrawerApi.open()"> 新建用户 </Button>
        </template>
        <template #roles="{ row }">
          <span v-if="row.roles && row.roles.length > 0">
            <Tag v-for="role in row.roles" :key="role.id" color="blue">
              {{ role.roleName }}
            </Tag>
          </span>
        </template>
        <template #status="{ row }">
          <Switch :checked="row.status === 0" @change="(checked) => onStateChange(!!checked, row)" />
        </template>
        <template #action="{ row }">
          <Space>
            <Button type="link" size="small" @click="onActionClick({ code: 'edit', row })">编辑</Button>
            <Button type="link" size="small" danger @click="onActionClick({ code: 'delete', row })">删除</Button>
          </Space>
        </template>
      </Grid>
    </div>
  </Page>
</template>
