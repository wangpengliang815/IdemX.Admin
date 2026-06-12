import type { VbenFormProps } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRecordLoginResp } from '#/api/system/record';
import type { EnumTagOption } from '#/utils';

export function useGridFormSchema(onSearch?: () => void): VbenFormProps['schema'] {
  return [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        placeholder: '请输入用户名',
        onPressEnter: onSearch,
      },
      fieldName: 'userName',
      label: '用户名',
    },
    {
      component: 'RangePicker',
      componentProps: {
        allowClear: true,
        placeholder: ['开始时间', '结束时间'],
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'timeRange',
      label: '登录时间',
    },
  ];
}

export function useTableColumns(operTypeOptions: EnumTagOption[]): VxeTableGridOptions<SysRecordLoginResp>['columns'] {
  return [
    { align: 'left', field: 'userName', fixed: 'left', title: '用户名', minWidth: 60 },
    { align: 'left', field: 'os', title: '操作系统', width: 'auto' },
    { align: 'left', field: 'browser', title: '浏览器类型', width: 'auto' },
    {
      align: 'center',
      field: 'operType',
      title: '操作类型',
      width: 120,
      cellRender: { name: 'CellTag', options: operTypeOptions },
    },
    { align: 'left', field: 'comments', title: '备注信息', minWidth: 100 },
    { align: 'center', field: 'loginSource', title: '登录来源', minWidth: 120 },
    { field: 'createTime', title: '创建时间', width: 180 },
  ];
}
