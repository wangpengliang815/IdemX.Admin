<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRoleResp } from '#/api/system/role';

import { onMounted } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import * as roleApi from '#/api/system/role';

import { useGridFormSchema, useTableColumns } from './data';
import RoleForm from './modules/form.vue';
import ImportPermission from './modules/importpermission.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: RoleForm,
  destroyOnClose: true,
});

const [ImportPermissionModalWrap, importPermissionModalApi] = useVbenModal({
  connectedComponent: ImportPermission,
});

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions: {
    schema: useGridFormSchema(() => gridApi.query()),
    submitOnChange: false,
    showCollapseButton: false,
  },
  gridOptions: {
    columns: useTableColumns(onActionClick),
    height: 'auto',
    pagerConfig: { pageSize: 30, pageSizes: [30, 50, 100] },
    proxyConfig: {
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }, formValues: Record<string, unknown>) => {
          const res = await roleApi.getPageListApi({
            page: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          });
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
  } as VxeTableGridOptions<SysRoleResp>,
});

async function onActionClick({ code, row }: { code: string; row: SysRoleResp }) {
  switch (code) {
    case 'delete': {
      const result = await roleApi.delApi(row.id);
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
    case 'permission': {
      importPermissionModalApi.setData({ roleId: row.id, roleName: row.roleName }).open();
      break;
    }
  }
}

onMounted(() => gridApi.query());
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="() => gridApi.query()" />
    <ImportPermissionModalWrap @success="() => gridApi.query()" />

    <Grid>
      <template #toolbar-tools>
        <Button type="primary" @click="formDrawerApi.open()"> 新增角色 </Button>
      </template>
    </Grid>
  </Page>
</template>
