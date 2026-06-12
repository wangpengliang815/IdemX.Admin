import dayjs from 'dayjs';

/**
 * 数字展示：zh-CN 千分位；默认保留两位小数，空值返回空字符串。
 */
export function formatNumber(v: unknown, fractionDigits = 2): string {
  if (v == null) return '';
  return Number(v).toLocaleString('zh-CN', {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits,
  });
}

/**
 * 价格展示：基于 formatNumber 输出，自动拼接人民币符号。
 */
export function formatPrice(v: unknown, fractionDigits = 2): string {
  const n = formatNumber(v, fractionDigits);
  return n ? `¥${n}` : '';
}

/**
 * 日期格式化，空值返回空字符串
 * @param v 日期值（字符串、数字或 Date）
 * @param format 默认 'YYYY-MM-DD'
 */
export function formatDate(v: unknown, format = 'YYYY-MM-DD'): string {
  if (v == null) return '';
  const d = dayjs(v as Date | number | string);
  return d.isValid() ? d.format(format) : '';
}
