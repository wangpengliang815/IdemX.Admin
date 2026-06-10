namespace Core.Infrastructure.Options;

/// <summary>
/// e签宝配置（appsettings 节名：EsignOptions）
/// </summary>
public class EsignOptions
{
    /// <summary>
    /// e签宝开放平台连接配置
    /// </summary>
    public EsignConfigOptions ConfigOptions { get; set; } = new();

    /// <summary>
    /// 甲方经办人默认信息配置
    /// </summary>
    public JiafangOperatorOptions JiafangOperatorOptions { get; set; } = new();

    /// <summary>
    /// 甲方公司默认信息配置
    /// </summary>
    public JiafangDefaultOptions JiafangDefaultOptions { get; set; } = new();

    /// <summary>
    /// 各业务模块下的协议与 e签宝文档模板对应关系（按模块分组；新增模块时增加分组属性并在 EsignService 中参与合并）。
    /// </summary>
    public EsignBusinessDocTemplatesOptions BusinessDocTemplates { get; set; } = new();
}

/// <summary>
/// e签宝开放平台配置
/// </summary>
public class EsignConfigOptions
{
    /// <summary>
    /// e签宝服务地址（不含尾部斜杠）
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 应用ID（X-Tsign-Open-App-Id）
    /// </summary>
    public string OpenAppId { get; set; }

    /// <summary>
    /// 应用密钥（用于签名、OAuth 等）
    /// </summary>
    public string OpenAppSecret { get; set; }

    /// <summary>
    /// 机构账号ID
    /// </summary>
    public string OrgId { get; set; }
}

/// <summary>
/// 合同甲方经办人默认信息配置
/// </summary>
public class JiafangOperatorOptions
{
    /// <summary>
    /// 默认机构名称（签署流程或合同中的展示名）。
    /// </summary>
    public string OrgName { get; set; }

    /// <summary>
    /// 经办人在 e签宝的个人账号标识（常见为手机号或登录账号）。
    /// </summary>
    public string TransactorPsnAccount { get; set; }

    /// <summary>
    /// 经办人姓名，与经办人个人账号对应。
    /// </summary>
    public string TransactorName { get; set; }
}

/// <summary>
/// 合同甲方默认配置信息
/// </summary>
public class JiafangDefaultOptions
{
    /// <summary>
    /// 甲方公司法人姓名
    /// </summary>
    public string LegalPerson { get; set; }

    /// <summary>
    /// 甲方公司授权代表姓名
    /// </summary>
    public string AuthorPerson { get; set; }

    /// <summary>
    /// 甲方公司注册或通讯地址
    /// </summary>
    public string CompanyAddress { get; set; }

    /// <summary>
    /// 甲方公司联系邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 甲方公司联系电话
    /// </summary>
    public string Phone { get; set; }
}

/// <summary>
/// 按业务模块分组的 e签宝合同模板配置
/// </summary>
public class EsignBusinessDocTemplatesOptions
{
    /// <summary>
    /// 公司模块：入驻类协议（AgreementKey 与 CompanyEsignAgreementType 枚举名一致）
    /// </summary>
    public List<EsignModuleDocTemplateItemOptions> Company { get; set; } = [];

    /// <summary>
    /// 合约模块：发起电子签时取列表中首条 AgreementKey、DocTemplateId 均非空的项（展示名与匹配键均为该项 AgreementKey）。
    /// </summary>
    public List<EsignModuleDocTemplateItemOptions> Contract { get; set; } = [];
}

/// <summary>
/// 单个模块内：协议键与 e签宝模板对应关系
/// </summary>
public class EsignModuleDocTemplateItemOptions
{
    /// <summary>
    /// 协议键（公司模块须与 CompanyEsignAgreementType 枚举名一致；合约模块为 Contract 列表首条有效项的键并用于签署流程展示）
    /// </summary>
    public string AgreementKey { get; set; }

    /// <summary>
    /// e签宝文档模板Id
    /// </summary>
    public string DocTemplateId { get; set; }

    /// <summary>
    /// e签宝侧模板名称
    /// </summary>
    public string DocTemplateName { get; set; }
}
