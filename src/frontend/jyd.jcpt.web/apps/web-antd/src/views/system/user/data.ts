import type { VbenFormProps } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SysRoleResp } from '#/api/system/role';
import type { SysUserResp } from '#/api/system/user';
import type { EnumSelectOption, EnumTagOption } from '#/utils';

import { z } from '#/adapter/form';

type OnActionClick = (params: { code: string; row: SysUserResp }) => void;

const passwordZodCreate = z
  .string()
  .min(1, { message: '密码不能为空' })
  .min(8, { message: '密码至少需要8位字符' })
  .max(50, { message: '密码最多 50 个字符' })
  .regex(/(?=.*[a-z])(?=.*\d)/i, { message: '密码必须包含字母和数字' });

const passwordZodEdit = z.string().superRefine((val, ctx) => {
  const v = val.trim();
  if (v.length > 50) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: '密码最多 50 个字符' });
    return;
  }
  if (!v) return;
  if (v.length < 8) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: '密码至少需要8位字符' });
  }
  if (!/(?=.*[a-z])(?=.*\d)/i.test(v)) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: '密码必须包含字母和数字' });
  }
});

export function useFormSchema(roleOptions: SysRoleResp[], sexOptions: EnumSelectOption[], isEdit: boolean): VbenFormProps['schema'] {
  const sexSelectOptions: EnumSelectOption[] = [{ label: '请选择性别', value: 0 }, ...sexOptions];

  return [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        disabled: isEdit,
        placeholder: '请输入用户名（5-20位英文、数字、下划线）',
        maxlength: 20,
      },
      fieldName: 'userName',
      label: '用户名',
      rules: z
        .string()
        .min(5, { message: '用户名长度5-20位' })
        .max(20, { message: '用户名长度5-20位' })
        .regex(/^\w+$/, { message: '用户名只能包含英文、数字和下划线' }),
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        disabled: isEdit,
        placeholder: '请输入真实姓名（最少2个中文字符）',
      },
      fieldName: 'realName',
      label: '真实姓名',
      rules: z
        .string()
        .min(2, { message: '真实姓名至少需要2个字符' })
        .regex(/^\p{Script=Han}+$/u, { message: '真实姓名只能包含中文' }),
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        disabled: isEdit,
        placeholder: '请输入手机号',
        maxlength: 11,
      },
      fieldName: 'phone',
      label: '手机号',
      rules: z
        .string()
        .min(1, { message: '请输入手机号' })
        .regex(/^1[3-9]\d{9}$/, { message: '请输入有效的手机号码' }),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: sexSelectOptions,
        placeholder: '请选择性别',
        style: { width: '100%' },
      },
      fieldName: 'sex',
      label: '性别',
      rules: z.number().refine((v) => v >= 1, { message: '请选择性别' }),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        placeholder: '请选择角色',
        options: roleOptions,
        fieldNames: { label: 'roleName', value: 'id' },
        showSearch: true,
        filterOption: (input: string, option: SysRoleResp) => option.roleName.toLowerCase().includes(input.toLowerCase()),
        style: { width: '100%' },
      },
      fieldName: 'roleId',
      label: '分配角色',
      rules: z.string({ required_error: '请选择角色' }).min(1, { message: '请选择角色' }),
    },
    {
      component: 'InputPassword',
      componentProps: {
        allowClear: true,
        autocomplete: 'new-password',
        placeholder: isEdit ? '密码不修改请留空' : '请输入密码，最少8位且含字母和数字',
      },
      fieldName: 'password',
      label: '密码',
      rules: isEdit ? passwordZodEdit : passwordZodCreate,
    },
  ];
}

export function useGridFormSchema(onSearch?: () => void, stateOptions: EnumSelectOption[] = [], roleOptions: SysRoleResp[] = []): VbenFormProps['schema'] {
  return [
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        placeholder: '请选择角色',
        options: roleOptions,
        fieldNames: { label: 'roleName', value: 'id' },
        showSearch: true,
        filterOption: (input: string, option: SysRoleResp) => option.roleName.toLowerCase().includes(input.toLowerCase()),
      },
      fieldName: 'roleId',
      label: '角色',
    },
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
      component: 'Input',
      componentProps: {
        allowClear: true,
        placeholder: '请输入真实姓名',
        onPressEnter: onSearch,
      },
      fieldName: 'realName',
      label: '真实姓名',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        placeholder: '账号状态',
        options: stateOptions,
      },
      fieldName: 'status',
      label: '状态',
    },
  ];
}

export function useTableColumns(
  onActionClick: OnActionClick,
  sexTagOptions: EnumTagOption[],
  userTypeTagOptions: EnumTagOption[],
): VxeTableGridOptions<SysUserResp>['columns'] {
  return [
    { align: 'left', field: 'userName', title: '用户名', width: 'auto' },
    {
      align: 'center',
      field: 'roles',
      title: '角色',
      width: 'auto',
      slots: { default: 'roles' },
    },
    { align: 'center', field: 'realName', title: '真实姓名', width: 'auto' },
    {
      align: 'center',
      field: 'avatar',
      title: '头像',
      width: 'auto',
      cellRender: { name: 'CellImage' },
    },
    { align: 'left', field: 'nickName', title: '昵称', width: 'auto' },
    {
      align: 'center',
      field: 'sex',
      title: '性别',
      width: 'auto',
      cellRender: { name: 'CellTag', options: sexTagOptions },
    },
    { align: 'left', field: 'phone', title: '联系电话', width: 'auto' },
    {
      align: 'left',
      field: 'wechatNo',
      width: 'auto',
      title: '微信号',
      slots: { default: 'wechatNo' },
    },
    {
      align: 'center',
      field: 'userType',
      title: '用户类型',
      width: 'auto',
      cellRender: { name: 'CellTag', options: userTypeTagOptions },
    },
    {
      align: 'center',
      field: 'status',
      title: '状态',
      width: 'auto',
      slots: { default: 'state' },
    },
    { align: 'left', field: 'email', title: '邮箱', width: 'auto' },

    { field: 'createTime', title: '创建时间', width: 'auto' },
    {
      align: 'left',
      field: 'operation',
      fixed: 'right',
      title: '操作',
      width: 'auto',
      cellRender: {
        name: 'CellOperation',
        attrs: { nameField: 'userName', onClick: onActionClick },
        options: [
          { code: 'edit', text: '编辑', color: 'blue' },
          {
            code: 'delete',
            text: '删除',
            color: 'red',
            popconfirm: {
              title: '删除用户（物理删除）',
              description: '请确认是否删除当前用户？建议停用而不是物理删除',
              okText: '确定',
              cancelText: '取消',
            },
          },
        ],
      },
    },
  ];
}
