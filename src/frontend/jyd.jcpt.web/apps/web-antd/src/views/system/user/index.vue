<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRoleResp } from '#/api/system/role';
import type { SysUserPageQueryReq, SysUserResp } from '#/api/system/user';

import { computed, h, nextTick, onMounted, ref, watch } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { IconifyIcon } from '@vben/icons';

import { Button, message, Segmented, Space, Switch, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import * as roleApi from '#/api/system/role';
import * as userApi from '#/api/system/user';
import { useEnumOptions } from '#/utils';

import { useGridFormSchema, useTableColumns } from './data';
import UserForm from './modules/form.vue';

/** UserType.内部用户 */
const USER_TYPE_INTERNAL = 0;
/** UserType.注册用户 */
const USER_TYPE_REGISTERED = 1;

const listScope = ref<'all' | 'internal' | 'registered'>('all');

function segmentLabel(icon: string, iconClass: string, title: string) {
  return h('div', { class: 'flex min-h-9 items-center gap-2 px-0.5 py-0.5' }, [
    h(IconifyIcon, { icon, class: `size-[20px] shrink-0 ${iconClass}` }),
    h('span', { class: 'text-sm font-semibold leading-tight text-slate-800' }, title),
  ]);
}

const listScopeSegmentOptions = computed(() => [
  { value: 'all', label: segmentLabel('mdi:account-group-outline', 'text-blue-600', '全部用户') },
  { value: 'internal', label: segmentLabel('mdi:account-tie-outline', 'text-indigo-600', '内部用户') },
  { value: 'registered', label: segmentLabel('mdi:account-plus-outline', 'text-cyan-600', '注册用户') },
]);

const roleOptions = ref<SysRoleResp[]>([]);
const stateOptions = useEnumOptions('UserStatus');
const userTypeOptions = useEnumOptions('UserType', { 0: 'blue', 1: 'cyan' });
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
    columns: useTableColumns(onActionClick, sexTagOptions.value, userTypeOptions.value),
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
            realName: formValues?.realName as string | undefined,
            status: formValues?.status as number | undefined,
          };
          if (listScope.value === 'internal') params.userType = USER_TYPE_INTERNAL;
          else if (listScope.value === 'registered') params.userType = USER_TYPE_REGISTERED;

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

watch(listScope, () => {
  gridApi.query();
});

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

    <div class="mb-3 shrink-0 rounded-xl border border-slate-200/90 bg-gradient-to-br from-slate-50/95 via-white to-blue-50/40 px-3 py-3 shadow-sm sm:px-4">
      <Segmented v-model:value="listScope" class="w-fit max-w-full" size="large" :options="listScopeSegmentOptions" />
    </div>

    <div class="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
      <Grid>
        <template #toolbar-tools>
          <Button type="primary" @click="formDrawerApi.open()"> 内部账号开通 </Button>
        </template>
        <template #roles="{ row }">
          <span v-if="row.roles && row.roles.length > 0">
            <Tag v-for="role in row.roles" :key="role.id" color="blue">
              {{ role.roleName }}
            </Tag>
          </span>
        </template>
        <template #state="{ row }">
          <Switch
            :checked="row.status === 0"
            checked-children="正常"
            un-checked-children="停用"
            @change="(checked: unknown) => void onStateChange(!!checked, row)"
          />
        </template>
        <template #wechatNo="{ row }">
          <Space :size="6" align="center">
            <IconifyIcon icon="mdi:wechat" class="h-4 w-4 text-green-500" />
            <span>{{ row.wechatNo }}</span>
          </Space>
        </template>
      </Grid>
    </div>
  </Page>
</template>
