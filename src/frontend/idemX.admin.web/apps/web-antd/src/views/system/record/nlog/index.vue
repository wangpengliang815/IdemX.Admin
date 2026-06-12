<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRecordNlogPageQueryReq, SysRecordNlogResp } from '#/api/system/record';

import { onMounted } from 'vue';

import { confirm, Page } from '@vben/common-ui';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { clearNLogDataApi, getNLogPageListApi } from '#/api/system/record';

import { useGridFormSchema, useTableColumns } from './data';

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: false,
    showCollapseButton: false,
  },
  gridOptions: {
    columns: useTableColumns(),
    height: 'auto',
    keepSource: true,
    pagerConfig: { autoHidden: false, pageSize: 30, pageSizes: [30, 50, 100] },
    proxyConfig: {
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }, formValues: Record<string, unknown>) => {
          const formData = (formValues ?? {}) as { logLevel?: string; timeRange?: [string, string] };
          const timeRange = formData.timeRange;
          const params: SysRecordNlogPageQueryReq = {
            page: page.currentPage,
            pageSize: page.pageSize,
            logLevel: formData.logLevel != null && formData.logLevel !== '' ? String(formData.logLevel) : undefined,
            endTime: timeRange?.[1],
            startTime: timeRange?.[0],
          };
          const result = await getNLogPageListApi(params);
          if (result.code !== 0) {
            message.error(result.msg);
            return { data: [], total: 0 };
          }
          return { data: result.data!, total: result.total! };
        },
      },
      response: { result: 'data', total: 'total' },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, zoom: true },
  } as VxeTableGridOptions<SysRecordNlogResp>,
});

onMounted(() => gridApi.query());

async function onClearData() {
  try {
    await confirm({
      title: '清空日志数据',
      content: '此操作将删除所有全局日志，且不可恢复。是否继续？',
      confirmText: '确定',
      cancelText: '取消',
    });
    const result = await clearNLogDataApi();
    if (result.code === 0) {
      message.success(result.msg);
      gridApi.query();
    } else {
      message.error(result.msg);
    }
  } catch (error) {
    const msg = (error as Error)?.message;
    if (msg && msg !== 'dialog cancelled') message.error(msg);
  }
}
</script>

<template>
  <Page auto-content-height>
    <Grid>
      <template #toolbar-tools>
        <Button type="primary" danger @click="onClearData"> 清空日志 </Button>
      </template>
    </Grid>
  </Page>
</template>
