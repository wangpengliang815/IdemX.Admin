import { createIconifyIcon } from '@vben/icons';

/**
 * 根据 Iconify 图标名称创建图标组件
 * @param iconName Iconify 格式的图标名称，如 'mdi:home'
 * @returns 图标组件，如果 iconName 为空则返回 null
 */
export function createIconComponent(iconName: null | string | undefined) {
  if (!iconName) {
    return null;
  }
  return createIconifyIcon(iconName);
}
