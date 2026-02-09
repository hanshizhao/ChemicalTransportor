using S7.Net;
using S7.Net.Types;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.Core.Models.Enums;
using CoreDataType = SiemensCommunicator.Core.Models.Enums.DataType;
using CoreVarType = SiemensCommunicator.Core.Interfaces.VarType;
using S7DataType = S7.Net.DataType;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// PLC 数据读写服务实现
/// </summary>
public class PlcDataService : IPlcDataService, ITransient
{
    private readonly IPlcConnectionService _connectionService;
    private readonly IPlcConfigurationRepository _configurationRepository;
    private readonly IPlcCacheManager _cacheManager;

    public PlcDataService(
        IPlcConnectionService connectionService,
        IPlcConfigurationRepository configurationRepository,
        IPlcCacheManager cacheManager)
    {
        _connectionService = connectionService;
        _configurationRepository = configurationRepository;
        _cacheManager = cacheManager;
    }

    #region 单点读取

    /// <inheritdoc />
    public async Task<object?> ReadAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var dataPoint = await _configurationRepository.GetDataPointByNameAsync(dataPointName, cancellationToken);

        if (dataPoint == null || dataPoint.ConnectionId != connectionId)
        {
            throw new InvalidOperationException($"数据点 {dataPointName} 不存在或不属于连接 {connectionId}");
        }

        var plc = await GetConnectedPlcAsync(connectionId, cancellationToken);

        if (plc == null)
        {
            throw new InvalidOperationException($"连接 {connectionId} 未建立");
        }

        try
        {
            object? value = await ReadValueByDataPointAsync(plc, dataPoint, cancellationToken);

            if (dataPoint.EnableCache)
            {
                await _cacheManager.SetAsync(connectionId, dataPointName, value, cancellationToken);
            }

            return value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"读取数据点 {dataPointName} 失败: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<T?> ReadAsync<T>(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var value = await ReadAsync(connectionId, dataPointName, cancellationToken);

        if (value == null)
        {
            return default;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            throw new InvalidCastException($"无法将 {value.GetType().Name} 转换为 {typeof(T).Name}");
        }
    }

    #endregion

    #region 批量读取

    /// <inheritdoc />
    public async Task<Dictionary<string, object?>> ReadBatchAsync(
        long connectionId,
        IEnumerable<string> dataPointNames,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, object?>();

        foreach (var dataPointName in dataPointNames)
        {
            try
            {
                var value = await ReadAsync(connectionId, dataPointName, cancellationToken);
                result[dataPointName] = value;
            }
            catch
            {
                result[dataPointName] = null;
            }
        }

        return result;
    }

    #endregion

    #region 写入

    /// <inheritdoc />
    public async Task<bool> WriteAsync(
        long connectionId,
        string dataPointName,
        object value,
        CancellationToken cancellationToken = default)
    {
        var dataPoint = await _configurationRepository.GetDataPointByNameAsync(dataPointName, cancellationToken);

        if (dataPoint == null || dataPoint.ConnectionId != connectionId)
        {
            return false;
        }

        var plc = await GetConnectedPlcAsync(connectionId, cancellationToken);

        if (plc == null)
        {
            return false;
        }

        try
        {
            await WriteValueByDataPointAsync(plc, dataPoint, value, cancellationToken);

            if (dataPoint.EnableCache)
            {
                await _cacheManager.SetAsync(connectionId, dataPointName, value, cancellationToken);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> WriteBatchAsync(
        long connectionId,
        Dictionary<string, object> dataPoints,
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        foreach (var kvp in dataPoints)
        {
            if (await WriteAsync(connectionId, kvp.Key, kvp.Value, cancellationToken))
            {
                count++;
            }
        }

        return count;
    }

    #endregion

    #region 原始读写

    /// <inheritdoc />
    public async Task<object?> ReadRawAsync(
        long connectionId,
        int dbNumber,
        int startByte,
        CoreVarType varType,
        int dataCount = 1,
        CancellationToken cancellationToken = default)
    {
        var plc = await GetConnectedPlcAsync(connectionId, cancellationToken);

        if (plc == null)
        {
            throw new InvalidOperationException($"连接 {connectionId} 未建立");
        }

        try
        {
            var s7VarType = ConvertToS7VarType(varType);
            return await Task.Run(() => plc.Read(S7DataType.DataBlock, dbNumber, startByte, s7VarType, dataCount), cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"读取原始数据失败: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> WriteRawAsync(
        long connectionId,
        int dbNumber,
        int startByte,
        CoreVarType varType,
        object value,
        CancellationToken cancellationToken = default)
    {
        var plc = await GetConnectedPlcAsync(connectionId, cancellationToken);

        if (plc == null)
        {
            return false;
        }

        try
        {
            await Task.Run(() => plc.Write(S7DataType.DataBlock, dbNumber, startByte, value), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region 缓存读取

    /// <inheritdoc />
    public async Task<T?> GetCachedValueAsync<T>(long connectionId, string dataPointName, CancellationToken cancellationToken = default)
    {
        var dataPoint = await _configurationRepository.GetDataPointByNameAsync(dataPointName, cancellationToken);

        if (dataPoint == null || !dataPoint.EnableCache)
        {
            return default;
        }

        var isExpired = await _cacheManager.IsExpiredAsync(connectionId, dataPointName, cancellationToken);

        if (isExpired)
        {
            await _cacheManager.RefreshDataPointAsync(connectionId, dataPointName, cancellationToken);
        }

        return await _cacheManager.GetAsync<T>(connectionId, dataPointName, cancellationToken);
    }

    #endregion

    #region 私有方法

    private async Task<Plc?> GetConnectedPlcAsync(long connectionId, CancellationToken cancellationToken)
    {
        var state = await _connectionService.GetStateAsync(connectionId, cancellationToken);

        if (state != PlcConnectionState.Connected)
        {
            var connected = await _connectionService.ConnectAsync(connectionId, cancellationToken);

            if (!connected)
            {
                return null;
            }
        }

        if (_connectionService is PlcConnectionService connectionService)
        {
            return connectionService.GetPlc(connectionId);
        }

        return null;
    }

    private static async Task<object?> ReadValueByDataPointAsync(Plc plc, PlcDataPoint dataPoint, CancellationToken cancellationToken)
    {
        var dataType = ConvertToDataType(dataPoint.DataType);
        var coreVarType = ParseCoreVarType(dataPoint.VarType);

        return coreVarType switch
        {
            CoreVarType.Bool => await ReadBoolAsync(plc, dataType, dataPoint.DbNumber, dataPoint.StartByte, dataPoint.BitOffset, cancellationToken),
            CoreVarType.Byte => (byte)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.Byte, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.Word => (ushort)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.Word, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.DWord => (uint)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.DWord, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.Int => (short)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.Int, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.DInt => (int)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.DInt, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.Real => (float)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.Real, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.LReal => (double)(await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.LReal, dataPoint.DataCount), cancellationToken))!,
            CoreVarType.String => await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.String, dataPoint.DataCount), cancellationToken) as string,
            CoreVarType.Timer => await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.Timer, dataPoint.DataCount), cancellationToken),
            CoreVarType.Counter => await Task.Run(() => plc.Read(dataType, dataPoint.DbNumber, dataPoint.StartByte, S7.Net.VarType.Counter, dataPoint.DataCount), cancellationToken),
            CoreVarType.ByteArray => await ReadBytesAsync(plc, dataType, dataPoint.DbNumber, dataPoint.StartByte, dataPoint.DataCount, cancellationToken),
            _ => null
        };
    }

    private static async Task WriteValueByDataPointAsync(Plc plc, PlcDataPoint dataPoint, object value, CancellationToken cancellationToken)
    {
        var dataType = ConvertToDataType(dataPoint.DataType);
        var coreVarType = ParseCoreVarType(dataPoint.VarType);

        await Task.Run(() =>
        {
            switch (coreVarType)
            {
                case CoreVarType.Bool:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (bool)value);
                    break;
                case CoreVarType.Byte:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (byte)value);
                    break;
                case CoreVarType.Word:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (ushort)value);
                    break;
                case CoreVarType.DWord:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (uint)value);
                    break;
                case CoreVarType.Int:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (short)value);
                    break;
                case CoreVarType.DInt:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (int)value);
                    break;
                case CoreVarType.Real:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (float)value);
                    break;
                case CoreVarType.LReal:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, (double)value);
                    break;
                case CoreVarType.String:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, value.ToString() ?? string.Empty);
                    break;
                case CoreVarType.Timer:
                case CoreVarType.Counter:
                case CoreVarType.ByteArray:
                    plc.Write(dataType, dataPoint.DbNumber, dataPoint.StartByte, value);
                    break;
            }
        }, cancellationToken);
    }

    private static async Task<bool> ReadBoolAsync(Plc plc, S7DataType dataType, int dbNumber, int startByte, int bitOffset, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var value = plc.Read(dataType, dbNumber, startByte, S7.Net.VarType.Byte, 1);
            if (value is byte byteValue)
            {
                return ((byteValue >> bitOffset) & 1) == 1;
            }
            return false;
        }, cancellationToken);
    }

    private static async Task<byte[]> ReadBytesAsync(Plc plc, S7DataType dataType, int dbNumber, int startByte, int dataCount, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var result = new byte[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var value = plc.Read(dataType, dbNumber, startByte + i, S7.Net.VarType.Byte, 1);
                if (value is byte byteValue)
                {
                    result[i] = byteValue;
                }
            }
            return result;
        }, cancellationToken);
    }

    private static CoreVarType ParseCoreVarType(string varTypeString)
    {
        return Enum.TryParse<CoreVarType>(varTypeString, true, out var result)
            ? result
            : CoreVarType.DWord;
    }

    private static S7.Net.VarType ConvertToS7VarType(CoreVarType varType)
    {
        return varType switch
        {
            CoreVarType.Byte => S7.Net.VarType.Byte,
            CoreVarType.Word => S7.Net.VarType.Word,
            CoreVarType.DWord => S7.Net.VarType.DWord,
            CoreVarType.Real => S7.Net.VarType.Real,
            CoreVarType.Bool => S7.Net.VarType.Bit,
            CoreVarType.Int => S7.Net.VarType.Int,
            CoreVarType.DInt => S7.Net.VarType.DInt,
            CoreVarType.String => S7.Net.VarType.String,
            CoreVarType.Timer => S7.Net.VarType.Timer,
            CoreVarType.Counter => S7.Net.VarType.Counter,
            CoreVarType.LReal => S7.Net.VarType.LReal,
            _ => S7.Net.VarType.DWord
        };
    }

    private static S7DataType ConvertToDataType(CoreDataType dataType)
    {
        return dataType switch
        {
            CoreDataType.Input => S7DataType.Input,
            CoreDataType.Output => S7DataType.Output,
            CoreDataType.Merker => S7DataType.Memory,
            CoreDataType.Timer => S7DataType.Timer,
            CoreDataType.Counter => S7DataType.Counter,
            _ => S7DataType.DataBlock
        };
    }

    #endregion
}
