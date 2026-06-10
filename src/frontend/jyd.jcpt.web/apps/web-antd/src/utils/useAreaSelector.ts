import type { SysAreaItemResp } from '#/api/public/tools';

import { ref } from 'vue';

import * as toolsApi from '#/api/public/tools';
import { logger } from '#/utils/logger';

export interface AreaCascaderOption {
  label: string;
  value: string;
  isLeaf?: boolean;
  loading?: boolean;
  level?: number;
  name?: string;
  code?: string;
  children?: AreaCascaderOption[];
}

export interface AreaChangeResult {
  areaCodes: string[] | undefined;
  areaCode: string;
  areaName: string;
  cityCode: string;
  cityName: string;
  provinceCode: string;
  provinceName: string;
}

export function useAreaSelector() {
  const areaOptions = ref<AreaCascaderOption[]>([]);

  async function loadAreaData(level: number, parentCode: string): Promise<AreaCascaderOption[]> {
    try {
      const result = await toolsApi.getAreaByCode({
        level,
        parentCode,
      });
      return result.map((item: SysAreaItemResp) => ({
        label: item.name,
        value: String(item.code),
        isLeaf: level >= 3,
        level,
        name: item.name,
        code: String(item.code),
      }));
    } catch (error: unknown) {
      logger.error('加载省市区数据失败:', error);
      return [];
    }
  }

  async function initProvinceData() {
    const provinces = await loadAreaData(1, '0');
    areaOptions.value = provinces;
  }

  async function loadAreaChildren(selectedOptions: AreaCascaderOption[]) {
    const targetOption = selectedOptions[selectedOptions.length - 1];
    if (!targetOption || targetOption.children) return;

    targetOption.loading = true;

    try {
      const level = selectedOptions.length + 1;
      const parentCode = String(targetOption.value || '');
      const children = await loadAreaData(level, parentCode);
      targetOption.children = children;
    } finally {
      targetOption.loading = false;
    }
  }

  async function loadCascaderDataForEdit(codes: string[]) {
    if (!codes || codes.length !== 3) return;

    await initProvinceData();

    const [provinceCode, cityCode] = codes;

    const provinceOption = areaOptions.value.find((opt) => opt.value === provinceCode);
    if (provinceOption) {
      await loadAreaChildren([provinceOption]);
      const cityOption = provinceOption.children?.find((opt) => opt.value === cityCode);
      cityOption && (await loadAreaChildren([provinceOption, cityOption]));
    }
  }

  function handleAreaChange(value: (number | string)[] | undefined, selectedOptions?: AreaCascaderOption[]): AreaChangeResult {
    const values = Array.isArray(value) ? value.map(String) : [];
    const options = Array.isArray(selectedOptions) ? selectedOptions : [];

    return values.length === 3
      ? {
          areaCodes: values,
          provinceCode: values[0] || '',
          provinceName: options[0]?.label || '',
          cityCode: values[1] || '',
          cityName: options[1]?.label || '',
          areaCode: values[2] || '',
          areaName: options[2]?.label || '',
        }
      : {
          areaCodes: undefined,
          provinceCode: '',
          provinceName: '',
          cityCode: '',
          cityName: '',
          areaCode: '',
          areaName: '',
        };
  }

  return {
    areaOptions,
    initProvinceData,
    loadAreaChildren,
    loadCascaderDataForEdit,
    handleAreaChange,
  };
}
