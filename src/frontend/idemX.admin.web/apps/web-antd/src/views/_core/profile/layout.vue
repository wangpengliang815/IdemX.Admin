<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { createIconifyIcon } from '@vben/icons';

import { Button } from 'ant-design-vue';

const route = useRoute();
const router = useRouter();

const isHome = computed(() => route.path === '/profile');
const pageTitle = computed(() => (route.meta?.title as string) || '个人中心');

function handleBack() {
  const backPath = route.meta.backPath as string | undefined;
  void router.push(backPath ?? '/profile');
}
</script>

<template>
  <div class="profile-layout">
    <div v-if="!isHome" class="profile-layout-header">
      <Button type="text" class="back-btn" @click="handleBack">
        <component :is="createIconifyIcon('mdi:chevron-left')" />
        返回
      </Button>
      <div class="title">{{ pageTitle }}</div>
      <div class="right"></div>
    </div>

    <div class="profile-layout-content">
      <router-view />
    </div>
  </div>
</template>

<style scoped lang="less">
.profile-layout {
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
  background: #f6f8fb;
}

.profile-layout-content {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  overflow-x: hidden;
  overflow-anchor: none;
}

.profile-layout-header {
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 8px;
  padding: 12px 20px 0;
}

.back-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 0 6px;
  font-size: 14px;
  font-weight: 600;
  color: #111827;
}

.title {
  text-align: center;
  font-size: 14px;
  font-weight: 600;
  color: #6b7280;
}

.right {
  width: 56px;
}

@media (max-width: 768px) {
  .profile-layout-header {
    padding: 10px 12px 0;
  }
}

/* Card 带 title 时 Ant Design 默认给表头加底部分割线，个人中心统一去掉 */
:deep(.points-card--headed .ant-card-head) {
  border-bottom: none;
}
</style>
