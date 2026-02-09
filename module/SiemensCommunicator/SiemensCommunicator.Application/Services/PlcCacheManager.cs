using Microsoft.Extensions.Caching.Memory;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// PLC 缓存管理器实现
/// </summary>
public class PlcCacheManager : IPlcCacheManager, ITransient
{
    private readonly IMemoryCache _cache;
    private readonly IPlcConfigurationRepository _configurationRepository;
    private readonly IPlcDataService _dataService;
    private readonly Dictionary<string, DateTime> _updateTimes;
    private readonly object _lock = new();

    private const string CacheKeyPrefix = "PlcCache_";
    private const string UpdateTimeKeyPrefix = "PlcCacheTime_";

    public PlcCacheManager(
        IMemoryCache cache,
        IPlcConfigurationRepository configurationRepository,
        IPlcDataService dataService)
    {
        _cache = cache;
        _configurationRepository = configurationRepository;
        _dataService = dataService;
        _updateTimes = new Dictionary<string, DateTime>();
    }

    #region 单点缓存操作

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(connectionId, dataPointName);

        if (_cache.TryGetValue<T>(cacheKey, out var value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult<T?>(default);
    }

    /// <inheritdoc />
    public Task SetAsync(long connectionId, string dataPointName, object? value, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(connectionId, dataPointName);
        var timeKey = GetTimeKey(connectionId, dataPointName);

        lock (_lock)
        {
            _cache.Set(cacheKey, value);
            _updateTimes[timeKey] = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region 批量缓存操作

    /// <inheritdoc />
    public Task<Dictionary<string, object?>> GetBatchAsync(
        long connectionId,
        IEnumerable<string> dataPointNames,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, object?>();

        foreach (var dataPointName in dataPointNames)
        {
            var cacheKey = GetCacheKey(connectionId, dataPointName);

            if (_cache.TryGetValue(cacheKey, out var value))
            {
                result[dataPointName] = value;
            }
            else
            {
                result[dataPointName] = null;
            }
        }

        return Task.FromResult(result);
    }

    #endregion

    #region 刷新操作

    /// <inheritdoc />
    public async Task<bool> RefreshDataPointAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataPoint = await _configurationRepository.GetDataPointByNameAsync(dataPointName, cancellationToken);

            if (dataPoint == null || dataPoint.ConnectionId != connectionId)
            {
                return false;
            }

            var value = await _dataService.ReadAsync(connectionId, dataPointName, cancellationToken);

            await SetAsync(connectionId, dataPointName, value, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> RefreshConnectionAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        var dataPoints = await _configurationRepository.GetDataPointsByConnectionAsync(connectionId, cancellationToken);
        var count = 0;

        foreach (var dataPoint in dataPoints.Where(dp => dp.EnableCache))
        {
            if (await RefreshDataPointAsync(connectionId, dataPoint.Name, cancellationToken))
            {
                count++;
            }
        }

        return count;
    }

    /// <inheritdoc />
    public async Task<int> RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        var connections = await _configurationRepository.GetAllConnectionsAsync(cancellationToken);
        var total = 0;

        foreach (var connection in connections.Where(c => c.IsActive))
        {
            total += await RefreshConnectionAsync(connection.Id, cancellationToken);
        }

        return total;
    }

    #endregion

    #region 缓存状态查询

    /// <inheritdoc />
    public Task<DateTime?> GetLastUpdateTimeAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var timeKey = GetTimeKey(connectionId, dataPointName);

        lock (_lock)
        {
            if (_updateTimes.TryGetValue(timeKey, out var updateTime))
            {
                return Task.FromResult<DateTime?>(updateTime);
            }
        }

        return Task.FromResult<DateTime?>(null);
    }

    /// <inheritdoc />
    public async Task<bool> IsExpiredAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var dataPoint = await _configurationRepository.GetDataPointByNameAsync(dataPointName, cancellationToken);

        if (dataPoint == null || dataPoint.ConnectionId != connectionId)
        {
            return true;
        }

        var connection = await _configurationRepository.GetConnectionAsync(connectionId, cancellationToken);

        if (connection == null)
        {
            return true;
        }

        var lastUpdateTime = await GetLastUpdateTimeAsync(connectionId, dataPointName, cancellationToken);

        if (lastUpdateTime == null)
        {
            return true;
        }

        var expiryTime = lastUpdateTime.Value.AddMilliseconds(connection.CacheRefreshInterval);

        return DateTime.UtcNow > expiryTime;
    }

    #endregion

    #region 缓存管理

    /// <inheritdoc />
    public Task ClearDataPointAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(connectionId, dataPointName);
        var timeKey = GetTimeKey(connectionId, dataPointName);

        lock (_lock)
        {
            _cache.Remove(cacheKey);
            _updateTimes.Remove(timeKey);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ClearConnectionAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        var dataPoints = await _configurationRepository.GetDataPointsByConnectionAsync(connectionId, cancellationToken);

        foreach (var dataPoint in dataPoints)
        {
            await ClearDataPointAsync(connectionId, dataPoint.Name, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var connections = await _configurationRepository.GetAllConnectionsAsync(cancellationToken);

        foreach (var connection in connections)
        {
            await ClearConnectionAsync(connection.Id, cancellationToken);
        }
    }

    #endregion

    #region 私有方法

    private static string GetCacheKey(long connectionId, string dataPointName)
    {
        return $"{CacheKeyPrefix}{connectionId}_{dataPointName}";
    }

    private static string GetTimeKey(long connectionId, string dataPointName)
    {
        return $"{UpdateTimeKeyPrefix}{connectionId}_{dataPointName}";
    }

    #endregion
}
