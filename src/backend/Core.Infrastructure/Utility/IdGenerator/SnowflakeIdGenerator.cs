using Yitter.IdGenerator;

namespace Core.Infrastructure.Utility;

/// <summary>
/// 雪花 ID 生成器（基于 Yitter.IdGenerator）
/// </summary>
public class SnowflakeIdGenerator : IIdGenerator
{
    /// <summary>
    /// 构造函数（初始化 Yitter 全局配置）
    /// </summary>
    /// <param name="workerId">机器码（0-63），默认 1</param>
    public SnowflakeIdGenerator(ushort workerId = 1)
    {
        var options = new IdGeneratorOptions(workerId)
        {
            WorkerIdBitLength = 6,          // 机器码位数（支持 0-63）
            SeqBitLength = 6,               // 序列号位数
            MinSeqNumber = 5,               // 最小序列号
            MaxSeqNumber = 50,              // 最大序列号（范围 1-63）
            TopOverCostCount = 2000,        // 最大漂移次数
            BaseTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        YitIdHelper.SetIdGenerator(options);
    }

    /// <summary>
    /// 生成下一个 ID
    /// </summary>
    public long NextId()
    {
        return YitIdHelper.NextId();
    }
}


