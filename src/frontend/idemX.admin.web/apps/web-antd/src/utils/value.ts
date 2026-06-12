/**
 * 判断是否有值：非 null、非 undefined、非空字符串（用于 v-if 等展示判断）
 * 作为类型守卫，便于在条件后收窄类型
 */
export function hasValue<T>(v: T): v is Exclude<NonNullable<T>, ''> {
  return v != null && v !== '';
}
