export * from './create-icon';

export * from './lucide';

export type { IconifyIcon as IconifyIconStructure } from '@iconify/vue/offline';
export {
  addCollection,
  addIcon,
  Icon as IconifyIcon,
} from '@iconify/vue/offline';

/** 图标选择器等场景列举已加载集合（不触发 CDN） */
export { listIcons } from '@iconify/vue';
