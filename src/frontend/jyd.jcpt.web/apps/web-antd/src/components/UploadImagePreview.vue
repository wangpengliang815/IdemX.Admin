<script lang="ts" setup>
import type { UploadFile } from 'ant-design-vue';

import { ref } from 'vue';

import { Image } from 'ant-design-vue';

defineOptions({ name: 'UploadImagePreview' });
/**
 * Upload 图片预览（仅用 AntD Image 的内置预览能力）
 * 用于 picture-card 的缩略图点击预览场景。
 */
const previewVisible = ref(false);
const previewUrl = ref('');

function setVisible(v: boolean) {
  previewVisible.value = v;
}

function handlePreview(file: UploadFile) {
  if (file.url) {
    previewUrl.value = file.url;
    previewVisible.value = true;
    return;
  }

  if (file.thumbUrl) {
    previewUrl.value = file.thumbUrl;
    previewVisible.value = true;
    return;
  }

  if (file.originFileObj) {
    previewUrl.value = URL.createObjectURL(file.originFileObj as Blob);
    previewVisible.value = true;
  }
}

defineExpose({
  handlePreview,
});
</script>

<template>
  <Image
    v-if="previewUrl"
    :src="previewUrl"
    :preview="{
      visible: previewVisible,
      onVisibleChange: setVisible,
    }"
    style="display: none"
  />
</template>
