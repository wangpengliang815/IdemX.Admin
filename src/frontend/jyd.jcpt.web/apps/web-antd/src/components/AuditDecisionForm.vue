<script lang="ts" setup>
import type { FormInstance } from 'ant-design-vue';

import { computed, ref } from 'vue';

import { IconifyIcon } from '@vben/icons';

import {
  Checkbox as ACheckbox,
  CheckboxGroup as ACheckboxGroup,
  Form as AForm,
  FormItem as AFormItem,
  FormItemRest as AFormItemRest,
  Textarea as ATextarea,
  Card,
  Col,
  message,
  Row,
  Space,
  Tag,
  Typography,
} from 'ant-design-vue';

export interface ConfirmItem {
  key: string;
  label: string;
}
export type ConfirmItemsInput = ConfirmItem[] | readonly ConfirmItem[];

export type CardConfirmations = Record<string, boolean>;

type AuditDecisionModel = {
  auditRemark: string;
  confirmations?: CardConfirmations;
  isApproved: boolean;
};

const props = defineProps<{
  confirmItems?: ConfirmItemsInput;
  decisionTitle: string;
  modelValue: AuditDecisionModel;
  quickReasons?: string[];
  remarkMaxLength?: number;
  remarkPlaceholder?: string;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', v: AuditDecisionModel): void;
}>();

const formRef = ref<FormInstance>();

const isApproved = computed<boolean>({
  get: () => props.modelValue.isApproved,
  set: (v) => emit('update:modelValue', { ...props.modelValue, isApproved: v }),
});

const auditRemark = computed<string>({
  get: () => props.modelValue.auditRemark,
  set: (v) => emit('update:modelValue', { ...props.modelValue, auditRemark: v }),
});

const confirmations = computed<CardConfirmations>({
  get: () => props.modelValue.confirmations ?? {},
  set: (v) => emit('update:modelValue', { ...props.modelValue, confirmations: v }),
});

const confirmItems = computed(() => props.confirmItems ?? []);
const needConfirmations = computed(() => props.modelValue.isApproved === true && confirmItems.value.length > 0);

const confirmationOptions = computed(() =>
  confirmItems.value.map((item) => ({
    label: `我已确认「${item.label}」准确无误`,
    value: item.key,
  })),
);

const checkedConfirmKeys = computed<string[]>({
  get: () => confirmItems.value.filter((item) => confirmations.value[item.key] === true).map((item) => item.key),
  set: (keys) => {
    const checkedKeySet = new Set(keys);
    const next = { ...confirmations.value };
    confirmItems.value.forEach((item) => {
      next[item.key] = checkedKeySet.has(item.key);
    });
    confirmations.value = next;
  },
});

const allConfirmed = computed(() => confirmItems.value.length > 0 && confirmItems.value.every((item) => confirmations.value[item.key] === true));

function handleToggleAllConfirmations(checked: boolean) {
  checkedConfirmKeys.value = checked ? confirmItems.value.map((item) => item.key) : [];
}

const rules: any = {
  isApproved: [{ required: true, message: '请选择审核结果', trigger: 'change' }],
  auditRemark: [
    {
      required: true,
      message: '拒绝时必须填写拒绝原因',
      validator: (_rule: any, value: string) => {
        if (isApproved.value === false && (!value || !value.trim())) {
          return Promise.reject(_rule.message);
        }
        return Promise.resolve();
      },
      trigger: [],
    },
  ],
};

const quickReasons = computed(() => props.quickReasons ?? []);

function appendQuickReason(reason: string) {
  const next = auditRemark.value ? `${auditRemark.value}\n${reason}` : reason;
  auditRemark.value = next;
}

async function validate(): Promise<boolean> {
  const form = formRef.value;
  if (!form) return true;
  try {
    await form.validate();
  } catch {
    return false;
  }
  if (needConfirmations.value) {
    const items = props.confirmItems ?? [];
    const all = items.every((item) => confirmations.value[item.key] === true);
    if (!all) {
      message.error('审核通过前请勾选确认上述各项信息准确无误');
      return false;
    }
  }
  return true;
}

defineExpose({
  validate,
});
</script>

<template>
  <AForm ref="formRef" layout="vertical" :model="modelValue" :rules="rules">
    <div class="rounded-xl border border-[#f0f0f0] bg-white p-px">
      <Space direction="vertical" :size="20" class="overflow-hidden rounded-[11px] bg-[#fafafa] p-4" style="width: 100%">
        <AFormItem class="!mb-0" name="isApproved">
          <Card :bordered="false" :head-style="{ borderBottom: 'none' }" :title="decisionTitle">
            <Row :gutter="[16, 16]" class="-mt-4">
              <Col :xs="24" :sm="12">
                <div
                  class="cursor-pointer rounded-lg px-4 py-[18px] text-center transition-[background-color,border-color]"
                  :class="isApproved === true ? 'border-2 border-solid border-[#52c41a] bg-[#f6ffed]' : 'border border-solid border-[#d9d9d9] bg-[#fafafa]'"
                  @click="isApproved = true"
                >
                  <Typography.Text strong class="text-[#389e0d]">通过</Typography.Text>
                  <Typography.Text type="secondary" class="mt-1 block text-xs">信息无误</Typography.Text>
                </div>
              </Col>
              <Col :xs="24" :sm="12">
                <div
                  class="cursor-pointer rounded-lg px-4 py-[18px] text-center transition-[background-color,border-color]"
                  :class="isApproved === false ? 'border-2 border-solid border-[#ff7875] bg-[#fff2f0]' : 'border border-solid border-[#d9d9d9] bg-[#fafafa]'"
                  @click="isApproved = false"
                >
                  <Typography.Text strong class="text-[#cf1322]">拒绝</Typography.Text>
                  <Typography.Text type="secondary" class="mt-1 block text-xs">需说明原因以便修改</Typography.Text>
                </div>
              </Col>
            </Row>
          </Card>
        </AFormItem>

        <div v-if="needConfirmations" class="!mb-0">
          <Card :bordered="false" :head-style="{ borderBottom: 'none' }">
            <template #title>
              <Space align="center" :size="10">
                <span class="flex size-9 items-center justify-center rounded-lg bg-[#f6ffed] text-[#52c41a]">
                  <IconifyIcon icon="mdi:playlist-check" class="size-5" aria-hidden="true" />
                </span>
                <span class="text-base font-semibold">审核确认项</span>
              </Space>
            </template>
            <Typography.Paragraph type="secondary" class="-mt-4 !text-xs">通过前请逐项核对下列信息</Typography.Paragraph>
            <AFormItemRest>
              <div class="overflow-hidden rounded-lg border border-[#f0f0f0] bg-white">
                <ACheckboxGroup v-model:value="checkedConfirmKeys" class="flex w-full flex-col">
                  <div v-for="opt in confirmationOptions" :key="opt.value" class="border-b border-[#f0f0f0] px-4 py-3 last:border-b-0">
                    <ACheckbox :value="opt.value">
                      <Typography.Text class="text-sm">{{ opt.label }}</Typography.Text>
                    </ACheckbox>
                  </div>
                </ACheckboxGroup>
                <div class="border-t border-[#f0f0f0] bg-[#fafafa] px-4 py-3">
                  <ACheckbox :checked="allConfirmed" @update:checked="handleToggleAllConfirmations">
                    <Typography.Text strong class="text-sm">我已确认以上信息全部无误</Typography.Text>
                  </ACheckbox>
                </div>
              </div>
            </AFormItemRest>
          </Card>
        </div>

        <AFormItem v-if="isApproved === false" class="!mb-0" name="auditRemark" :required="true">
          <Card :bordered="false" :head-style="{ borderBottom: 'none' }">
            <template #title>
              <Space align="center" :size="10">
                <span class="flex size-9 items-center justify-center rounded-lg bg-[#fff2f0] text-[#ff4d4f]">
                  <IconifyIcon icon="mdi:comment-alert-outline" class="size-5" aria-hidden="true" />
                </span>
                <span class="text-base font-semibold">拒绝原因</span>
              </Space>
            </template>
            <Typography.Paragraph type="secondary" class="!mb-3 !text-xs">请具体说明问题，便于用户修改后重新提交。</Typography.Paragraph>
            <Space v-if="quickReasons.length > 0" direction="vertical" :size="10" class="w-full">
              <Typography.Text type="secondary" class="text-xs">快捷填入</Typography.Text>
              <Space wrap :size="8">
                <Tag
                  v-for="reason in quickReasons"
                  :key="reason"
                  color="blue"
                  class="m-0 cursor-pointer border-0 px-3 py-0.5"
                  @click="appendQuickReason(reason)"
                >
                  {{ reason }}
                </Tag>
              </Space>
            </Space>
            <ATextarea
              v-model:value="auditRemark"
              class="mt-3"
              :placeholder="remarkPlaceholder ?? '请填写拒绝原因（必填），或点击上方快捷标签填入'"
              :rows="5"
              :maxlength="remarkMaxLength ?? 500"
              show-count
              allow-clear
            />
          </Card>
        </AFormItem>
      </Space>
    </div>
  </AForm>
</template>
