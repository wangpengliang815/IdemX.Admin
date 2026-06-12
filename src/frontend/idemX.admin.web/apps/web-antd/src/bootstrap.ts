import { createApp, watchEffect } from 'vue';

import { registerAccessDirective } from '@vben/access';
import { registerLoadingDirective } from '@vben/common-ui/es/loading';
import { preferences } from '@vben/preferences';
import { initStores } from '@vben/stores';
import '@vben/styles';
import '@vben/styles/antd';

import { useTitle } from '@vueuse/core';

import { message } from 'ant-design-vue';

import { $t, setupI18n } from '#/locales';
import { logger } from '#/utils/logger';

import { initComponentAdapter } from './adapter/component';
import { initSetupVbenForm } from './adapter/form';
import App from './app.vue';
import { router } from './router';
import { setupIconifyOffline } from './utils/setup-iconify-offline';

async function bootstrap(namespace: string) {
  await setupIconifyOffline();
  try {
    // 初始化组件适配器
    await initComponentAdapter();

    // 初始化表单组件
    await initSetupVbenForm();
  } catch (error) {
    logger.error('初始化组件失败:', error);
    throw error;
  }

  // // 设置弹窗的默认配置
  // setDefaultModalProps({
  //   fullscreenButton: false,
  // });
  // // 设置抽屉的默认配置
  // setDefaultDrawerProps({
  //   zIndex: 1020,
  // });

  const app = createApp(App);

  message.config({ duration: 6 });

  // 全局异常兜底：统一记录（请求层提示已由 requestClient 拦截器处理）
  app.config.errorHandler = (err, instance, info) => {
    logger.error('vue errorHandler:', { err, info, instance });
  };

  // 统一处理未捕获的 Promise 异常（常见于事件回调 async 未 await）
  window.addEventListener('unhandledrejection', (event) => {
    const reason = event.reason;
    // Ant Design Vue 表单校验失败：含 errorFields 视为已由表单展示，不再打日志、不展示控制台红字
    if (reason && typeof reason === 'object' && 'errorFields' in reason) {
      event.preventDefault();
      return;
    }
    logger.error('unhandledrejection:', reason);
    if (import.meta.env.PROD) event.preventDefault();
  });

  // 兜底捕获运行时错误（例如脚本错误）
  window.addEventListener('error', (event) => {
    const msg = (event.error?.message ?? event.message)?.toString?.() ?? '';
    // 忽略 ResizeObserver 循环告警（表格/布局组件引起的良性提示，不影响功能）
    if (msg.includes('ResizeObserver loop')) return;
    logger.error('window error:', event.error ?? event.message);
  });

  // 注册v-loading指令
  registerLoadingDirective(app, {
    loading: 'loading', // 在这里可以自定义指令名称，也可以明确提供false表示不注册这个指令
    spinning: 'spinning',
  });

  // 国际化 i18n 配置
  await setupI18n(app);

  try {
    await initStores(app, { namespace });
  } catch (error) {
    logger.error('初始化 Store 失败:', error);
    throw error;
  }

  // 枚举在登录后由路由守卫 ensureEnumsLoaded 或各业务页 onMounted 加载，不在启动时请求

  // 安装权限指令
  registerAccessDirective(app);

  // 初始化 tippy
  const { initTippy } = await import('@vben/common-ui/es/tippy');
  initTippy(app);

  // 配置路由及路由守卫
  try {
    app.use(router);
  } catch (error) {
    logger.error('路由初始化失败:', error);
    throw error;
  }

  // 配置Motion插件
  const { MotionPlugin } = await import('@vben/plugins/motion');
  app.use(MotionPlugin);

  // 动态更新标题
  watchEffect(() => {
    if (preferences.app.dynamicTitle) {
      const routeTitle = router.currentRoute.value.meta?.title;
      const pageTitle = (routeTitle ? `${$t(routeTitle)} - ` : '') + preferences.app.name;
      useTitle(pageTitle);
    }
  });

  app.mount('#app');
}

export { bootstrap };
