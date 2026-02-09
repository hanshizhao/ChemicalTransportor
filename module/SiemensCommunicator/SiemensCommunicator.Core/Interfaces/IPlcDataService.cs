namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 数据读写服务接口
/// </summary>
public interface IPlcDataService
{
    #region 单点读取

    /// <summary>
    /// 读取指定数据点的值（返回对象类型）
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读取的值</returns>
    Task<object?> ReadAsync(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取指定数据点的值（返回强类型）
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读取的值</returns>
    Task<T?> ReadAsync<T>(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    #endregion

    #region 批量读取

    /// <summary>
    /// 批量读取多个数据点的值
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointNames">数据点名称集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据点名称与值的字典</returns>
    Task<Dictionary<string, object?>> ReadBatchAsync(
        long connectionId,
        IEnumerable<string> dataPointNames,
        CancellationToken cancellationToken = default);

    #endregion

    #region 写入

    /// <summary>
    /// 写入指定数据点的值
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="value">要写入的值</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>写入是否成功</returns>
    Task<bool> WriteAsync(
        long connectionId,
        string dataPointName,
        object value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量写入多个数据点的值
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPoints">数据点名称与值的字典</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>成功写入的数据点数量</returns>
    Task<int> WriteBatchAsync(
        long connectionId,
        Dictionary<string, object> dataPoints,
        CancellationToken cancellationToken = default);

    #endregion

    #region 原始读写

    /// <summary>
    /// 原始方式读取 PLC 数据（直接指定地址）
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dbNumber">数据块编号</param>
    /// <param name="startByte">起始字节</param>
    /// <param name="varType">变量类型</param>
    /// <param name="dataCount">数据数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读取的值</returns>
    Task<object?> ReadRawAsync(
        long connectionId,
        int dbNumber,
        int startByte,
        VarType varType,
        int dataCount = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 原始方式写入 PLC 数据（直接指定地址）
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dbNumber">数据块编号</param>
    /// <param name="startByte">起始字节</param>
    /// <param name="varType">变量类型</param>
    /// <param name="value">要写入的值</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>写入是否成功</returns>
    Task<bool> WriteRawAsync(
        long connectionId,
        int dbNumber,
        int startByte,
        VarType varType,
        object value,
        CancellationToken cancellationToken = default);

    #endregion

    #region 缓存读取

    /// <summary>
    /// 从缓存中获取数据点的值
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="connectionId">连接ID</param>
    /// <param name="dataPointName">数据点名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存中的值，如果不存在或已过期则返回 null</returns>
    Task<T?> GetCachedValueAsync<T>(long connectionId, string dataPointName, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// PLC 变量类型枚举
/// </summary>
public enum VarType
{
    /// <summary>
    /// 字节（8位）
    /// </summary>
    Byte = 0,

    /// <summary>
    /// 字（16位）
    /// </summary>
    Word = 1,

    /// <summary>
    /// 双字（32位）
    /// </summary>
    DWord = 2,

    /// <summary>
    /// 浮点数（32位）
    /// </summary>
    Real = 3,

    /// <summary>
    /// 布尔值（1位）
    /// </summary>
    Bool = 4,

    /// <summary>
    /// 整数（16位）
    /// </summary>
    Int = 5,

    /// <summary>
    /// 双整数（32位）
    /// </summary>
    DInt = 6,

    /// <summary>
    /// 字符串
    /// </summary>
    String = 7,

    /// <summary>
    /// 定时器
    /// </summary>
    Timer = 8,

    /// <summary>
    /// 计数器
    /// </summary>
    Counter = 9,

    /// <summary>
    /// 浮点数双精度（64位）
    /// </summary>
    LReal = 10,

    /// <summary>
    /// 字节数组
    /// </summary>
    ByteArray = 11
}
