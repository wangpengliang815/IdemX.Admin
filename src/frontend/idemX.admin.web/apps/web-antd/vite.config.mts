import { defineConfig } from '@vben/vite-config';

export default defineConfig(async (env) => {
  // 判断是否为生产环境
  const isProd = env?.mode === 'production' || process.env.NODE_ENV === 'production';

  return {
    application: {
      // 生产环境和开发环境都使用 /api 作为前缀
      // 开发环境：请求 /api/... -> Vite Proxy -> http://localhost:5000/api/...
      // 生产环境：请求 /api/... -> Nginx -> 后端
      apiURL: '/api',
    },
    vite: {
      server: {
        proxy: {
          '/api': {
            changeOrigin: true,
            // 开发环境代理到后端 localhost:5000
            target: 'http://localhost:5000',
            ws: true,
            // 注意：后端路由本身包含 /api 前缀，所以不需要 rewrite 去掉 /api
          },
        },
      },
    },
  };
});
