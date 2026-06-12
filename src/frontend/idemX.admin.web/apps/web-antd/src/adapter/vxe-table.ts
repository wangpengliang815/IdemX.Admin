import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';

import { computed, defineComponent, h } from 'vue';

import { setupVbenVxeTable, useVbenVxeGrid } from '@vben/plugins/vxe-table';
import { get, isFunction, isString } from '@vben/utils';

import { Badge, Button, Image, Popconfirm, Tag, Tooltip } from 'ant-design-vue';

import { useAuthStore } from '#/store';

import { useVbenForm } from './form';

import './styles/popconfirm.css';

/** 与 layouts/basic.vue 右上角头像一致：http(s) 资源加 ?t=nonce，避免浏览器沿用旧缓存图 */
function appendHttpImageCacheQuery(src: string, t: number): string {
  if (!src || typeof src !== 'string' || src.startsWith('data:') || src.startsWith('blob:')) {
    return src;
  }
  if (!src.startsWith('http://') && !src.startsWith('https://')) {
    return src;
  }
  return src.includes('?') ? `${src}&t=${t}` : `${src}?t=${t}`;
}

/** 订阅 authStore.avatarNonce，头像 URL 不变时 getUserInfo 后也会重绘、换图 */
const CellImageCell = defineComponent({
  name: 'CellImageCell',
  props: {
    raw: { type: String, default: '' },
    field: { type: String, required: true },
    cacheBust: { type: Boolean, default: false },
    extraImageProps: { type: Object, default: () => ({}) },
  },
  setup(props) {
    const authStore = useAuthStore();
    const src = computed(() => {
      const raw = props.raw || '/default-avatar.png';
      const needBust = props.cacheBust || props.field === 'avatar';
      if (!needBust || typeof raw !== 'string') return raw;
      const nonceRaw: unknown = authStore.avatarNonce;
      const t = typeof nonceRaw === 'number' ? nonceRaw : (nonceRaw as { value?: number })?.value;
      if (!t) return raw;
      return appendHttpImageCacheQuery(raw, t);
    });
    return () =>
      h(Image, {
        src: src.value,
        width: 30,
        height: 30,
        preview: false,
        style: {
          borderRadius: '50%',
          objectFit: 'cover',
          display: 'block',
          margin: '0 auto',
        },
        ...props.extraImageProps,
      });
  },
});

setupVbenVxeTable({
  configVxeTable: (vxeUI) => {
    vxeUI.setConfig({
      grid: {
        align: 'center',
        border: false,
        columnConfig: {
          resizable: true,
          // 使列 width: 'auto' 按内容计算宽度（表头、表体、表尾均参与计算）
          autoOptions: {
            isCalcBody: true,
            isCalcFooter: true,
            isCalcHeader: true,
          },
        },
        minHeight: 180,
        formConfig: {
          // 全局禁用vxe-table的表单配置，使用formOptions
          enabled: false,
        },
        proxyConfig: {
          autoLoad: true,
          // 约定：ajax.query 返回 { data: 列表, total: 总数 }，与 result/total 字段对应，勿用 items
          response: {
            result: 'data',
            total: 'total',
            list: 'data',
          },
          showActiveMsg: false,
          showResponseMsg: false,
          showLoadMsg: false,
        },
        round: true,
        showOverflow: true,
        size: 'small',
      } as VxeTableGridOptions,
    });

    // 表格配置项可以用 cellRender: { name: 'CellImage' },
    vxeUI.renderer.add('CellImage', {
      renderTableDefault(renderOpts, params) {
        const { props } = renderOpts;
        const { column, row } = params;
        const raw = row[column.field] as string | undefined;
        const cacheBust = (props as undefined | { cacheBust?: boolean })?.cacheBust === true || column.field === 'avatar';
        return h(CellImageCell, {
          raw: raw ?? '',
          field: String(column.field),
          cacheBust,
          extraImageProps: props ?? {},
        });
      },
    });

    // 表格配置项可以用 cellRender: { name: 'CellLink' },
    vxeUI.renderer.add('CellLink', {
      renderTableDefault(renderOpts) {
        const { props } = renderOpts;
        return h(Button, { size: 'small', type: 'link' }, { default: () => props?.text });
      },
    });

    // 单元格渲染：Tag（column 可传 attrs 或 props，均会生效）
    vxeUI.renderer.add('CellTag', {
      renderTableDefault(renderOpts, { column, row }) {
        const { attrs, options, props } = renderOpts as {
          attrs?: Record<string, unknown>;
          options?: Array<{ color?: string; label: string; value: unknown }>;
          props?: Record<string, unknown>;
        };
        const tagProps = props ?? attrs ?? {};
        const value = get(row, column.field);
        // 如果没有值，直接返回空字符串，不显示任何内容
        if (!value && value !== 0 && value !== false) {
          return '';
        }
        const displayValue = value;
        // 如果没有options，直接使用 props/attrs 中的 color（用于简单的单颜色 Tag）
        if (!options) {
          return h(
            Tag,
            {
              color: (tagProps.color as string) || 'blue',
              ...tagProps,
            },
            { default: () => displayValue },
          );
        }

        const tagOptions = options;
        const tagItem = tagOptions.find((item: any) => {
          // 支持字符串和数字的值比较
          return item.value === value || String(item.value) === String(value);
        });
        const itemProps = tagItem ? { ...tagItem } : {};
        delete (itemProps as any).label;
        return h(
          Tag,
          {
            ...tagProps,
            ...itemProps,
          },
          { default: () => tagItem?.label ?? displayValue },
        );
      },
    });

    // 单元格渲染：Badge（Ant Design Vue Badge 状态用 status + text）
    vxeUI.renderer.add('CellBadge', {
      renderTableDefault(renderOpts, { column, row }) {
        const colRender = (renderOpts || (column as any)?.cellRender) as
          | undefined
          | {
              attrs?: Record<string, unknown>;
              options?: Array<{
                color?: string;
                label: string;
                value: unknown;
              }>;
              props?: Record<string, unknown>;
            };
        const { attrs, options, props } = colRender ?? {};
        const value = get(row, column.field);
        // 无值时仍显示，用原始值或空；Badge 的 status 为 'success' | 'default' | 'error' | 'processing' | 'warning'
        type BadgeStatus = 'default' | 'error' | 'processing' | 'success' | 'warning';
        let badgeStatus: BadgeStatus = (attrs?.status as BadgeStatus) ?? (props?.status as BadgeStatus) ?? 'default';
        let badgeText: number | string = value;

        if (options && Array.isArray(options)) {
          const badgeItem = options.find((item: any) => {
            return item.value === value || String(item.value) === String(value);
          });
          if (badgeItem) {
            const color = badgeItem.color || 'default';
            const validStatus: BadgeStatus[] = ['default', 'error', 'processing', 'success', 'warning'];
            badgeStatus = validStatus.includes(color as BadgeStatus) ? (color as BadgeStatus) : 'default';
            badgeText = badgeItem.label ?? String(value);
          }
        }

        return h(Badge, {
          status: badgeStatus,
          text: badgeText,
          ...props,
          ...attrs,
        });
      },
    });

    // 单元格渲染：Icon（性别等）
    vxeUI.renderer.add('CellIcon', {
      renderTableDefault({ options, props }, { column, row }) {
        const value = get(row, column.field);
        let iconConfig: any = null;

        if (Array.isArray(options)) {
          iconConfig = options.find((opt: any) => {
            // 支持字符串和数字的值比较
            return opt.value === value || String(opt.value) === String(value);
          });
        } else {
          const iconOptions = (options as unknown as Record<number | string, any>) || {};
          iconConfig = iconOptions[value];
        }

        if (!iconConfig) {
          return value;
        }
        const IconComponent = iconConfig.icon;
        return h(
          'div',
          {
            style: {
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '16px',
            },
            title: iconConfig.label,
          },
          h(IconComponent, {
            style: { color: iconConfig.color || '#1890ff' },
            ...props,
          }),
        );
      },
    });

    // 注册表格的操作按钮渲染器
    vxeUI.renderer.add('CellOperation', {
      renderTableDefault({ attrs, options, props }, { column, row }) {
        const defaultProps = { size: 'small', type: 'link', ...props };
        let align = 'end';
        switch (column.align) {
          case 'center': {
            align = 'center';
            break;
          }
          case 'left': {
            align = 'start';
            break;
          }
          default: {
            align = 'end';
            break;
          }
        }
        const presets: any = {
          delete: {
            danger: true,
            text: '删除',
          },
          edit: {
            text: '编辑',
          },
          append: {
            text: '新增下级',
          },
        };
        const operations: any[] = (options || ['edit', 'delete'])
          .map((opt: any) => {
            if (isString(opt)) {
              return presets[opt]
                ? { code: opt, ...presets[opt], ...defaultProps }
                : {
                    code: opt,
                    text: opt,
                    ...defaultProps,
                  };
            } else {
              return { ...defaultProps, ...presets[opt.code], ...opt };
            }
          })
          .map((opt: any) => {
            const optBtn: any = {};
            Object.keys(opt).forEach((key) => {
              optBtn[key] = isFunction(opt[key]) ? opt[key](row) : opt[key];
            });
            return optBtn;
          })
          .filter((opt: any) => opt.show !== false);

        const wrapTooltip = (node: any, title?: string) => {
          if (!title) return node;
          return h(Tooltip, { title }, { default: () => node });
        };

        const renderBtn = (opt: any, listen = true) => {
          const isDisabled = Boolean(opt.disabled);

          // 默认所有操作按钮都使用Tag样式，通过color属性控制颜色
          // 如果明确设置了type属性且不是'link'，则使用Button组件
          const shouldUseButton = opt.type && opt.type !== 'link';
          let content: any;

          if (shouldUseButton) {
            // 使用Button组件（当明确设置了type且不是'link'时）
            let onClick: (() => void) | undefined;
            if (listen && !isDisabled) {
              onClick = () => {
                attrs?.onClick?.({
                  code: opt.code,
                  row,
                });
              };
            }
            content = h(
              Button,
              {
                ...opt,
                disabled: isDisabled || opt.disabled,
                onClick,
              },
              { default: () => opt.text },
            );
          } else {
            const tagElement = h(Tag, { bordered: opt.bordered, color: opt.color || 'blue' }, { default: () => opt.text });

            // 如果设置了popconfirm或confirm，使用Popconfirm包裹
            if (opt.popconfirm || opt.confirm) {
              const confirmConfig = opt.popconfirm || opt.confirm || {};
              content = isDisabled
                ? h(
                    'div',
                    {
                      style: { cursor: 'not-allowed', opacity: 0.45 },
                    },
                    tagElement,
                  )
                : h(
                    'div',
                    {
                      style: { cursor: 'pointer' },
                    },
                    h(
                      Popconfirm,
                      {
                        title: confirmConfig.title || '确认删除？',
                        description: confirmConfig.description || '你确定要删除此条记录吗？',
                        okText: confirmConfig.okText || '确定',
                        cancelText: confirmConfig.cancelText || '取消',
                        okType: 'danger',
                        placement: 'bottomRight',
                        okButtonProps: {
                          style: { marginLeft: '8px' },
                        },
                        cancelButtonProps: {
                          style: { marginRight: '8px' },
                        },
                        onConfirm: () => {
                          attrs?.onClick?.({
                            code: opt.code,
                            row,
                          });
                        },
                      },
                      { default: () => tagElement },
                    ),
                  );
            } else {
              // 普通操作按钮直接响应点击
              content = h(
                'div',
                {
                  style: isDisabled ? { cursor: 'not-allowed', opacity: 0.45 } : { cursor: 'pointer' },
                  onClick: isDisabled
                    ? undefined
                    : () => {
                        attrs?.onClick?.({
                          code: opt.code,
                          row,
                        });
                      },
                },
                tagElement,
              );
            }
          }

          return wrapTooltip(content, opt.tooltip);
        };

        const btns = operations.map((opt: any) => renderBtn(opt));
        return h(
          'div',
          {
            class: 'flex table-operations',
            style: { justifyContent: align },
          },
          btns,
        );
      },
    });

    // 这里可以自行扩展 vxe-table 的全局配置，比如自定义格式化
    // vxeUI.formats.add
  },
  useVbenForm,
});

export { useVbenVxeGrid };

export type * from '@vben/plugins/vxe-table';
