import type { VbenFormProps } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysOrgResp, SysOrgTreeSelectNode } from '#/api/system/org';

import { loadOrgTreeSelectChild } from '#/api/system/org';

type OnActionClick = (params: { code: string; row: SysOrgResp }) => void;

export function useFormSchema(orgTreeData: SysOrgTreeSelectNode[]): VbenFormProps['schema'] {
  return [
    {
      component: 'TreeSelect',
      componentProps: {
        allowClear: true,
        loadData: loadOrgTreeSelectChild,
        placeholder: '留空为根节点',
        treeData: orgTreeData,
        fieldNames: { label: 'name', value: 'id', children: 'children' },
        dropdownStyle: { maxHeight: '400px', overflow: 'auto' },
      },
      fieldName: 'parentId',
      label: '上级机构',
    },
    {
      component: 'Input',
      componentProps: {
        placeholder: '请输入机构名称',
      },
      fieldName: 'name',
      label: '机构名称',
      rules: 'required',
    },
    {
      component: 'InputNumber',
      componentProps: {
        min: 0,
        placeholder: '请输入排序',
        style: { width: '100%' },
      },
      fieldName: 'sort',
      label: '排序',
      rules: 'required',
    },
  ];
}

export function useTableColumns(onActionClick: OnActionClick): VxeTableGridOptions<SysOrgResp>['columns'] {
  return [
    {
      align: 'left',
      field: 'name',
      fixed: 'left',
      title: '组织名称',
      treeNode: true,
    },
    {
      align: 'center',
      field: 'sort',
      title: '排序',
    },
    {
      field: 'createTime',
      title: '创建时间',
    },
    {
      field: 'updateTime',
      title: '更新时间',
    },
    {
      align: 'left',
      cellRender: {
        attrs: {
          nameField: 'name',
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'edit',
            text: '编辑',
            color: 'blue',
          },
          {
            code: 'delete',
            text: '删除',
            color: 'red',
            popconfirm: {
              title: '删除组织机构（物理删除）',
              description: '当前节点和所有子节点都会被删除，是否继续？',
              okText: '确定',
              cancelText: '取消',
            },
          },
        ],
      },
      field: 'operation',
      title: '操作',
    },
  ];
}
