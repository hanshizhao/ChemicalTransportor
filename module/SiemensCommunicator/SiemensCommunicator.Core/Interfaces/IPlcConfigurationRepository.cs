namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 配置仓储接口
/// </summary>
public interface IPlcConfigurationRepository
{
    #region 连接配置

    /// <summary>
    /// 获取所有连接配置
    /// </summary>
    Task<List<Models.PlcConnection>> GetAllConnectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取连接配置
    /// </summary>
    Task<Models.PlcConnection?> GetConnectionAsync(long id, CancellationToken cancellationToken = default);

    #endregion

    #region 数据点配置

    /// <summary>
    /// 获取所有数据点
    /// </summary>
    Task<List<Models.PlcDataPoint>> GetAllDataPointsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据连接ID获取数据点
    /// </summary>
    Task<List<Models.PlcDataPoint>> GetDataPointsByConnectionAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据分组ID获取数据点
    /// </summary>
    Task<List<Models.PlcDataPoint>> GetDataPointsByGroupAsync(long groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取数据点
    /// </summary>
    Task<Models.PlcDataPoint?> GetDataPointAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据名称获取数据点
    /// </summary>
    Task<Models.PlcDataPoint?> GetDataPointByNameAsync(string name, CancellationToken cancellationToken = default);

    #endregion

    #region 分组配置

    /// <summary>
    /// 获取所有分组
    /// </summary>
    Task<List<Models.PlcGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 标签配置

    /// <summary>
    /// 获取所有标签
    /// </summary>
    Task<List<Models.PlcTag>> GetAllTagsAsync(CancellationToken cancellationToken = default);

    #endregion
}
