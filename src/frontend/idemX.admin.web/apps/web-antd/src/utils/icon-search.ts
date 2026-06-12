/**
 * 本地图标搜索工具
 * 使用静态导入常用图标集，避免动态导入问题
 */

import antDesignIcons from '@iconify/json/json/ant-design.json';
import carbonIcons from '@iconify/json/json/carbon.json';
// 静态导入常用图标集（Vite 会在构建时处理）
import mdiIcons from '@iconify/json/json/mdi.json';

// 图标集合映射
const iconCollections: Record<string, any> = {
  mdi: mdiIcons,
  'ant-design': antDesignIcons,
  carbon: carbonIcons,
};

/**
 * 从图标集合中搜索图标
 * @param query 搜索关键词
 * @param limit 返回结果数量限制
 * @param collections 要搜索的图标集前缀（如 ['mdi']），不传则搜索全部
 * @returns 匹配的图标名称数组（格式：prefix:name）
 */
export async function searchIconsLocally(query: string, limit = 100, collections?: string[]): Promise<string[]> {
  if (!query || query.trim().length < 2) {
    return [];
  }

  const keyword = query.toLowerCase().trim();
  const results: string[] = [];
  const maxResults = limit;

  // 确定要搜索的图标集
  const searchCollections =
    collections && collections.length > 0
      ? Object.entries(iconCollections).filter(([prefix]) => collections.includes(prefix))
      : Object.entries(iconCollections);

  // 遍历已加载的图标集
  for (const [prefix, collection] of searchCollections) {
    if (results.length >= maxResults) {
      break;
    }

    if (!collection || typeof collection !== 'object') {
      continue;
    }

    // @iconify/json 的结构：{ icons: { iconName: iconData } }
    const icons = collection.icons || collection;
    if (!icons || typeof icons !== 'object') {
      continue;
    }

    // 搜索图标名称
    for (const [iconName, iconData] of Object.entries(icons)) {
      if (results.length >= maxResults) {
        break;
      }

      // 匹配图标名称或别名
      const name = iconName.toLowerCase();
      const fullName = `${prefix}:${iconName}`.toLowerCase();

      let matched = false;

      // 检查图标名称
      if (name.includes(keyword) || fullName.includes(keyword)) {
        matched = true;
      }

      // 检查别名
      if (!matched && iconData && typeof iconData === 'object') {
        // 检查 aliases
        if ('aliases' in iconData && Array.isArray(iconData.aliases) && iconData.aliases.some((alias: string) => alias.toLowerCase().includes(keyword))) {
          matched = true;
        }

        // 检查 category
        if (!matched && 'category' in iconData && typeof iconData.category === 'string' && iconData.category.toLowerCase().includes(keyword)) {
          matched = true;
        }
      }

      if (matched) {
        results.push(`${prefix}:${iconName}`);
      }
    }
  }

  return results;
}

/**
 * 获取常用图标列表
 * @param collections 要包含的图标集前缀（如 ['mdi', 'ant-design', 'carbon']）
 * @param limit 每个图标集返回的图标数量
 */
export async function getCommonIcons(collections: string[] = ['mdi', 'ant-design', 'carbon'], limit = 20): Promise<string[]> {
  const iconList: string[] = [];

  for (const prefix of collections) {
    const collection = iconCollections[prefix];
    if (!collection || typeof collection !== 'object') {
      continue;
    }

    const icons = collection.icons || collection;
    if (!icons || typeof icons !== 'object') {
      continue;
    }

    // 获取前 N 个图标
    const iconNames = Object.keys(icons).slice(0, limit);
    iconNames.forEach((name) => {
      iconList.push(`${prefix}:${name}`);
    });
  }

  return iconList;
}

/**
 * 检查图标是否存在
 */
export async function iconExists(iconName: string): Promise<boolean> {
  if (!iconName || !iconName.includes(':')) {
    return false;
  }

  const parts = iconName.split(':');
  const prefix = parts[0];
  const name = parts[1];

  if (!prefix || !name) {
    return false;
  }

  const collection = iconCollections[prefix];

  if (!collection || typeof collection !== 'object') {
    return false;
  }

  const icons = collection.icons || collection;
  return !!icons && typeof icons === 'object' && Object.prototype.hasOwnProperty.call(icons, name);
}
