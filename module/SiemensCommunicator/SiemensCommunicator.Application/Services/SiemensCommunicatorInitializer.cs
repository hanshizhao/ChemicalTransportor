using Microsoft.Extensions.Logging;
using SiemensCommunicator.Core.Interfaces;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// Siemens 通讯模块初始化器实现
/// </summary>
public class SiemensCommunicatorInitializer : ISiemensCommunicatorInitializer, ITransient
{
    private readonly IPlcConfigurationRepository _configurationRepository;
    private readonly IPlcConnectionService _connectionService;
    private readonly ILogger<SiemensCommunicatorInitializer> _logger;

    public SiemensCommunicatorInitializer(
        IPlcConfigurationRepository configurationRepository,
        IPlcConnectionService connectionService,
        ILogger<SiemensCommunicatorInitializer> logger)
    {
        _configurationRepository = configurationRepository;
        _connectionService = connectionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<InitializationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("开始初始化 Siemens 通讯模块");

        var result = new InitializationResult();
        var connections = await _configurationRepository.GetAllConnectionsAsync(cancellationToken);
        var activeConnections = connections.Where(c => c.IsActive).ToList();

        result.TotalCount = activeConnections.Count;

        _logger.LogInformation("发现 {Count} 个活动连接配置", result.TotalCount);

        foreach (var connection in activeConnections)
        {
            try
            {
                _logger.LogInformation(
                    "正在连接 PLC: {Name} ({Ip}:{Rack}/{Slot})",
                    connection.Name,
                    connection.Ip,
                    connection.Rack,
                    connection.Slot);

                var success = await _connectionService.ConnectAsync(connection.Id, cancellationToken);

                if (success)
                {
                    result.ConnectedCount++;
                    _logger.LogInformation("PLC {Name} 连接成功", connection.Name);
                }
                else
                {
                    result.FailedCount++;
                    result.FailedConnectionIds.Add(connection.Id);
                    result.Errors.Add($"连接 {connection.Name} 失败");
                    _logger.LogWarning("PLC {Name} 连接失败", connection.Name);
                }
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.FailedConnectionIds.Add(connection.Id);
                result.Errors.Add($"连接 {connection.Name} 异常: {ex.Message}");
                _logger.LogError(ex, "连接 PLC {Name} 时发生异常", connection.Name);
            }
        }

        result.IsSuccess = result.FailedCount == 0;

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Siemens 通讯模块初始化完成，成功连接 {Count}/{Total} 个 PLC",
                result.ConnectedCount,
                result.TotalCount);
        }
        else
        {
            _logger.LogWarning(
                "Siemens 通讯模块初始化部分完成，成功 {Success}/{Total}，失败 {Failed}",
                result.ConnectedCount,
                result.TotalCount,
                result.FailedCount);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ShutdownResult> ShutdownAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("开始关闭 Siemens 通讯模块");

        var result = new ShutdownResult();

        try
        {
            var disconnectedCount = await _connectionService.DisconnectAllAsync(cancellationToken);
            result.DisconnectedCount = disconnectedCount;
            result.IsSuccess = true;

            _logger.LogInformation("已断开 {Count} 个 PLC 连接", disconnectedCount);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Errors.Add($"关闭模块时发生异常: {ex.Message}");
            _logger.LogError(ex, "关闭 Siemens 通讯模块时发生异常");
        }

        return result;
    }
}
