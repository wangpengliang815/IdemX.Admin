<script lang="ts" setup>
import type { TreeProps } from 'ant-design-vue';

import type { SysRoleMenuMapTreeNodeResp } from '#/api/system/role';

import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Badge, Checkbox, message, Space, Spin, Tag, Tree } from 'ant-design-vue';

import * as roleApi from '#/api/system/role';

interface ImportPermissionModalData {
  roleId?: string;
  roleName?: string;
}

const emit = defineEmits<{ success: [] }>();

const loading = ref(false);
const treeData = ref<SysRoleMenuMapTreeNodeResp[]>([]);
const checkedKeys = ref<string[]>([]);
const halfCheckedKeys = ref<string[]>([]);
const expandedKeys = ref<string[]>([]);

function collectAllKeys(nodes: SysRoleMenuMapTreeNodeResp[]): string[] {
  const keys: string[] = [];
  function walk(n: SysRoleMenuMapTreeNodeResp[]) {
    for (const node of n) {
      keys.push(node.key);
      if (node.children.length > 0) walk(node.children);
    }
  }
  walk(nodes);
  return keys;
}

function buildKeyToMenuType(nodes: SysRoleMenuMapTreeNodeResp[]): Map<string, number> {
  const map = new Map<string, number>();
  function walk(n: SysRoleMenuMapTreeNodeResp[]) {
    for (const node of n) {
      map.set(node.key, node.menuType);
      if (node.children.length > 0) walk(node.children);
    }
  }
  walk(nodes);
  return map;
}

const validKeySet = computed(() => new Set(collectAllKeys(treeData.value)));
const keyToMenuType = computed(() => buildKeyToMenuType(treeData.value));
const selectedKeysSet = computed(() => new Set([...checkedKeys.value, ...halfCheckedKeys.value]));
const selectedMenuCount = computed(() => [...selectedKeysSet.value].filter((k) => validKeySet.value.has(k) && keyToMenuType.value.get(k) === 0).length);
const selectedButtonCount = computed(() => [...selectedKeysSet.value].filter((k) => validKeySet.value.has(k) && keyToMenuType.value.get(k) === 1).length);
const selectedCount = computed(() => selectedMenuCount.value + selectedButtonCount.value);
const allKeys = computed(() => collectAllKeys(treeData.value));
const isAllSelected = computed(() => allKeys.value.length > 0 && allKeys.value.every((k) => checkedKeys.value.includes(k)));
const isAllIndeterminate = computed(() => selectedCount.value > 0 && !isAllSelected.value);

const [ImportPermissionModal, modalApi] = useVbenModal({
  onConfirm: () => handleOk(),
  onOpenChange: async (isOpen) => {
    if (isOpen) {
      const data = modalApi.getData() as ImportPermissionModalData | undefined;
      modalApi.setState({ title: data?.roleName ? `权限分配 - ${data.roleName}` : '权限分配' });
      await loadRoleMenuMap();
    } else {
      checkedKeys.value = [];
      halfCheckedKeys.value = [];
      treeData.value = [];
    }
  },
});

async function loadRoleMenuMap() {
  const data = modalApi.getData() as ImportPermissionModalData | undefined;
  if (!data?.roleId) return;

  loading.value = true;
  modalApi.setState({ loading: true });
  try {
    const result = await roleApi.getRoleMenuMapApi(data.roleId);
    if (result.code !== 0) {
      message.error(result.msg);
      return;
    }
    const res = result.data!;
    treeData.value = res.treeData;
    checkedKeys.value = res.checkedKeys.map(String);
    halfCheckedKeys.value = res.halfCheckedKeys.map(String);
    expandedKeys.value = [];
  } finally {
    loading.value = false;
    modalApi.setState({ loading: false });
  }
}

function onSelectAllChange(checked: boolean) {
  checkedKeys.value = checked ? [...allKeys.value] : [];
  halfCheckedKeys.value = [];
}

const onCheck: TreeProps['onCheck'] = (keys, info) => {
  if (Array.isArray(keys)) {
    checkedKeys.value = keys as string[];
    halfCheckedKeys.value = (info?.halfCheckedKeys ?? []) as string[];
    return;
  }
  checkedKeys.value = keys.checked as string[];
  halfCheckedKeys.value = keys.halfChecked as string[];
};

async function handleOk() {
  const data = modalApi.getData() as ImportPermissionModalData | undefined;
  if (!data?.roleId) {
    message.error('角色ID无效');
    return;
  }
  const finalKeys = [...new Set([...checkedKeys.value, ...halfCheckedKeys.value])].filter((k) => validKeySet.value.has(k));
  // 允许空提交：与后端 setRoleMenuMap 一致，表示清空该角色全部菜单权限

  modalApi.setState({ confirmLoading: true });
  try {
    const result = await roleApi.setRoleMenuMapApi(data.roleId, finalKeys);
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
  <ImportPermissionModal width="600px" content-class="px-6 py-4" @cancel="modalApi.close()">
    <Space class="mb-3" align="center" style="width: 100%; justify-content: space-between">
      <Checkbox
        :checked="isAllSelected"
        :indeterminate="isAllIndeterminate"
        @change="(e: { target?: { checked?: boolean } }) => onSelectAllChange(!!(e && e.target && e.target.checked))"
      >
        全选
      </Checkbox>
      <Space align="center">
        <span>菜单</span>
        <Badge :count="selectedMenuCount" :show-zero="true" />
        <span>项</span>
        <span>按钮</span>
        <Badge :count="selectedButtonCount" :show-zero="true" />
        <span>项</span>
      </Space>
    </Space>

    <Spin :spinning="loading">
      <div class="max-h-[450px] overflow-y-auto rounded border border-gray-200 p-3">
        <Tree
          v-model:expanded-keys="expandedKeys"
          :checked-keys="{ checked: checkedKeys, halfChecked: halfCheckedKeys }"
          checkable
          block-node
          :tree-data="treeData"
          :field-names="{ title: 'title', key: 'key', children: 'children' }"
          @check="onCheck"
        >
          <template #title="{ title, menuType }">
            <Tag v-if="menuType === 0" color="blue" class="!m-0 text-xs">{{ title }}</Tag>
            <span v-else class="text-sm">{{ title }}</span>
          </template>
        </Tree>
      </div>
    </Spin>
  </ImportPermissionModal>
</template>
