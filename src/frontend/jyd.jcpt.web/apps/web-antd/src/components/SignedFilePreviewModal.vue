<script lang="ts" setup>
import { computed, ref } from 'vue';

import { IconifyIcon } from '@vben/icons';

import { Button, message, Modal, Spin } from 'ant-design-vue';

import { isLikelyPdfDownload } from '#/utils';

defineOptions({ name: 'SignedFilePreviewModal' });

const props = withDefaults(
  defineProps<{
    /** 加载失败时的第二行提示 */
    failedHint?: string;
    /** 是否尽量占满视口高度（宽度仍限制，避免竖版 PDF 两侧黑边） */
    fullscreen?: boolean;
    /** 是否在底部展示「关闭 / 新窗口打开」 */
    showFooter?: boolean;
  }>(),
  {
    failedHint: '请关闭后于签署记录中使用链接新开页面查看或下载。',
    fullscreen: true,
    showFooter: false,
  },
);

const modalOpen = ref(false);
const title = ref('已签署文件预览');
const sourceUrl = ref('');
const blobUrl = ref('');
const loading = ref(false);
let abortController: AbortController | null = null;

const previewPaneClass = computed(() => (props.fullscreen ? 'h-[calc(100vh-110px)] min-h-[70vh]' : 'min-h-[75vh]'));

function revokeBlob() {
  const b = blobUrl.value.trim();
  if (!b) return;
  URL.revokeObjectURL(b);
  blobUrl.value = '';
}

function reset() {
  abortController?.abort();
  abortController = null;
  revokeBlob();
  sourceUrl.value = '';
  loading.value = false;
}

function close() {
  modalOpen.value = false;
  reset();
}

function open(url: string, fileName?: null | string) {
  const u = url.trim();
  if (!u) return;
  if (!isLikelyPdfDownload(u, fileName)) {
    message.warning('该文件可能不是 PDF 或无法在弹窗中内嵌，请使用新开页面打开');
    return;
  }
  abortController?.abort();
  revokeBlob();
  abortController = new AbortController();
  const { signal } = abortController;
  title.value = fileName?.trim() || '已签署文件预览';
  sourceUrl.value = u;
  modalOpen.value = true;
  loading.value = true;
  void (async () => {
    try {
      const res = await fetch(u, { mode: 'cors', cache: 'no-store', signal });
      if (!res.ok) {
        message.warning(`无法在页面内加载文件（HTTP ${res.status}），请使用「新窗口打开」`);
        return;
      }
      const blob = await res.blob();
      const pdfBlob = blob.type === 'application/pdf' ? blob : new Blob([blob], { type: 'application/pdf' });
      if (signal.aborted) return;
      blobUrl.value = URL.createObjectURL(pdfBlob);
    } catch (error) {
      if (error instanceof DOMException && error.name === 'AbortError') return;
      message.warning('无法在页面内预览（多为 OSS 未放行跨域读取），请使用「新窗口打开」');
    } finally {
      if (!signal.aborted) loading.value = false;
    }
  })();
}

function openInNewTab() {
  const u = sourceUrl.value.trim();
  if (!u) return;
  window.open(u, '_blank', 'noopener,noreferrer');
}

defineExpose({ open, close });
</script>

<template>
  <Modal
    v-model:open="modalOpen"
    :title="title"
    centered
    width="min(1200px, 96vw)"
    :footer="showFooter ? undefined : null"
    destroy-on-close
    @after-close="reset"
  >
    <div :class="previewPaneClass">
      <Spin :spinning="loading" class="block w-full [&_.ant-spin-container]:w-full">
        <iframe
          v-if="blobUrl"
          :src="blobUrl"
          title="PDF 预览"
          class="block w-full rounded-sm border-0 bg-gray-50"
          :class="fullscreen ? 'h-[calc(100vh-110px)]' : 'h-[75vh]'"
        ></iframe>
        <div
          v-else-if="!loading"
          class="flex flex-col items-center justify-center gap-2 px-4 text-center text-sm text-gray-500"
          :class="fullscreen ? 'h-[calc(100vh-110px)]' : 'h-[75vh]'"
        >
          <span>未能在页面内加载预览。</span>
          <span>{{ failedHint }}</span>
        </div>
      </Spin>
    </div>
    <template v-if="showFooter" #footer>
      <div class="flex flex-wrap justify-end gap-2">
        <Button @click="close">关闭</Button>
        <Button type="primary" :disabled="!sourceUrl.trim()" @click="openInNewTab">
          <IconifyIcon icon="mdi:open-in-new" class="mr-1 inline size-4" />
          新窗口打开
        </Button>
      </div>
    </template>
  </Modal>
</template>
