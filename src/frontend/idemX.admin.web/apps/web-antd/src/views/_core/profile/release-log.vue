<script lang="ts" setup>
import { computed } from 'vue';

import { IconifyIcon } from '@vben/icons';

import { Card, Empty, Tag, Timeline, TimelineItem } from 'ant-design-vue';

import { listReleaseNotes } from '#/components/release-notice';

const appVersion = import.meta.env.VITE_APP_VERSION as string;
const notes = computed(() => listReleaseNotes());
</script>

<template>
  <div class="release-log-page">
    <Card v-if="notes.length > 0" :bordered="false" :body-style="{ padding: '32px 24px 24px' }" class="release-log-card">
      <Timeline class="release-log-timeline">
        <TimelineItem v-for="(note, index) in notes" :key="note.version" :color="index === 0 ? '#b45309' : 'gray'">
          <template #dot>
            <span class="release-log-dot" :class="{ 'release-log-dot--latest': index === 0 }">
              <IconifyIcon icon="mdi:tag-outline" class="size-3.5" />
            </span>
          </template>

          <div class="release-log-item">
            <div class="release-log-item-head">
              <div class="release-log-item-meta">
                <span class="release-log-date">{{ note.publishedAt }}</span>
                <Tag v-if="note.version === appVersion" color="warning" class="release-log-current">当前版本</Tag>
                <Tag class="release-log-version">v{{ note.version }}</Tag>
              </div>
            </div>

            <div v-if="note.features?.length" class="release-log-block release-log-block--feature">
              <div class="release-log-block-title">
                <IconifyIcon icon="mdi:sparkles" class="size-4" />
                新增功能
              </div>
              <ul class="release-log-list">
                <li v-for="(text, i) in note.features" :key="`f-${note.version}-${i}`">{{ text }}</li>
              </ul>
            </div>

            <div v-if="note.fixes?.length" class="release-log-block release-log-block--fix">
              <div class="release-log-block-title">
                <IconifyIcon icon="mdi:bug-check-outline" class="size-4" />
                Bug 修复
              </div>
              <ul class="release-log-list">
                <li v-for="(text, i) in note.fixes" :key="`x-${note.version}-${i}`">{{ text }}</li>
              </ul>
            </div>
          </div>
        </TimelineItem>
      </Timeline>
    </Card>

    <Card v-else :bordered="false" :body-style="{ padding: '48px 24px' }" class="release-log-card">
      <Empty description="暂无版本更新记录" />
    </Card>
  </div>
</template>

<style scoped>
.release-log-page {
  padding: 12px 16px 24px;
}

.release-log-card {
  border-radius: 16px;
}

.release-log-timeline :deep(.ant-timeline-item) {
  padding-bottom: 20px;
}

.release-log-timeline :deep(.ant-timeline-item-last) {
  padding-bottom: 0;
}

.release-log-timeline :deep(.ant-timeline-item-content) {
  top: 2px;
  margin-inline-start: 22px !important;
}

.release-log-dot {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  color: #78716c;
  background: #f5f5f4;
  border: 2px solid #fff;
  border-radius: 50%;
  box-shadow: 0 0 0 1px #e7e5e4;
}

.release-log-dot--latest {
  color: #b45309;
  background: #fef3c7;
  box-shadow: 0 0 0 1px #fde68a;
}

.release-log-item {
  padding: 14px 16px;
  margin-top: 0;
  margin-bottom: 4px;
  background: #fffdf8;
  border: 1px solid #efe9dd;
  border-radius: 14px;
}

.release-log-item-head {
  margin-bottom: 10px;
}

.release-log-item-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.release-log-date {
  font-size: 13px;
  font-weight: 600;
  color: #78716c;
}

.release-log-version {
  margin: 0;
  font-size: 12px;
  border-radius: 6px;
}

.release-log-current {
  margin: 0;
  font-size: 12px;
  border-radius: 6px;
}

.release-log-block + .release-log-block {
  margin-top: 10px;
}

.release-log-block {
  padding: 10px 12px;
  border-radius: 10px;
}

.release-log-block--feature {
  background: #f0fdf4;
  border: 1px solid #dcfce7;
}

.release-log-block--fix {
  background: #fffbeb;
  border: 1px solid #fde68a;
}

.release-log-block-title {
  display: flex;
  gap: 6px;
  align-items: center;
  margin-bottom: 8px;
  font-size: 13px;
  font-weight: 600;
  color: #44403c;
}

.release-log-block--feature .release-log-block-title {
  color: #15803d;
}

.release-log-block--fix .release-log-block-title {
  color: #b45309;
}

.release-log-list {
  padding-left: 18px;
  margin: 0;
  font-size: 13px;
  line-height: 1.6;
  color: #44403c;
}

.release-log-list li + li {
  margin-top: 4px;
}
</style>
