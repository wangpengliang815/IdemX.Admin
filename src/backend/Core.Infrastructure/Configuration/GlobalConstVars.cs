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
    public const string AuthMismatchingFailure = "账户密码不匹配";
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
    public const string RegInviteRequired = "请填写邀请码";
    public const string RegInviteInvalid = "邀请码无效、已使用或已过期";
    public const string RegFailed = "注册失败，请稍后重试";
    public const string RegSuccess = "注册成功";

    /// <summary>
    /// 身份认证相关
    /// </summary>
    public const string IdCardVerifySuccess = "身份核验通过";
    public const string IdCardVerifyFailure = "身份核验失败：身份证与姓名不匹配";
    public const string IdCardInfoEmpty = "姓名和身份证号不能为空";

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
    /// 商品主数据（分类/品牌/单位）
    /// </summary>
    public const string MasterDataNameEmptyFailure = "名称不能为空";

    /// <summary>
    /// 商品分类相关
    /// </summary>
    public const string CategoryInvalidParentFailure = "无效的上级分类";
    public const string CategorySelfParentFailure = "上级不能为本类";
    public const string CategoryChildParentFailure = "上级不能为本类子类";
    public const string CategoryDuplicateNameFailure = "同级分类中已存在相同的名称";
    public const string CategoryMaxLevelFailure = "分类层级不能超过{0}级";
    public const string CategoryMoveChildLevelExceedsMax = "无法移动分类：移动后分类「{0}」的层级（{1}）将超过最大限制（{2}）";
    public const string CategoryChangeParentHasDependentFailure = "该分类已关联商品或品牌，不能修改上级分类";
    public const string CategoryMapBrandOnlyLevel3Failure = "仅三级分类可关联品牌";

    /// <summary>
    /// 商品品牌（主数据）
    /// </summary>
    public const string BrandCnNameDuplicateFailure = "中文品牌名称已存在";
    public const string BrandInvalidIdFailure = "存在无效的品牌";
    public const string BrandEnNameDuplicateFailure = "英文品牌名称已存在";

    /// <summary>
    /// 销售单位（主数据）
    /// </summary>
    public const string ProductUnitNameDuplicateFailure = "单位名称已存在";

    /// <summary>
    /// 头像上传
    /// </summary>
    public const string UploadAvatarSuccess = "头像更新成功";
    public const string UploadAvatarFailure = "头像更新失败";

    /// <summary>
    /// 文件操作
    /// </summary>
    public const string FileSuccess = "文件上传成功";
    
    public const string FileFailure = "文件上传失败";

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
    /// 订单
    /// </summary>
    public const string OrderRejectSuccess = "订单已被商务官拒绝";

    /// <summary>
    /// 数据移动
    /// </summary>
    public const string MoveSuccess = "数据移动成功";

    public const string MoveFailure = "数据移动失败";

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
    public const string DataDateTimeFormatInvalid = "时间参数格式不正确";
    public const string GetDataSuccess = "获取数据成功";
    public const string GetDataException = "获取数据异常";
    public const string GetDataFailure = "获取数据失败";
    public const string SetDataSuccess = "设置数据成功";
    public const string SetDataException = "设置数据异常";
    public const string SetDataFailure = "设置数据失败";

    /// <summary>
    /// 通用状态
    /// </summary>
    public const string EnumValueInvalid = "状态值无效";
    public const string AuditPermissionDenied = "无权审核该数据";
    public const string AuditSuccess = "数据审核成功";
    public const string AuditFailure = "数据审核失败";

    public const string ViewPermissionDenied = "没有权限查看该数据";

    public const string AuditStatusInvalid = "数据状态不正确，无法审核";
    public const string AuditRejectRemarkRequired = "审核拒绝时必须填写驳回原因";

    public const string OperateSuccess = "操作成功";

    public const string DataStatusInvalidCannotEdit = "数据状态不正确，无法修改";
    public const string DataStatusInvalidCannotDelete = "数据状态不正确，无法删除";

    /// <summary>
    /// 公司收货地址
    /// </summary>
    public const string ConsigneeSetDefaultRequiresApproved = "仅审核通过的收货地址可设为默认";

    /// <summary>
    /// OSS 图片暂存上传（商品/公司/合约等模块复用）
    /// </summary>
    public const string OssTempImageUploadEmpty = "请选择要上传的图片";
    public const string OssTempImageUploadTooLarge = "图片大小不能超过3MB";
    public const string OssTempImageUploadTypeInvalid = "仅支持 JPG、PNG、GIF、WebP 图片";
}