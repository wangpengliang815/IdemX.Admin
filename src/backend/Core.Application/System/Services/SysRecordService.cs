namespace Core.Application;

public class SysRecordService(IBaseRepo<SysRecordLogin> loginRepo
    , IUnitOfWork unitOfWork
    , IMapper mapper) : ISysRecordService
{
    /// <summary>
    /// 登录日志分页列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<List<SysRecordLoginResp>>> GetLoginPageListAsync(SysRecordLoginPageQueryReq request)
    {
        var where = PredicateBuilder.True<SysRecordLogin>();

        if (!string.IsNullOrEmpty(request.UserName))
            where = where.And(p => p.UserName.Contains(request.UserName));

        if (!string.IsNullOrEmpty(request.StartTime) && !string.IsNullOrEmpty(request.EndTime))
        {
            if (DateTime.TryParse(request.StartTime, out var startTime) &&
                DateTime.TryParse(request.EndTime, out var endTime))
            {
                startTime = startTime.Date;
                endTime = endTime.Date.AddDays(1).AddSeconds(-1);
                where = where.And(p => p.CreateTime >= startTime && p.CreateTime <= endTime);
            }
        }

        var list = await loginRepo.GetPageAsync(where, p => p.Id, OrderByType.Desc, request.Page, request.PageSize);
        var dtos = mapper.Map<List<SysRecordLoginResp>>(list);

        return CustomApiResponse<List<SysRecordLoginResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos, list.TotalCount);
    }

    /// <summary>
    /// 清空登录日志表
    /// </summary>
    /// <returns></returns>
    public Task<CustomApiResponse> ClearLoginDataAsync() =>
        TruncateRecordTableAsync("TRUNCATE TABLE public.sys_record_login RESTART IDENTITY");

    /// <summary>
    /// 全局NLog分页列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<List<SysRecordNlogResp>>> GetNLogPageListAsync(SysRecordNlogPageQueryReq request)
    {
        var where = PredicateBuilder.True<SysRecordNlog>();

        if (!string.IsNullOrEmpty(request.LogLevel))
            where = where.And(p => p.LogLevel == request.LogLevel);

        if (!string.IsNullOrEmpty(request.StartTime) && !string.IsNullOrEmpty(request.EndTime))
        {
            if (DateTime.TryParse(request.StartTime, out var startTime) &&
                DateTime.TryParse(request.EndTime, out var endTime))
            {
                startTime = startTime.Date;
                endTime = endTime.Date.AddDays(1).AddSeconds(-1);
                where = where.And(p => p.LogDate >= startTime && p.LogDate <= endTime);
            }
        }

        // NLog 目标表，非业务 BaseEntity 仓储模型，故意不经 IBaseRepo 分页
        var db = unitOfWork.GetDbClient();

        RefAsync<int> totalCount = 0;
        var list = await db.Queryable<SysRecordNlog>()
            .Where(where)
            .OrderBy(p => p.LogDate, OrderByType.Desc)
            .ToPageListAsync(request.Page, request.PageSize, totalCount);

        var dtos = mapper.Map<List<SysRecordNlogResp>>(list);
        return CustomApiResponse<List<SysRecordNlogResp>>.Ok(GlobalConstVars.GetDataSuccess, dtos, totalCount);
    }

    /// <summary>
    /// 清空全局NLog表
    /// </summary>
    /// <returns></returns>
    public Task<CustomApiResponse> ClearNLogDataAsync() =>
        TruncateRecordTableAsync("TRUNCATE TABLE public.sys_record_nlog RESTART IDENTITY");

    /// <summary>
    /// 执行 TRUNCATE：不抛异常即成功，不根据影响行数判定
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    private async Task<CustomApiResponse> TruncateRecordTableAsync(string sql)
    {
        await unitOfWork.GetDbClient().Ado.ExecuteCommandAsync(sql);
        return CustomApiResponse.Ok(GlobalConstVars.DeleteSuccess);
    }
}
