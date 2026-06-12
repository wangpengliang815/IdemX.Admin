import { addCollection } from '@vben/icons';

/**
 * 启动时注册离线图标集；未命中集合的图标将不显示，但不会发起外网请求。
 */
export async function setupIconifyOffline(): Promise<void> {
  const loaders = [
    import('@iconify/json/json/mdi.json'),
    import('@iconify/json/json/ant-design.json'),
    import('@iconify/json/json/carbon.json'),
    import('@iconify/json/json/fluent-mdl2.json'),
  ] as const;

  const results = await Promise.allSettled(
    loaders.map(async (load) => {
      const data = await load;
      addCollection(data.default ?? data);
    }),
  );

  const failed = results.filter((r) => r.status === 'rejected');
  if (failed.length > 0)
    console.warn(
      '[iconify] 部分图标集加载失败，相关图标可能无法显示:',
      failed.map((r) => (r as PromiseRejectedResult).reason),
    );
}
