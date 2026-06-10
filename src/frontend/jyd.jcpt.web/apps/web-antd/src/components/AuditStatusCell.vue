<script lang="ts" setup>
import { computed } from 'vue';

import { Tag, Tooltip } from 'ant-design-vue';

export interface AuditStatusOption {
  color?: string;
  label: string;
  value: number;
}

const props = withDefaults(
  defineProps<{
    auditRemark?: null | string;
    auditTime?: null | string;
    auditUserName?: null | string;
    bordered?: boolean;
    options: AuditStatusOption[];
    value?: null | number;
  }>(),
  {
    auditRemark: undefined,
    auditTime: undefined,
    auditUserName: undefined,
    bordered: undefined,
    value: undefined,
  },
);

const matchedOption = computed(() => props.options.find((option) => option.value === props.value));

const tooltipText = computed(() => {
  const lines: string[] = [];
  if (props.auditUserName) lines.push(`审核人：${props.auditUserName}`);
  if (props.auditTime) lines.push(`审核时间：${props.auditTime}`);
  if (props.auditRemark) lines.push(`审核备注：${props.auditRemark}`);
  return lines.join('\n');
});
</script>

<template>
  <div class="inline-flex w-full min-w-0 items-center justify-center leading-none [&_.ant-tag]:my-0">
    <Tooltip v-if="matchedOption && tooltipText" placement="top">
      <template #title>
        <div class="whitespace-pre-line text-left">
          {{ tooltipText }}
        </div>
      </template>
      <Tag :bordered="bordered" :color="matchedOption.color ?? 'default'" class="m-0 cursor-pointer">
        {{ matchedOption.label }}
      </Tag>
    </Tooltip>
    <Tag v-else-if="matchedOption" :bordered="bordered" :color="matchedOption.color ?? 'default'" class="m-0">
      {{ matchedOption.label }}
    </Tag>
  </div>
</template>
