import type { AllEnumsMap, EnumOptionItemResp } from '#/api/public/tools';

import { ref } from 'vue';

import { defineStore } from 'pinia';

import * as toolsApi from '#/api/public/tools';

export const useEnumsStore = defineStore('enums', () => {
  const enums = ref<AllEnumsMap>({});
  const loading = ref(false);
  const error = ref<null | string>(null);

  /**
   * 加载所有枚举数据（已有数据或正在加载时不再请求，避免重复调用 getEnum）
   * @param force - 为 true 时先清空再加载，用于刷新
   */
  async function loadEnums(force?: boolean) {
    if (loading.value && !force) return;
    if (!force && enums.value && Object.keys(enums.value).length > 0) {
      return;
    }
    if (force) {
      enums.value = {};
    }
    loading.value = true;
    error.value = null;
    try {
      const result = await toolsApi.getEnums();
      if (result.code === 0 && result.data) {
        enums.value = result.data;
      } else {
        error.value = result.msg ?? '';
      }
    } catch (error_: unknown) {
      error.value = error_ instanceof Error ? error_.message : String(error_);
    } finally {
      loading.value = false;
    }
  }

  /**
   * 获取指定枚举的选项列表
   * @param enumName - 枚举名称
   * @returns 枚举选项数组
   */
  function getEnum(enumName: string): EnumOptionItemResp[] {
    return enums.value[enumName] ?? [];
  }

  /**
   * 刷新枚举数据（清空后重新请求）
   */
  async function refreshEnums() {
    await loadEnums(true);
  }

  /**
   * 重置 store 状态
   */
  function $reset() {
    enums.value = {};
    loading.value = false;
    error.value = null;
  }

  return {
    enums,
    loading,
    error,
    loadEnums,
    getEnum,
    refreshEnums,
    $reset,
  };
});
