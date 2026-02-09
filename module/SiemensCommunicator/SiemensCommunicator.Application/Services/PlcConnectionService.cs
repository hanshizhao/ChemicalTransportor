using S7.Net;
using SiemensCommunicator.Core.Events;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.Core.Models.Enums;
using CoreCpuType = SiemensCommunicator.Core.Models.Enums.CpuType;
using S7CpuType = S7.Net.CpuType;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// PLC 连接管理服务实现
/// </summary>
public class PlcConnectionService : IPlcConnectionService, ITransient
{
    private readonly IPlcConfigurationRepository _configurationRepository;
    private readonly Dictionary<long, Plc> _connections;
    private readonly Dictionary<long, PlcConnectionState> _connectionStates;
    private readonly Dictionary<long, object> _connectionLocks;
    private long? _activeConnectionId;
    private readonly object _activeConnectionLock = new();

    public PlcConnectionService(IPlcConfigurationRepository configurationRepository)
    {
        _configurationRepository = configurationRepository;
        _connections = new Dictionary<long, Plc>();
        _connectionStates = new Dictionary<long, PlcConnectionState>();
        _connectionLocks = new Dictionary<long, object>();
    }

    #region 事件

    /// <inheritdoc />
    public event EventHandler<PlcStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public event EventHandler<PlcErrorEventArgs>? ErrorOccurred;

    #endregion

    #region 连接管理

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await _configurationRepository.GetConnectionAsync(connectionId, cancellationToken);

        if (connection == null)
        {
            OnErrorOccurred(connectionId, PlcErrorType.InvalidConfig, "连接配置不存在");
            return false;
        }

        var lockObj = GetConnectionLock(connectionId);

        lock (lockObj)
        {
            if (_connections.TryGetValue(connectionId, out var existingPlc))
            {
                if (existingPlc.IsConnected)
                {
                    return true;
                }

                _connections.Remove(connectionId);
            }

            SetState(connectionId, PlcConnectionState.Connecting);
        }

        try
        {
            var cpuType = ConvertToCpuType(connection.CpuType);
            var plc = new Plc(cpuType, connection.Ip, (short)connection.Rack, (short)connection.Slot);

            lock (lockObj)
            {
                _connections[connectionId] = plc;
            }

            await Task.Run(() => plc.Open(), cancellationToken);

            if (plc.IsConnected)
            {
                SetState(connectionId, PlcConnectionState.Connected);
                return true;
            }
            else
            {
                SetState(connectionId, PlcConnectionState.Error);
                OnErrorOccurred(connectionId, PlcErrorType.ConnectionFailed, "PLC 连接失败");
                return false;
            }
        }
        catch (Exception ex)
        {
            SetState(connectionId, PlcConnectionState.Error);
            OnErrorOccurred(connectionId, PlcErrorType.ConnectionFailed, $"PLC 连接异常: {ex.Message}", ex);

            lock (lockObj)
            {
                if (_connections.TryGetValue(connectionId, out var plc))
                {
                    try
                    {
                        plc.Close();
                    }
                    catch { }
                    _connections.Remove(connectionId);
                }
            }

            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        var lockObj = GetConnectionLock(connectionId);

        lock (lockObj)
        {
            if (!_connections.TryGetValue(connectionId, out var plc))
            {
                SetState(connectionId, PlcConnectionState.Disconnected);
                return true;
            }

            try
            {
                if (plc.IsConnected)
                {
                    plc.Close();
                }

                _connections.Remove(connectionId);
                SetState(connectionId, PlcConnectionState.Disconnected);

                if (_activeConnectionId == connectionId)
                {
                    lock (_activeConnectionLock)
                    {
                        _activeConnectionId = null;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(connectionId, PlcErrorType.ConnectionLost, $"断开连接时发生错误: {ex.Message}", ex);
                return false;
            }
        }
    }

    /// <inheritdoc />
    public async Task<int> DisconnectAllAsync(CancellationToken cancellationToken = default)
    {
        var connectionIds = _connections.Keys.ToList();
        var count = 0;

        foreach (var connectionId in connectionIds)
        {
            if (await DisconnectAsync(connectionId, cancellationToken))
            {
                count++;
            }
        }

        return count;
    }

    #endregion

    #region 状态查询

    /// <inheritdoc />
    public Task<PlcConnectionState> GetStateAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        if (_connectionStates.TryGetValue(connectionId, out var state))
        {
            return Task.FromResult(state);
        }

        return Task.FromResult(PlcConnectionState.Disconnected);
    }

    /// <inheritdoc />
    public Task<Dictionary<long, PlcConnectionState>> GetAllStatesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<long, PlcConnectionState>(_connectionStates));
    }

    #endregion

    #region 活动连接管理

    /// <inheritdoc />
    public async Task<bool> SetActiveConnectionAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await _configurationRepository.GetConnectionAsync(connectionId, cancellationToken);

        if (connection == null)
        {
            return false;
        }

        lock (_activeConnectionLock)
        {
            _activeConnectionId = connectionId;
            return true;
        }
    }

    /// <inheritdoc />
    public Task<long?> GetActiveConnectionAsync(CancellationToken cancellationToken = default)
    {
        lock (_activeConnectionLock)
        {
            return Task.FromResult(_activeConnectionId);
        }
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 获取指定连接ID的Plc实例（供内部服务使用）
    /// </summary>
    internal Plc? GetPlc(long connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var plc))
        {
            return plc;
        }

        return null;
    }

    #endregion

    #region 私有方法

    private object GetConnectionLock(long connectionId)
    {
        lock (_connectionLocks)
        {
            if (!_connectionLocks.TryGetValue(connectionId, out var lockObj))
            {
                lockObj = new object();
                _connectionLocks[connectionId] = lockObj;
            }

            return lockObj;
        }
    }

    private void SetState(long connectionId, PlcConnectionState newState)
    {
        var oldState = PlcConnectionState.Disconnected;

        if (_connectionStates.TryGetValue(connectionId, out var currentState))
        {
            oldState = currentState;
        }

        _connectionStates[connectionId] = newState;

        if (oldState != newState)
        {
            OnStateChanged(connectionId, oldState, newState);
        }
    }

    private void OnStateChanged(long connectionId, PlcConnectionState oldState, PlcConnectionState newState)
    {
        StateChanged?.Invoke(this, new PlcStateChangedEventArgs
        {
            ConnectionId = connectionId.ToString(),
            OldState = oldState,
            NewState = newState,
            Timestamp = DateTime.Now
        });
    }

    private void OnErrorOccurred(long connectionId, PlcErrorType errorType, string message, Exception? exception = null)
    {
        ErrorOccurred?.Invoke(this, new PlcErrorEventArgs
        {
            ConnectionId = connectionId.ToString(),
            ErrorType = errorType,
            Message = message,
            OccurredAt = DateTime.Now,
            Exception = exception
        });
    }

    private static S7CpuType ConvertToCpuType(CoreCpuType cpuType)
    {
        return cpuType switch
        {
            CoreCpuType.S7200 => S7CpuType.S7200,
            CoreCpuType.S7300 => S7CpuType.S7300,
            CoreCpuType.S7400 => S7CpuType.S7400,
            CoreCpuType.S71200 => S7CpuType.S71200,
            CoreCpuType.S71500 => S7CpuType.S71500,
            _ => S7CpuType.S71200
        };
    }

    #endregion
}
