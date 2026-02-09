namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// Siemens 通讯模块初始化器接口
/// </summary>
public interface ISiemensCommunicatorInitializer
{
    /// <summary>
    /// 初始化模块，连接所有活动配置的 PLC
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>初始化结果</returns>
    Task<InitializationResult> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止模块，断开所有 PLC 连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>停止结果</returns>
    Task<ShutdownResult> ShutdownAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 初始化结果
/// </summary>
public class InitializationResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 成功连接的 PLC 数量
    /// </summary>
    public int ConnectedCount { get; set; }

    /// <summary>
    /// 失败连接的 PLC 数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 总 PLC 数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 连接失败的 PLC ID 列表
    /// </summary>
    public List<long> FailedConnectionIds { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 停止结果
/// </summary>
public class ShutdownResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 成功断开的 PLC 数量
    /// </summary>
    public int DisconnectedCount { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
