import type { VbenFormProps } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysMenuResp, SysMenuTreeSelectNode } from '#/api/system/menu';

import { loadMenuTreeSelectChild } from '#/api/system/menu';

type OnActionClick = (params: { code: string; row: SysMenuResp }) => void;

export function useFormSchema(menuTreeData: SysMenuTreeSelectNode[], componentOptions: Array<{ value: string }>): VbenFormProps['schema'] {
  return [
    {
      component: 'TreeSelect',
      componentProps: {
        allowClear: true,
        loadData: loadMenuTreeSelectChild,
        placeholder: '留空为根节点',
        treeData: menuTreeData,
        fieldNames: { label: 'title', value: 'id', children: 'children' },
        dropdownStyle: { maxHeight: '400px', overflow: 'auto' },
      },
      fieldName: 'parentId',
      label: '父级菜单',
    },
    {
      component: 'Input',
      componentProps: { allowClear: true, placeholder: '请输入英文编码，用于路由，必须唯一' },
      fieldName: 'name',
      label: '英文编码',
      rules: 'required',
    },
    {
      component: 'Input',
      componentProps: { allowClear: true, placeholder: '请输入菜单标题' },
      fieldName: 'title',
      label: '标题',
      rules: 'required',
    },
    {
      component: 'Input',
      componentProps: { allowClear: true, placeholder: '如：/system/menu' },
      fieldName: 'path',
      label: '路由路径',
      rules: 'required',
    },
    {
      component: 'IconPicker',
      componentProps: { class: 'w-full', prefix: 'ant-design', type: 'input' },
      fieldName: 'icon',
      label: '菜单图标',
    },
    {
      component: 'AutoComplete',
      componentProps: {
        allowClear: true,
        options: componentOptions,
        placeholder: '请输入或选择组件路径',
        style: { width: '100%' },
        filterOption: (input: string, option: { value: string }) => option.value.toLowerCase().includes(input.toLowerCase()),
      },
      fieldName: 'component',
      label: '组件路径',
    },
    {
      component: 'Input',
      componentProps: { allowClear: true, placeholder: '权限标识' },
      fieldName: 'authority',
      label: '权限标识',
    },
    {
      component: 'Input',
      componentProps: { allowClear: true, placeholder: '例如：NEW、HOT、99+' },
      fieldName: 'badge',
      label: '徽标内容',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        placeholder: '请选择徽标类型',
        options: [
          { label: '无', value: '' },
          { label: '点状', value: 'dot' },
          { label: '普通', value: 'normal' },
        ],
      },
      fieldName: 'badgeType',
      label: '徽标类型',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        placeholder: '请选择徽标样式',
        options: [
          { label: '默认', value: 'default' },
          { label: '危险', value: 'destructive' },
          { label: '主要', value: 'primary' },
          { label: '成功', value: 'success' },
          { label: '信息', value: 'info' },
          { label: '警告', value: 'warning' },
        ],
      },
      fieldName: 'badgeVariants',
      label: '徽标样式',
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, placeholder: '数字越大越靠后', style: { width: '100%' } },
      fieldName: 'sort',
      label: '排序',
      rules: 'required',
    },
    {
      component: 'RadioGroup',
      componentProps: {
        optionType: 'button',
        buttonStyle: 'solid',
        options: [
          { label: '启用', value: 1 },
          { label: '禁用', value: 0 },
        ],
      },
      fieldName: 'status',
      label: '状态',
      rules: 'required',
    },
    {
      component: 'Checkbox',
      fieldName: 'keepAlive',
      label: '缓存组件',
    },
    {
      component: 'Checkbox',
      fieldName: 'affixTab',
      label: '固定标签页',
    },
    {
      component: 'Checkbox',
      fieldName: 'isExternal',
      label: '外部链接',
    },
  ];
}

export function useTableColumns(onActionClick: OnActionClick): VxeTableGridOptions<SysMenuResp>['columns'] {
  return [
    {
      align: 'left',
      field: 'title',
      fixed: 'left',
      title: '菜单标题',
      width: 200,
      treeNode: true,
      slots: { default: 'title' },
    },
    {
      align: 'center',
      field: 'menuType',
      title: '类型',
      cellRender: {
        name: 'CellTag',
        options: [
          { label: '菜单', value: 0, color: 'blue' },
          { label: '按钮', value: 1, color: 'orange' },
        ],
      },
    },
    {
      field: 'name',
      title: '英文编码',
      align: 'left',
      width: 'auto',
      cellRender: { name: 'CellTag', attrs: { color: 'green' } },
    },
    {
      field: 'authority',
      title: '权限标识',
      align: 'left',
      width: 'auto',
    },
    {
      align: 'left',
      field: 'path',
      title: '路由路径',
      width: 'auto',
      cellRender: { name: 'CellTag', attrs: { color: 'geekblue' } },
    },
    {
      field: 'component',
      title: '组件路径',
      minWidth: '150',
      align: 'left',
      cellRender: { name: 'CellTag', attrs: { color: 'purple' } },
    },
    {
      field: 'badge',
      title: '徽标内容',
      align: 'center',
      width: 'auto',
      cellRender: { name: 'CellTag', attrs: { color: 'geekblue' } },
    },
    {
      field: 'badgeType',
      title: '徽标类型',
      align: 'center',
      width: 'auto',
    },
    {
      field: 'badgeVariants',
      title: '徽标样式',
      align: 'center',
      width: 'auto',
    },
    {
      field: 'sort',
      title: '排序',
      align: 'center',
    },
    {
      field: 'status',
      title: '状态',
      align: 'center',
      cellRender: {
        name: 'CellBadge',
        options: [
          { label: '启用', value: 1, color: 'success' },
          { label: '禁用', value: 0, color: 'error' },
        ],
      },
    },
    {
      align: 'left',
      cellRender: {
        attrs: {
          nameField: 'title',
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'edit',
            text: '编辑',
            color: 'blue',
            show: (row: SysMenuResp) => row.menuType === 0,
          },
          {
            code: 'delete',
            text: '删除',
            color: 'red',
            show: (row: SysMenuResp) => row.menuType === 0,
            popconfirm: {
              title: '删除菜单（物理删除）',
              description: '当前菜单和所有子菜单都会被删除，是否继续？',
              okText: '确定',
              cancelText: '取消',
            },
          },
          {
            code: 'import',
            text: '导入按钮',
            color: 'blue',
            show: (row: SysMenuResp) => row.menuType === 0 && !row.hasChild,
          },
        ],
      },
      field: 'operation',
      width: 'auto',
      title: '操作',
    },
  ];
}
