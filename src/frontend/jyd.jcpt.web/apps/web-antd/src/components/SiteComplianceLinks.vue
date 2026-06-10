<script setup lang="ts">
defineOptions({
  name: 'SiteComplianceLinks',
});

const complianceLinks = [
  {
    href: 'https://beian.miit.gov.cn/',
    text: '京ICP备2025155896号-1',
  },
  {
    href: 'https://dxzhgl.miit.gov.cn/',
    text: 'EDI 京B2-20261329',
  },
] as const;
</script>

<template>
  <span class="site-compliance-links inline-flex flex-wrap items-center gap-x-4 gap-y-1">
    <a v-for="item in complianceLinks" :key="item.text" :href="item.href" class="transition-colors" rel="noopener noreferrer" target="_blank">
      {{ item.text }}
    </a>
  </span>
</template>
