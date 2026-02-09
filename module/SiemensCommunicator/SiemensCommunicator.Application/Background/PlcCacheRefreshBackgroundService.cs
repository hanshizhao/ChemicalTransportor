using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiemensCommunicator.Core.Interfaces;

namespace SiemensCommunicator.Application.Background;

/// <summary>
/// PLC 缓存刷新后台服务
/// </summary>
public class PlcCacheRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlcCacheRefreshBackgroundService> _logger;
    private readonly Dictionary<long, Timer> _timers;
    private readonly object _lock = new();

    public PlcCacheRefreshBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PlcCacheRefreshBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _timers = new Dictionary<long, Timer>();
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC 缓存刷新后台服务启动");

        // 初始化所有活动的连接的定时器
        await InitializeTimersAsync(stoppingToken);

        // 等待取消请求
        await Task.Delay(Timeout.Infinite, stoppingToken);

        // 清理所有定时器
        CleanupTimers();

        _logger.LogInformation("PLC 缓存刷新后台服务停止");
    }

    /// <summary>
    /// 初始化所有活动连接的定时器
    /// </summary>
    private async Task InitializeTimersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var configurationRepository = scope.ServiceProvider.GetRequiredService<IPlcConfigurationRepository>();

        var connections = await configurationRepository.GetAllConnectionsAsync(cancellationToken);

        foreach (var connection in connections.Where(c => c.IsActive))
        {
            if (connection.CacheRefreshInterval > 0)
            {
                CreateTimer(connection.Id, connection.CacheRefreshInterval);
                _logger.LogInformation(
                    "为连接 {ConnectionId} ({ConnectionName}) 创建缓存刷新定时器，间隔: {Interval}ms",
                    connection.Id,
                    connection.Name,
                    connection.CacheRefreshInterval);
            }
        }
    }

    /// <summary>
    /// 为指定连接创建定时器
    /// </summary>
    private void CreateTimer(long connectionId, int interval)
    {
        lock (_lock)
        {
            // 如果已存在定时器，先释放
            if (_timers.TryGetValue(connectionId, out var existingTimer))
            {
                existingTimer.Dispose();
                _timers.Remove(connectionId);
            }

            // 创建新的定时器
            var timer = new Timer(
                callback: async _ => await RefreshConnectionCacheAsync(connectionId),
                state: null,
                dueTime: interval,  // 首次延迟
                period: interval);   // 周期

            _timers[connectionId] = timer;
        }
    }

    /// <summary>
    /// 移除指定连接的定时器
    /// </summary>
    private void RemoveTimer(long connectionId)
    {
        lock (_lock)
        {
            if (_timers.TryGetValue(connectionId, out var timer))
            {
                timer.Dispose();
                _timers.Remove(connectionId);
                _logger.LogInformation("已移除连接 {ConnectionId} 的缓存刷新定时器", connectionId);
            }
        }
    }

    /// <summary>
    /// 刷新连接缓存
    /// </summary>
    private async Task RefreshConnectionCacheAsync(long connectionId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheManager = scope.ServiceProvider.GetRequiredService<IPlcCacheManager>();

            var count = await cacheManager.RefreshConnectionAsync(connectionId);

            if (count > 0)
            {
                _logger.LogDebug(
                    "连接 {ConnectionId} 缓存刷新完成，刷新数据点: {Count}",
                    connectionId,
                    count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接 {ConnectionId} 缓存刷新失败", connectionId);
        }
    }

    /// <summary>
    /// 清理所有定时器
    /// </summary>
    private void CleanupTimers()
    {
        lock (_lock)
        {
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }

            _timers.Clear();
        }
    }

    /// <summary>
    /// 为连接添加或更新定时器（供外部调用）
    /// </summary>
    public void AddOrUpdateTimer(long connectionId, int interval)
    {
        if (interval > 0)
        {
            CreateTimer(connectionId, interval);
            _logger.LogInformation(
                "已更新连接 {ConnectionId} 的缓存刷新定时器，间隔: {Interval}ms",
                connectionId,
                interval);
        }
    }

    /// <summary>
    /// 移除连接定时器（供外部调用）
    /// </summary>
    public void RemoveConnectionTimer(long connectionId)
    {
        RemoveTimer(connectionId);
    }
}
