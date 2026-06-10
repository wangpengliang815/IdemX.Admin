<script lang="ts" setup>
import type { ProductCategoryResp } from '#/api/opscenter/productCategory';

import { computed, onMounted, ref, watch } from 'vue';

import { IconifyIcon } from '@vben/icons';

import { Form, Input } from 'ant-design-vue';

import * as productCategoryApi from '#/api/opscenter/productCategory';

defineOptions({ name: 'ProductCategoryTriplePicker' });

const props = defineProps<{
  onlyShow?: boolean;
}>();

const emit = defineEmits<{
  change: [value: string[] | undefined];
}>();

const model = defineModel<string[] | undefined>();
const formItemContext = Form.useInjectFormItemContext();

const activeL1Id = ref<string | undefined>();
const activeL2Id = ref<string | undefined>();
const panelExpanded = ref(true);
const suppressNavClear = ref(false);

const l1List = ref<ProductCategoryResp[]>([]);
const l2List = ref<ProductCategoryResp[]>([]);
const l3List = ref<ProductCategoryResp[]>([]);
const l1Search = ref('');
const l2Search = ref('');
const l3Search = ref('');

function filterCategoryList(list: ProductCategoryResp[], keyword: string) {
  const k = keyword.trim().toLowerCase();
  if (!k) return list;
  return list.filter((item) => item.name.toLowerCase().includes(k));
}

const filteredL1List = computed(() => filterCategoryList(l1List.value, l1Search.value));
const filteredL2List = computed(() => filterCategoryList(l2List.value, l2Search.value));
const filteredL3List = computed(() => filterCategoryList(l3List.value, l3Search.value));

async function loadL1() {
  const result = await productCategoryApi.getListApi(props.onlyShow ?? false);
  if (result.code !== 0) {
    l1List.value = [];
    return;
  }
  l1List.value = result.data!;
}

async function loadL2(parentId: string) {
  const result = await productCategoryApi.getTreeNodesApi(parentId, props.onlyShow ?? false);
  if (result.code !== 0) {
    l2List.value = [];
    return;
  }
  l2List.value = result.data!;
}

async function loadL3(parentId: string) {
  const result = await productCategoryApi.getTreeNodesApi(parentId, props.onlyShow ?? false);
  if (result.code !== 0) {
    l3List.value = [];
    return;
  }
  l3List.value = result.data!;
}

async function syncPanelsFromModel(ids: string[] | undefined) {
  if (!ids?.length) return;
  if (l1List.value.length === 0) await loadL1();
  activeL1Id.value = ids[0];
  await loadL2(ids[0]!);
  if (ids.length >= 2) {
    activeL2Id.value = ids[1];
    await loadL3(ids[1]!);
  }
}

watch(
  () => model.value,
  (ids) => {
    if (!ids?.length) {
      panelExpanded.value = true;
      if (!suppressNavClear.value) {
        activeL1Id.value = undefined;
        activeL2Id.value = undefined;
        l2Search.value = '';
        l3Search.value = '';
        l2List.value = [];
        l3List.value = [];
      }
      suppressNavClear.value = false;
      return;
    }
    void syncPanelsFromModel(ids);
    if (ids.length === 3) panelExpanded.value = false;
  },
  { immediate: true },
);

onMounted(() => {
  void loadL1();
});

const selectedPath = computed(() => model.value);

const selectedDisplayText = computed(() => {
  const ids = selectedPath.value;
  if (!ids || ids.length !== 3) return '';
  const names: string[] = [];
  const l1 = l1List.value.find((item) => item.id === ids[0]);
  if (!l1) return '';
  names.push(l1.name);
  const l2 = l2List.value.find((item) => item.id === ids[1]);
  if (!l2) return '';
  names.push(l2.name);
  const l3 = l3List.value.find((item) => item.id === ids[2]);
  if (!l3) return '';
  names.push(l3.name);
  return names.join(' / ');
});

function isL1Active(id: string) {
  return selectedPath.value?.[0] === id || activeL1Id.value === id;
}

function isL2Active(id: string) {
  return selectedPath.value?.[1] === id || activeL2Id.value === id;
}

function l1ItemClass(id: string) {
  if (isL1Active(id)) return 'bg-blue-500 font-medium text-white';
  return 'text-slate-700 hover:bg-blue-50/80 hover:text-blue-600';
}

function l2ItemClass(id: string) {
  if (isL2Active(id)) return 'bg-blue-500 font-medium text-white';
  return 'text-slate-700 hover:bg-blue-50/80 hover:text-blue-600';
}

function l3ItemClass(id: string) {
  if (selectedPath.value?.[2] === id) return 'bg-blue-500 font-medium text-white';
  return 'text-slate-700 hover:bg-blue-50/80 hover:text-blue-600';
}

function chevronClass(active: boolean) {
  return active ? 'text-white/90' : 'text-slate-300 group-hover:text-blue-400';
}

function pickL1(node: ProductCategoryResp) {
  panelExpanded.value = true;
  if (activeL1Id.value === node.id) return;
  activeL1Id.value = node.id;
  activeL2Id.value = undefined;
  l2Search.value = '';
  l3Search.value = '';
  l3List.value = [];
  void loadL2(node.id);
  suppressNavClear.value = true;
  model.value = undefined;
  emit('change', undefined);
}

function pickL2(node: ProductCategoryResp) {
  if (!activeL1Id.value) return;
  const sameL2 = activeL2Id.value === node.id;
  activeL2Id.value = node.id;
  l3Search.value = '';
  void loadL3(node.id);
  if (sameL2 && model.value?.length === 3) return;
  suppressNavClear.value = true;
  model.value = undefined;
  emit('change', undefined);
}

function pickL3(node: ProductCategoryResp) {
  if (!activeL1Id.value || !activeL2Id.value) return;
  const path = [activeL1Id.value, activeL2Id.value, node.id];
  model.value = path;
  panelExpanded.value = false;
  formItemContext.onFieldChange();
  emit('change', path);
}

function expandPanel() {
  void syncPanelsFromModel(model.value);
  panelExpanded.value = true;
}

const columnClass = 'min-h-0 flex-1 overflow-y-auto';
const columnWrapClass = 'flex max-h-56 min-h-[168px] flex-col';
const itemClass = 'group flex w-full items-center justify-between gap-1 px-3 py-2 text-left text-sm transition-colors duration-150';
const emptyClass = 'flex flex-col items-center justify-center gap-1.5 px-3 py-10 text-center text-xs text-slate-400';
const searchWrapClass = 'shrink-0 border-b border-slate-200/70 px-2 py-1.5';
</script>

<template>
  <div class="flex flex-col gap-2">
    <div
      v-if="selectedDisplayText && !panelExpanded"
      class="flex flex-wrap items-center gap-x-2 gap-y-1 rounded-lg bg-gradient-to-r from-blue-50 to-slate-50 px-3 py-2.5 text-sm ring-1 ring-blue-100/80"
    >
      <IconifyIcon icon="mdi:check-circle" class="size-4 shrink-0 text-blue-500" />
      <span class="text-slate-500">已选分类：</span>
      <button type="button" class="font-medium text-blue-600 transition-colors hover:text-blue-700 hover:underline" @click="expandPanel">
        {{ selectedDisplayText }}
      </button>
      <button type="button" class="ml-auto inline-flex items-center gap-1 text-slate-500 transition-colors hover:text-blue-600" @click="expandPanel">
        <IconifyIcon icon="mdi:pencil-outline" class="size-3.5" />
        重新选择
      </button>
    </div>

    <div v-if="panelExpanded" class="overflow-hidden rounded-lg border border-slate-200/80 bg-white shadow-sm ring-1 ring-slate-100">
      <div class="grid grid-cols-3 divide-x divide-slate-200/70 border-b border-slate-200/70 bg-slate-50/90">
        <div class="flex items-center gap-1.5 px-3 py-2 text-xs font-medium text-slate-600">
          <IconifyIcon icon="mdi:format-list-bulleted" class="size-3.5 text-blue-500" />
          一级分类
        </div>
        <div class="flex items-center gap-1.5 px-3 py-2 text-xs font-medium text-slate-600">
          <IconifyIcon icon="mdi:subdirectory-arrow-right" class="size-3.5 text-blue-500" />
          二级分类
        </div>
        <div class="flex items-center gap-1.5 px-3 py-2 text-xs font-medium text-slate-600">
          <IconifyIcon icon="mdi:tag-outline" class="size-3.5 text-blue-500" />
          三级分类
        </div>
      </div>
      <div class="grid grid-cols-3 divide-x divide-slate-200/70">
        <div class="bg-slate-50/60" :class="columnWrapClass">
          <div :class="searchWrapClass">
            <Input v-model:value="l1Search" size="small" placeholder="搜索一级分类" allow-clear />
          </div>
          <div :class="columnClass">
            <button v-for="item in filteredL1List" :key="item.id" type="button" :class="[itemClass, l1ItemClass(item.id)]" @click="pickL1(item)">
              <span class="min-w-0 truncate">{{ item.name }}</span>
              <IconifyIcon v-if="item.hasChild" icon="mdi:chevron-right" class="size-4 shrink-0" :class="chevronClass(isL1Active(item.id))" />
            </button>
            <div v-if="l1List.length === 0" :class="emptyClass">
              <IconifyIcon icon="mdi:folder-outline" class="size-5 text-slate-300" />
              暂无分类
            </div>
            <div v-else-if="filteredL1List.length === 0" :class="emptyClass">
              <IconifyIcon icon="mdi:magnify" class="size-5 text-slate-300" />
              无匹配分类
            </div>
          </div>
        </div>
        <div :class="columnWrapClass">
          <div :class="searchWrapClass">
            <Input v-model:value="l2Search" size="small" placeholder="搜索二级分类" allow-clear :disabled="!activeL1Id" />
          </div>
          <div :class="columnClass">
            <template v-if="activeL1Id">
              <button v-for="item in filteredL2List" :key="item.id" type="button" :class="[itemClass, l2ItemClass(item.id)]" @click="pickL2(item)">
                <span class="min-w-0 truncate">{{ item.name }}</span>
                <IconifyIcon v-if="item.hasChild" icon="mdi:chevron-right" class="size-4 shrink-0" :class="chevronClass(isL2Active(item.id))" />
              </button>
              <div v-if="l2List.length === 0" :class="emptyClass">
                <IconifyIcon icon="mdi:folder-open-outline" class="size-5 text-slate-300" />
                暂无二级分类
              </div>
              <div v-else-if="filteredL2List.length === 0" :class="emptyClass">
                <IconifyIcon icon="mdi:magnify" class="size-5 text-slate-300" />
                无匹配分类
              </div>
            </template>
            <div v-else :class="emptyClass">
              <IconifyIcon icon="mdi:gesture-tap" class="size-5 text-slate-300" />
              请先选择一级分类
            </div>
          </div>
        </div>
        <div :class="columnWrapClass">
          <div :class="searchWrapClass">
            <Input v-model:value="l3Search" size="small" placeholder="搜索三级分类" allow-clear :disabled="!activeL2Id" />
          </div>
          <div :class="columnClass">
            <template v-if="activeL2Id">
              <button v-for="item in filteredL3List" :key="item.id" type="button" :class="[itemClass, l3ItemClass(item.id)]" @click="pickL3(item)">
                <span class="min-w-0 truncate">{{ item.name }}</span>
              </button>
              <div v-if="l3List.length === 0" :class="emptyClass">
                <IconifyIcon icon="mdi:folder-open-outline" class="size-5 text-slate-300" />
                暂无三级分类
              </div>
              <div v-else-if="filteredL3List.length === 0" :class="emptyClass">
                <IconifyIcon icon="mdi:magnify" class="size-5 text-slate-300" />
                无匹配分类
              </div>
            </template>
            <div v-else :class="emptyClass">
              <IconifyIcon icon="mdi:gesture-tap" class="size-5 text-slate-300" />
              请先选择二级分类
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
