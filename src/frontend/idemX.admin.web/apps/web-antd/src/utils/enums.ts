import type { ComputedRef } from 'vue';

import { computed } from 'vue';

import { useEnumsStore } from '#/store/enums';

/** 下拉框选项格式 */
export type EnumSelectOption = { label: string; value: number };

/** 带颜色的选项（表格 CellTag） */
export type EnumTagOption = EnumSelectOption & { color: string };

/**
 * 枚举选项 - 统一入口，通过是否传 colorByValue 区分用途：
 * - 不传：下拉框选项 { label, value }
 * - 传：表格 CellTag 选项 { label, value, color }
 */
export function useEnumOptions(enumName: string): ComputedRef<EnumSelectOption[]>;
export function useEnumOptions(enumName: string, colorByValue: Record<number, string>): ComputedRef<EnumTagOption[]>;
export function useEnumOptions(enumName: string, colorByValue?: Record<number, string>): ComputedRef<EnumSelectOption[] | EnumTagOption[]> {
  const enumsStore = useEnumsStore();
  const baseOptions = computed(() => {
    const list = enumsStore.getEnum(enumName);
    return list.map((item) => ({ label: item.label, value: item.value }));
  });
  if (!colorByValue) return baseOptions;
  return computed(() =>
    baseOptions.value.map((item) => ({
      ...item,
      color: colorByValue[item.value] ?? 'default',
    })),
  );
}

/**
 * 根据枚举名和 value 取展示文案（详情/列表纯展示）
 */
export function getEnumLabel(enumName: string, value: number): string {
  const enumsStore = useEnumsStore();
  const item = enumsStore.getEnum(enumName)?.find((e) => e.value === value);
  return item?.label ?? '';
}

/**
 * 按展示文案取枚举值（label 与后端 [Description] 一致）
 */
export function getEnumValue(enumName: string, label: string): number {
  const enumsStore = useEnumsStore();
  const item = enumsStore.getEnum(enumName).find((e) => e.label === label);
  return item!.value;
}
