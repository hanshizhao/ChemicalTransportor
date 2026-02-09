namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 缓存管理器接口
/// </summary>
public interface IPlcCacheManager
{
    #region 单点缓存操作

    /// <summary>
    /// 从缓存获取指定数据点的值
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存中的值，如果不存在或已过期则返回 null</returns>
    Task<T?> GetAsync<T>(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置指定数据点的缓存值
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="value">要缓存的值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SetAsync(long connectionId, string dataPointName, object? value, CancellationToken cancellationToken = default);

    #endregion

    #region 批量缓存操作

    /// <summary>
    /// 批量获取多个数据点的缓存值
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointNames">数据点名称集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据点名称与值的字典</returns>
    Task<Dictionary<string, object?>> GetBatchAsync(
        long connectionId,
        IEnumerable<string> dataPointNames,
        CancellationToken cancellationToken = default);

    #endregion

    #region 刷新操作

    /// <summary>
    /// 刷新指定数据点的缓存（从PLC读取最新值）
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新是否成功</returns>
    Task<bool> RefreshDataPointAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新指定连接的所有缓存数据
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新的数据点数量</returns>
    Task<int> RefreshConnectionAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新所有连接的缓存数据
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新的数据点总数</returns>
    Task<int> RefreshAllAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 缓存状态查询

    /// <summary>
    /// 获取指定数据点的最后更新时间
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最后更新时间，如果不存在则返回 null</returns>
    Task<DateTime?> GetLastUpdateTimeAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查指定数据点的缓存是否过期
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否过期</returns>
    Task<bool> IsExpiredAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除指定数据点的缓存
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task ClearDataPointAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除指定连接的所有缓存
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task ClearConnectionAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task ClearAllAsync(CancellationToken cancellationToken = default);

    #endregion
}
