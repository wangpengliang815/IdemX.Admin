import type { VbenFormProps } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRecordNlogResp } from '#/api/system/record';

const logLevelOptions = [
  { label: 'Trace', value: 'Trace', color: 'default' },
  { label: 'Debug', value: 'Debug', color: 'blue' },
  { label: 'Info', value: 'Info', color: 'green' },
  { label: 'Warn', value: 'Warn', color: 'orange' },
  { label: 'Error', value: 'Error', color: 'red' },
  { label: 'Fatal', value: 'Fatal', color: 'red' },
];

export function useGridFormSchema(): VbenFormProps['schema'] {
  return [
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        placeholder: '请选择日志级别',
        options: logLevelOptions,
      },
      fieldName: 'logLevel',
      label: '日志级别',
    },
    {
      component: 'RangePicker',
      componentProps: {
        allowClear: true,
        placeholder: ['开始日期', '结束日期'],
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'timeRange',
      label: '时间范围',
    },
  ];
}

export function useTableColumns(): VxeTableGridOptions<SysRecordNlogResp>['columns'] {
  return [
    {
      align: 'center',
      field: 'logLevel',
      fixed: 'left',
      title: '日志级别',
      width: 'auto',
      cellRender: { name: 'CellTag', options: logLevelOptions },
    },
    { field: 'logDate', title: '日志时间', width: 'auto' },
    { align: 'center', field: 'logType', title: '事件日志上下文', width: 'auto' },
    { align: 'center', field: 'logTitle', title: '事件标题', minWidth: 150 },
    { align: 'center', field: 'logger', title: '记录器名字', minWidth: 100 },
    { align: 'left', field: 'message', title: '消息内容', minWidth: 250, showOverflow: 'tooltip' },
    { align: 'left', field: 'exception', title: '异常信息', minWidth: 250, showOverflow: 'tooltip' },
    { align: 'center', field: 'machineName', title: '机器名称', minWidth: 150 },
    { align: 'center', field: 'machineIp', title: '机器IP', minWidth: 120 },
    { align: 'center', field: 'netRequestMethod', title: '请求方式', minWidth: 100 },
    { align: 'left', field: 'netRequestUrl', title: '请求地址', minWidth: 200, showOverflow: 'tooltip' },
    { align: 'center', field: 'netUserIsauthenticated', title: '是否授权', minWidth: 100 },
    { align: 'center', field: 'netUserAuthtype', title: '授权类型', minWidth: 200 },
    { align: 'center', field: 'netUserIdentity', title: '身份认证', minWidth: 120 },
  ];
}
