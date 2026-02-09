using SiemensCommunicator.Core.Events;
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 连接管理服务接口
/// </summary>
public interface IPlcConnectionService
{
    #region 事件

    /// <summary>
    /// 连接状态改变事件
    /// </summary>
    event EventHandler<PlcStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// 错误发生事件
    /// </summary>
    event EventHandler<PlcErrorEventArgs>? ErrorOccurred;

    #endregion

    #region 连接管理

    /// <summary>
    /// 连接到指定的 PLC
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>连接是否成功</returns>
    Task<bool> ConnectAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开指定 PLC 的连接
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>断开是否成功</returns>
    Task<bool> DisconnectAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开所有 PLC 连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>断开的连接数量</returns>
    Task<int> DisconnectAllAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 状态查询

    /// <summary>
    /// 获取指定连接的状态
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>连接状态</returns>
    Task<PlcConnectionState> GetStateAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有连接的状态
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>连接ID与状态的字典</returns>
    Task<Dictionary<long, PlcConnectionState>> GetAllStatesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 活动连接管理

    /// <summary>
    /// 设置活动的连接
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>设置是否成功</returns>
    Task<bool> SetActiveConnectionAsync(long connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前活动的连接ID
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>活动连接ID，如果没有活动连接则返回 null</returns>
    Task<long?> GetActiveConnectionAsync(CancellationToken cancellationToken = default);

    #endregion
}
