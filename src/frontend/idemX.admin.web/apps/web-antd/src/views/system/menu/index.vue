<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysMenuResp } from '#/api/system/menu';

import { onMounted } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { IconifyIcon } from '@vben/icons';

import { Button, message, Space } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import * as menuApi from '#/api/system/menu';

import { useTableColumns } from './data';
import MenuForm from './modules/form.vue';
import ImportButton from './modules/importbutton.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: MenuForm,
  destroyOnClose: true,
});

const [ImportButtonModalWrap, importButtonModalApi] = useVbenModal({
  connectedComponent: ImportButton,
});

const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useTableColumns(onActionClick),
    keepSource: true,
    pagerConfig: {
      enabled: false,
    },
    proxyConfig: {
      ajax: {
        query: loadMenuTreeData,
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
      zoom: false,
    },
    treeConfig: {
      lazy: true,
      childrenField: 'children',
      hasChildField: 'hasChild',
      loadMethod: loadMenuChildren,
      expandAll: false,
    },
  } as VxeTableGridOptions<SysMenuResp>,
});

onMounted(() => {
  refreshGrid();
});

async function loadMenuTreeNodes(parentId?: string) {
  const result = parentId ? await menuApi.getTreeNodesApi(parentId) : await menuApi.getListApi();
  if (result.code !== 0) {
    message.error(result.msg);
    return [] as SysMenuResp[];
  }
  return result.data!;
}

async function loadMenuChildren({ row }: { row: SysMenuResp }) {
  return loadMenuTreeNodes(row.id);
}

async function loadMenuTreeData() {
  const data = await loadMenuTreeNodes();
  return { data, total: data.length };
}

function refreshGrid() {
  gridApi.query();
}

async function onActionClick({ code, row }: { code: string; row: SysMenuResp }) {
  switch (code) {
    case 'append': {
      formDrawerApi.setData({ parentId: row.id }).open();
      break;
    }
    case 'delete': {
      const result = await menuApi.delApi(row.id);
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
    case 'import': {
      importButtonModalApi.setData({ menuId: row.id, menuName: row.title }).open();
      break;
    }
  }
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="refreshGrid" />
    <ImportButtonModalWrap @success="refreshGrid" />
    <Grid>
      <template #toolbar-tools>
        <Button type="primary" @click="formDrawerApi.open()"> 新增菜单 </Button>
      </template>
      <template #title="{ row }">
        <Space :size="6" align="center">
          <IconifyIcon v-if="row.icon" :icon="row.icon" class="size-4" />
          <span>{{ row.title }}</span>
        </Space>
      </template>
    </Grid>
  </Page>
</template>
