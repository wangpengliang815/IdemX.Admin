<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysOrgResp } from '#/api/system/org';

import { onMounted } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import * as orgApi from '#/api/system/org';

import { useTableColumns } from './data';
import OrgForm from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: OrgForm,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useTableColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    pagerConfig: {
      enabled: false,
    },
    proxyConfig: {
      ajax: {
        query: loadOrgTreeData,
      },
      response: { result: 'data', total: 'total' },
    },
    rowConfig: {
      keyField: 'id',
    },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      zoom: true,
    },
    treeConfig: {
      lazy: true,
      childrenField: 'children',
      hasChildField: 'hasChild',
      loadMethod: loadOrgChildren,
      expandAll: false,
    },
  } as VxeTableGridOptions<SysOrgResp>,
});

onMounted(() => {
  refreshGrid();
});

async function loadOrgTreeNodes(parentId?: string) {
  const result = parentId ? await orgApi.getTreeNodesApi(parentId) : await orgApi.getListApi();
  if (result.code !== 0) {
    message.error(result.msg);
    return [] as SysOrgResp[];
  }
  return result.data!;
}

async function loadOrgChildren({ row }: { row: SysOrgResp }) {
  return loadOrgTreeNodes(row.id);
}

async function loadOrgTreeData() {
  const data = await loadOrgTreeNodes();
  return {
    data,
    total: data.length,
  };
}

function refreshGrid() {
  gridApi.query();
}

async function onActionClick({ code, row }: { code: string; row: SysOrgResp }) {
  switch (code) {
    case 'delete': {
      const result = await orgApi.delApi(row.id);
      if (result.code === 0) {
        message.success(result.msg);
        refreshGrid();
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
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="refreshGrid" />
    <Grid>
      <template #toolbar-tools>
        <Button type="primary" @click="formDrawerApi.open()"> 新增组织机构 </Button>
      </template>
    </Grid>
  </Page>
</template>
