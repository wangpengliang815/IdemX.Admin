namespace Core.Infrastructure.Configuration;

/// <summary>
/// 全局业务常量类
/// </summary>
public class GlobalConstVars
{
    /// <summary>
    /// 账号密码登录
    /// </summary>
    public const string AuthEmptyFailure = "用户名或密码不可为空";
    public const string AuthFreezeFailure = "您的账户已经被停用，请联系管理员解锁";
    public const string AuthSuccess = "登录成功";
    public const string AuthLogoutSuccess = "退出登录";

    /// <summary>
    /// 手机号登录
    /// </summary>
    public const string PhoneSmsInvalid = "验证码错误或已过期";
    public const string PhoneNotRegistered = "该手机号未注册";
    public const string PhoneNotBound = "当前账号未绑定手机号";
    public const string PhoneVerifySuccess = "当前手机号验证成功";
    public const string SameAsCurrentPhone = "新手机号不能与当前手机号一致";

    /// <summary>
    /// 个人注册相关
    /// </summary>
    public const string RegUserNameExists = "用户名已被注册";
    public const string RegPhoneExists = "手机号已注册";
    public const string RegSmsInvalid = "短信验证码错误或已过期";
    public const string RegFailed = "注册失败，请稍后重试";
    public const string RegSuccess = "注册成功";

    /// <summary>
    /// 密码相关
    /// </summary>
    public const string PasswordEditSuccess = "密码修改成功，请重新登录";
    public const string PasswordEditFailure = "密码修改失败";
    public const string SameAsOldPassword = "新密码不能与当前密码相同";
    public const string OldPasswordError = "旧密码输入错误";
    public const string ResetPasswordSuccess = "密码已重置，请使用新密码登录";
    public const string ResetPasswordFailure = "密码重置失败，请稍后重试";

    /// <summary>
    /// 头像上传
    /// </summary>
    public const string UploadAvatarSuccess = "头像更新成功";
    public const string UploadAvatarFailure = "头像更新失败";

    /// <summary>
    /// 数据删除
    /// </summary>
    public const string DeleteSuccess = "数据删除成功";
    public const string DeleteFailure = "数据删除失败";
    public const string DeleteProhibitDelete = "系统禁止删除此数据";
    public const string DeleteHasDependentRecords = "存在关联数据，禁止删除";

    /// <summary>
    /// 数据创建
    /// </summary>
    public const string CreateSuccess = "数据添加成功";
    public const string CreateFailure = "数据添加失败";

    /// <summary>
    /// 数据编辑
    /// </summary>
    public const string EditSuccess = "数据修改成功";
    public const string EditFailure = "数据修改失败";
    public const string EditProhibitEdit = "系统禁止修改此数据";

    /// <summary>
    /// 组织机构
    /// </summary>
    public const string OrgSelfParentFailure = "上级机构不能为当前机构本身";
    public const string OrgChildParentFailure = "上级机构不能为当前机构的下级机构";
    public const string OrgInvalidParentFailure = "上级机构不存在";

    /// <summary>
    /// 系统菜单（树）
    /// </summary>
    public const string MenuSelfParentFailure = "上级菜单不能为当前菜单本身";
    public const string MenuChildParentFailure = "上级菜单不能为当前菜单的下级菜单";
    public const string MenuInvalidParentFailure = "上级菜单不存在";

    /// <summary>
    /// 系统角色
    /// </summary>
    public const string RoleNameDuplicateFailure = "角色名称已存在";
    public const string RoleCodeDuplicateFailure = "角色标识已存在";
    public const string RoleMenuCatalogEmptyFailure = "系统中暂无菜单数据，无法配置角色权限";
    public const string RoleMenuInvalidMenuFailure = "存在无效的菜单项";

    /// <summary>
    /// 数据查询与消息
    /// </summary>
    public const string DataIsHave = "数据已存在";
    public const string DataIsNo = "数据不存在";
    public const string DataAccessDenied = "没有权限操作该数据";
    public const string DataParameterError = "请提交必要的参数";
    public const string GetDataSuccess = "获取数据成功";
    public const string GetDataFailure = "获取数据失败";
    public const string SetDataSuccess = "设置数据成功";
    public const string SetDataFailure = "设置数据失败";

    /// <summary>
    /// 通用状态
    /// </summary>
    public const string EnumValueInvalid = "状态值无效";
    public const string ViewPermissionDenied = "没有权限查看该数据";

    /// <summary>
    /// 头像图片校验
    /// </summary>
    public const string OssTempImageUploadEmpty = "请选择要上传的图片";
    public const string OssTempImageUploadTooLarge = "图片大小不能超过3MB";
    public const string OssTempImageUploadTypeInvalid = "仅支持 JPG、PNG、GIF、WebP 图片";
}
