/** 本 app 独立配置：避免扩展未解析到包时仍保持一行、不强制折行 */
export default {
  endOfLine: 'auto',
  printWidth: 160,
  overrides: [
    {
      files: ['*.json5'],
      options: {
        quoteProps: 'preserve',
        singleQuote: false,
      },
    },
    {
      files: ['*.vue'],
      options: {
        singleAttributePerLine: false,
      },
    },
  ],
  plugins: ['prettier-plugin-vue', 'prettier-plugin-tailwindcss'],
  singleAttributePerLine: false,
  proseWrap: 'never',
  semi: true,
  singleQuote: true,
  trailingComma: 'all',
};
