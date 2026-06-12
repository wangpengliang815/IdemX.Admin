import type { VbenFormProps } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRoleResp } from '#/api/system/role';

import { z } from '#/adapter/form';

type OnActionClick = (params: { code: string; row: SysRoleResp }) => void;

export function useFormSchema(): VbenFormProps['schema'] {
  return [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        maxlength: 50,
        placeholder: '请输入角色名称',
        showCount: true,
      },
      fieldName: 'roleName',
      label: '角色名称',
      rules: z.string().min(2, { message: '角色名称 2-50 个字符' }).max(50, { message: '角色名称 2-50 个字符' }),
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        maxlength: 50,
        placeholder: '请输入角色编码',
        showCount: true,
      },
      fieldName: 'roleCode',
      label: '角色编码',
      rules: z.string().min(2, { message: '角色编码 2-50 个字符' }).max(50, { message: '角色编码 2-50 个字符' }),
    },
    {
      component: 'Textarea',
      componentProps: {
        allowClear: true,
        maxlength: 255,
        placeholder: '请输入备注',
        rows: 3,
        showCount: true,
      },
      fieldName: 'memo',
      label: '备注',
      rules: z.string().max(255, { message: '备注最多 255 个字符' }).optional(),
    },
  ];
}

export function useGridFormSchema(onSearch?: () => void): VbenFormProps['schema'] {
  return [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        placeholder: '请输入角色名称',
        onPressEnter: onSearch,
      },
      fieldName: 'roleName',
      label: '角色名称',
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        placeholder: '请输入角色编码',
        onPressEnter: onSearch,
      },
      fieldName: 'roleCode',
      label: '角色编码',
    },
  ];
}

export function useTableColumns(onActionClick: OnActionClick): VxeTableGridOptions<SysRoleResp>['columns'] {
  return [
    { field: 'roleName', title: '角色名称', align: 'left', minWidth: 120 },
    {
      field: 'roleCode',
      title: '角色编码',
      align: 'left',
      cellRender: { name: 'CellTag', attrs: { color: 'blue' } },
    },
    {
      field: 'memo',
      title: '备注',
      align: 'left',
      cellRender: { name: 'CellTag', attrs: { color: 'blue' } },
    },
    { field: 'createTime', title: '创建时间' },
    { field: 'updateTime', title: '更新时间' },
    {
      field: 'operation',
      title: '操作',
      width: 220,
      align: 'left',
      cellRender: {
        name: 'CellOperation',
        attrs: { nameField: 'roleName', onClick: onActionClick },
        options: [
          { code: 'edit', text: '编辑', color: 'blue' },
          {
            code: 'delete',
            text: '删除',
            color: 'red',
            popconfirm: {
              title: '删除角色（物理删除）',
              description: '当前角色及绑定权限会一起删除，是否继续？',
              okText: '确定',
              cancelText: '取消',
            },
          },
          { code: 'permission', text: '权限分配', color: 'blue' },
        ],
      },
    },
  ];
}
