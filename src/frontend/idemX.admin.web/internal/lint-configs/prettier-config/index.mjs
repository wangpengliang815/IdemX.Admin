export default {
  endOfLine: 'auto',
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
