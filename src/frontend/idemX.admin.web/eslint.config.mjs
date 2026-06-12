import { defineConfig } from '@vben/eslint-config';

export default defineConfig([
  {
    rules: {
      // 允许 == null / != null（同时判断 null 与 undefined 的惯用写法）
      eqeqeq: ['error', 'smart'],
      // 与 prettier-config 一致，保证格式化后模板单属性可保持一行、不强制换行
      'prettier/prettier': [
        'error',
        {
          singleAttributePerLine: false,
        },
      ],
    },
  },
]);
