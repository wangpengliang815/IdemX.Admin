<script lang="ts" setup>
import type { SysMenuApiEndpointResp, SysMenuImportButtonItemReq } from '#/api/system/menu';

import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Badge, Checkbox, Empty, Input, message, Space, Spin } from 'ant-design-vue';

import * as menuApi from '#/api/system/menu';

interface ImportButtonModalData {
  menuId?: string;
  menuName?: string;
}

const emit = defineEmits<{ success: [] }>();

const loading = ref(false);
const flatRows = ref<SysMenuApiEndpointResp[]>([]);
const selectedKeys = ref<string[]>([]);
const keyword = ref('');

const selectedCount = computed(() => selectedKeys.value.length);

const visibleRows = computed(() => {
  const k = keyword.value.trim().toLowerCase();
  if (!k) return flatRows.value;
  return flatRows.value.filter(
    (row) =>
      row.controllerTitle.toLowerCase().includes(k) ||
      row.controllerName.toLowerCase().includes(k) ||
      row.actionName.toLowerCase().includes(k) ||
      (row.description && row.description.toLowerCase().includes(k)),
  );
});

const visibleKeys = computed(() => visibleRows.value.map((row) => row.key));
const isAllSelected = computed(() => visibleKeys.value.length > 0 && visibleKeys.value.every((key) => selectedKeys.value.includes(key)));
const isIndeterminate = computed(() => {
  const hit = visibleKeys.value.filter((key) => selectedKeys.value.includes(key)).length;
  return hit > 0 && hit < visibleKeys.value.length;
});

const [ImportButtonModal, modalApi] = useVbenModal({
  onConfirm: () => handleOk(),
  onOpenChange: async (isOpen) => {
    if (isOpen) {
      const data = modalApi.getData() as ImportButtonModalData | undefined;
      modalApi.setState({ title: data?.menuName ? `导入按钮 - ${data.menuName}` : '导入按钮' });
      await loadEndpoints();
    } else {
      selectedKeys.value = [];
      flatRows.value = [];
      keyword.value = '';
    }
  },
});

async function loadEndpoints() {
  loading.value = true;
  modalApi.setState({ loading: true });
  try {
    const data = modalApi.getData() as ImportButtonModalData | undefined;
    if (!data?.menuId) {
      flatRows.value = [];
      selectedKeys.value = [];
      return;
    }

    const [endpointResult, buttonsResult] = await Promise.all([menuApi.getApiEndpointsApi(), menuApi.getButtonsApi(data.menuId)]);

    if (endpointResult.code !== 0) {
      message.error(endpointResult.msg);
      flatRows.value = [];
      selectedKeys.value = [];
      return;
    }
    const endpointRows = endpointResult.data!;
    if (buttonsResult.code !== 0) {
      message.error(buttonsResult.msg);
      flatRows.value = [];
      selectedKeys.value = [];
      return;
    }

    flatRows.value = endpointRows;
    keyword.value = '';

    const buttons = buttonsResult.data;
    if (buttons === undefined) {
      flatRows.value = [];
      selectedKeys.value = [];
      return;
    }
    const importedAuthorities = new Set(buttons.map((btn) => btn.authority));
    selectedKeys.value = flatRows.value.filter((row) => importedAuthorities.has(`${row.controllerName}:${row.actionName}`)).map((row) => row.key);
  } finally {
    loading.value = false;
    modalApi.setState({ loading: false });
  }
}

function toggleKey(key: string, checked: boolean) {
  if (checked) {
    if (!selectedKeys.value.includes(key)) {
      selectedKeys.value = [...selectedKeys.value, key];
    }
  } else {
    selectedKeys.value = selectedKeys.value.filter((item) => item !== key);
  }
}

function onSelectAllChange(checked: boolean) {
  if (checked) {
    const next = new Set(selectedKeys.value);
    for (const key of visibleKeys.value) next.add(key);
    selectedKeys.value = [...next];
  } else {
    const drop = new Set(visibleKeys.value);
    selectedKeys.value = selectedKeys.value.filter((key) => !drop.has(key));
  }
}

async function handleOk() {
  const data = modalApi.getData() as ImportButtonModalData | undefined;
  if (!data?.menuId) {
    message.error('菜单ID无效');
    return;
  }
  if (selectedKeys.value.length === 0) {
    message.warning('请至少选择一个按钮');
    return;
  }
  modalApi.setState({ confirmLoading: true });
  try {
    const items: SysMenuImportButtonItemReq[] = flatRows.value
      .filter((row) => selectedKeys.value.includes(row.key))
      .map((row) => ({
        controllerName: row.controllerName,
        actionName: row.actionName,
        description: row.description,
      }));
    const result = await menuApi.importButtonsApi(data.menuId!, items);
    if (result.code === 0) {
      message.success(result.msg);
      modalApi.close();
      emit('success');
    } else {
      message.error(result.msg);
    }
  } finally {
    modalApi.setState({ confirmLoading: false });
  }
}
</script>

<template>
  <ImportButtonModal width="800px" content-class="px-6 py-4" @cancel="modalApi.close()">
    <div class="flex flex-col gap-3">
      <Space align="center" class="w-full shrink-0" style="justify-content: space-between" wrap>
        <Space align="center">
          <Checkbox
            :checked="isAllSelected"
            :indeterminate="isIndeterminate"
            :disabled="visibleRows.length === 0"
            @update:checked="(c: boolean) => onSelectAllChange(c)"
          >
            全选（当前列表）
          </Checkbox>
        </Space>
        <Space align="center">
          <span>已选择</span>
          <Badge :count="selectedCount" :show-zero="true" />
          <span>个按钮</span>
        </Space>
      </Space>

      <Spin :spinning="loading" class="min-h-[120px]">
        <div v-if="loading" class="min-h-[240px] rounded border border-dashed border-gray-200 bg-gray-50/30"></div>
        <div v-else-if="flatRows.length === 0" class="rounded border border-gray-200 p-8">
          <Empty description="未扫描到按钮" />
        </div>
        <div v-else class="overflow-hidden rounded border border-gray-200">
          <div class="border-b border-gray-100 bg-gray-50/80 px-3 py-2.5">
            <Input v-model:value="keyword" allow-clear class="w-full" placeholder="搜索控制器、接口名或描述，勾选需绑定的按钮" />
          </div>
          <div class="max-h-[420px] overflow-y-auto p-3">
            <Empty v-if="visibleRows.length === 0" description="无匹配筛选结果" />
            <ul v-else class="m-0 list-none space-y-2 p-0">
              <li v-for="row in visibleRows" :key="row.key" class="flex gap-3 rounded px-1 py-1.5 hover:bg-gray-50">
                <Checkbox :checked="selectedKeys.includes(row.key)" class="mt-0.5 shrink-0" @update:checked="(c: boolean) => toggleKey(row.key, c)" />
                <div class="min-w-0 flex-1 truncate text-sm font-medium text-gray-900">
                  <span class="text-xs text-gray-500">{{ row.controllerTitle }}</span>
                  <span class="mx-1 text-gray-300">/</span>
                  <span>{{ row.actionName }}</span>
                  <span v-if="row.description" class="ml-2 text-xs text-gray-500">{{ row.description }}</span>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </Spin>
    </div>
  </ImportButtonModal>
</template>
