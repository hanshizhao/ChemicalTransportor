# SiemensCommunicator 模块设计文档

**日期**: 2025-02-09
**版本**: 1.0
**作者**: Claude Code

## 1. 概述

SiemensCommunicator 是 ChemicalTransportor 项目中专门处理西门子 PLC 通讯的独立模块，遵循职责专一原则，仅负责 PLC 通讯相关功能。

### 1.1 设计原则

- **职责专一**：仅处理 PLC 通讯，不包含业务逻辑和展示功能
- **接口隔离**：通过服务接口暴露功能，与 S7.Net Plus 完全解耦
- **事件驱动**：错误和状态变化通过事件通知主应用
- **配置驱动**：所有配置来源于数据库，启动时加载

### 1.2 核心功能

| 功能 | 说明 |
|------|------|
| 连接管理 | 多 PLC 连接配置、连接/断开、状态监控 |
| 数据读写 | 支持_DB/Input/Output/Merker_ 等类型的数据读写 |
| 缓存刷新 | 定时自动刷新缓存数据 |
| 错误通知 | 连接失败、读写失败等错误主动通知 |
| 配置持久化 | 数据库存储连接配置和数据点配置 |

## 2. 整体架构

```
SiemensCommunicator.Core              # 领域层
├── Interfaces/                       # 服务接口定义
├── Models/                           # 领域模型、DTOs、枚举
└── Events/                           # 事件定义

SiemensCommunicator.Application       # 应用层
├── Services/                         # 服务实现
├── Background/                       # 后台服务
└── Dtos/                             # 跨模块 DTO

SiemensCommunicator.EntityFramework.Core  # 持久化层
└── SiemensCommunicatorDbContext      # 数据库上下文
```

### 2.2 模块边界

**包含**：
- ✅ PLC 连接管理
- ✅ 数据读写操作
- ✅ 数据缓存与定时刷新
- ✅ 错误事件通知
- ✅ 配置持久化

**不包含**：
- ❌ 业务逻辑处理
- ❌ 数据展示 API
- ❌ 主应用任何功能

## 3. 核心领域模型

### 3.1 PLC 连接配置

```csharp
public class PlcConnection
{
    public string Id { get; set; }              // PK
    public string Name { get; set; }
    public int CpuType { get; set; }            // S7-200/300/400/1200/1500
    public string Ip { get; set; }
    public short Rack { get; set; }
    public short Slot { get; set; }
    public int CacheRefreshInterval { get; set; } // 毫秒
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 3.2 PLC 数据点配置

```csharp
public class PlcDataPoint
{
    public string Id { get; set; }              // PK
    public string ConnectionId { get; set; }    // FK -> PlcConnections
    public string GroupId { get; set; }         // 分组 ID
    public string Name { get; set; }
    public string Description { get; set; }
    public int DataType { get; set; }           // DB/Input/Output/Merker
    public int? DbNumber { get; set; }
    public int StartByte { get; set; }
    public int? BitOffset { get; set; }
    public int VarType { get; set; }            // S7.Net Plus VarType
    public int DataCount { get; set; }          // 读取数量
    public bool EnableCache { get; set; }

    // 单位与换算
    public string Unit { get; set; }
    public string ConversionFormula { get; set; } // "x*10+100"
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }

    // 标签
    public string Tags { get; set; }            // JSON: ["温度","报警"]

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 3.3 分组与标签

```csharp
public class PlcGroup
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
}

public class PlcTag
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
}
```

### 3.4 错误事件

```csharp
public enum PlcErrorType
{
    ConnectionFailed,
    ConnectionLost,
    ReadFailed,
    WriteFailed,
    InvalidConfig,
    Timeout
}

public class PlcErrorEvent
{
    public string ConnectionId { get; set; }
    public PlcErrorType ErrorType { get; set; }
    public string Message { get; set; }
    public DateTime OccurredAt { get; set; }
    public Exception Exception { get; set; }
}
```

## 4. 服务接口设计

### 4.1 连接管理服务

```csharp
public interface IPlcConnectionService
{
    // 连接管理
    Task<bool> ConnectAsync(string connectionId);
    Task<bool> DisconnectAsync(string connectionId);
    Task<bool> DisconnectAllAsync();

    // 状态查询
    Task<PlcConnectionState> GetStateAsync(string connectionId);
    Task<Dictionary<string, PlcConnectionState>> GetAllStatesAsync();

    // 切换当前连接
    Task<bool> SetActiveConnectionAsync(string connectionId);
    Task<string> GetActiveConnectionAsync();

    // 事件
    event EventHandler<PlcStateChangedEventArgs> StateChanged;
    event EventHandler<PlcErrorEventArgs> ErrorOccurred;
}

public enum PlcConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}
```

### 4.2 数据读写服务

```csharp
public interface IPlcDataService
{
    // 读取单个数据点
    Task<object> ReadAsync(string dataPointId);
    Task<T> ReadAsync<T>(string dataPointId);

    // 批量读取
    Task<Dictionary<string, object>> ReadBatchAsync(params string[] dataPointIds);

    // 写入单个数据点
    Task<bool> WriteAsync(string dataPointId, object value);

    // 批量写入
    Task<Dictionary<string, bool>> WriteBatchAsync(Dictionary<string, object> values);

    // 原始读写
    Task<object> ReadRawAsync(string connectionId, DataType type, int dbNumber,
                              int startByte, VarType varType, int count = 1);
    Task<bool> WriteRawAsync(string connectionId, DataType type, int dbNumber,
                             int startByte, object value);

    // 获取缓存数据
    Task<object> GetCachedValueAsync(string dataPointId);
}
```

### 4.3 缓存管理服务

```csharp
public interface IPlcCacheManager
{
    // 获取/设置缓存值
    Task<T> GetAsync<T>(string dataPointId);
    Task SetAsync(string dataPointId, object value);

    // 批量操作
    Task<Dictionary<string, object>> GetBatchAsync(params string[] dataPointIds);

    // 刷新控制
    Task RefreshDataPointAsync(string dataPointId);
    Task RefreshConnectionAsync(string connectionId);
    Task RefreshAllAsync();

    // 缓存状态
    DateTime? GetLastUpdateTime(string dataPointId);
    bool IsExpired(string dataPointId);
}
```

### 4.4 配置仓储服务

```csharp
public interface IPlcConfigurationRepository
{
    // 连接配置
    Task<List<PlcConnection>> GetAllConnectionsAsync();
    Task<PlcConnection> GetConnectionAsync(string id);

    // 数据点配置
    Task<List<PlcDataPoint>> GetAllDataPointsAsync();
    Task<List<PlcDataPoint>> GetDataPointsByConnectionAsync(string connectionId);
    Task<List<PlcDataPoint>> GetDataPointsByGroupAsync(string groupId);
    Task<PlcDataPoint> GetDataPointAsync(string id);
    Task<PlcDataPoint> GetDataPointByNameAsync(string name);

    // 分组与标签
    Task<List<PlcGroup>> GetAllGroupsAsync();
    Task<List<PlcTag>> GetAllTagsAsync();
}
```

## 5. 缓存与刷新机制

### 5.1 刷新策略

1. 按连接分组刷新，避免单个 PLC 过载
2. 同一连接内的数据点批量读取
3. 仅刷新 `EnableCache = true` 的数据点
4. 刷新失败触发 `ErrorOccurred` 事件，不中断服务

### 5.2 后台刷新服务

```csharp
public class PlcCacheRefreshBackgroundService : BackgroundService
{
    // 根据每个连接的 CacheRefreshInterval 独立定时刷新
    // 连接断开时跳过刷新
    // 刷新成功后记录时间戳
}
```

## 6. 数据库设计

### 6.1 数据库配置

- **数据库文件**: `SiemensCommunicator.db`
- **数据库类型**: SQLite
- **连接字符串名称**: `SiemensCommunicator`

### 6.2 表结构

| 表名 | 说明 |
|------|------|
| PlcConnections | PLC 连接配置 |
| PlcDataPoints | 数据点配置 |
| PlcGroups | 分组定义 |
| PlcTags | 标签定义 |

### 6.3 数据库上下文

```csharp
[AppDbContext("SiemensCommunicator", DbProvider.Sqlite)]
public class SiemensCommunicatorDbContext : AppDbContext<SiemensCommunicatorDbContext>
{
    public DbSet<PlcConnection> PlcConnections { get; set; }
    public DbSet<PlcDataPoint> PlcDataPoints { get; set; }
    public DbSet<PlcGroup> PlcGroups { get; set; }
    public DbSet<PlcTag> PlcTags { get; set; }
}
```

## 7. 初始化流程

```
应用启动
    ↓
模块初始化 (ISiemensCommunicatorInitializer)
    ↓
执行数据库迁移
    ↓
加载 PlcConnection 配置
    ↓
加载 PlcDataPoint 配置
    ↓
启动后台缓存刷新服务
    ↓
自动连接 IsActive=true 的连接
```

## 8. NuGet 依赖

```xml
<PackageReference Include="S7netplus" Version="0.20.0" />
```

## 9. 目录结构

```
SiemensCommunicator.Core/
├── Interfaces/
│   ├── IPlcConnectionService.cs
│   ├── IPlcDataService.cs
│   ├── IPlcCacheManager.cs
│   ├── IPlcConfigurationRepository.cs
│   └── ISiemensCommunicatorInitializer.cs
├── Models/
│   ├── PlcConnection.cs
│   ├── PlcDataPoint.cs
│   ├── PlcGroup.cs
│   ├── PlcTag.cs
│   └── Enums/
│       ├── CpuType.cs
│       ├── DataType.cs
│       ├── VarType.cs
│       ├── PlcErrorType.cs
│       └── PlcConnectionState.cs
└── Events/
    ├── PlcStateChangedEventArgs.cs
    └── PlcErrorEventArgs.cs

SiemensCommunicator.Application/
├── Services/
│   ├── PlcConnectionService.cs
│   ├── PlcDataService.cs
│   ├── PlcCacheManager.cs
│   └── PlcConfigurationRepository.cs
├── Background/
│   └── PlcCacheRefreshBackgroundService.cs
├── SystemAppService.cs
└── Dtos/

SiemensCommunicator.EntityFramework.Core/
├── SiemensCommunicatorDbContext.cs
└── Migrations/
```

## 10. 测试策略

- **单元测试**: 使用 Moq 模拟 S7.Net Plus，测试服务逻辑
- **集成测试**: 可选 PLC 模拟器或真实 PLC 环境
- **不包含**: 业务逻辑测试（由主应用负责）

## 11. 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0 | 2025-02-09 | 初始设计 |
