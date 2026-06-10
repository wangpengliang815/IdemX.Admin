namespace Core.Infrastructure.Mapping;

public interface IAutoMapperIProfile
{
}

/// <summary>
/// AutoMapper的全局实体映射配置静态类
/// </summary>
public class AutoMapperConfiguration : Profile, IAutoMapperIProfile
{
    public AutoMapperConfiguration()
    {
        // 组织机构
        CreateMap<SysOrgResp, SysOrg>().ReverseMap();
        CreateMap<SysOrgReq, SysOrg>();

        // 系统菜单
        CreateMap<SysMenuResp, SysMenu>().ReverseMap();
        CreateMap<SysMenuReq, SysMenu>();

        // 系统角色
        CreateMap<SysRoleResp, SysRole>().ReverseMap();
        CreateMap<SysRoleReq, SysRole>();

        // 内部用户管理
        CreateMap<SysUser, SysUserResp>().ReverseMap();
        CreateMap<SysUserReq, SysUser>();

        // 登录日志
        CreateMap<SysRecordLogin, SysRecordLoginResp>();
        // 全局 NLog 日志
        CreateMap<SysRecordNlog, SysRecordNlogResp>();
    }
}
