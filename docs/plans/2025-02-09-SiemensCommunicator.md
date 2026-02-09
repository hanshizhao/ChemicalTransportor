# SiemensCommunicator 模块实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 实现一个专责西门子 PLC 通讯的 Furion 多模块，支持多连接管理、数据读写、定时缓存和配置持久化。

**架构:** Clean Architecture 分层，Core 层定义接口和模型，Application 层实现服务，EntityFramework.Core 层处理 SQLite 持久化。通过服务接口暴露功能，使用事件通知错误状态。

**Tech Stack:** .NET 10, Furion 4.9.8.15, Entity Framework Core, S7.Net Plus 0.20.0, SQLite

---

## Task 1: 添加 S7.Net Plus NuGet 包

**Files:**
- Modify: `module/SiemensCommunicator/SiememensCommunicator.Application/SiemensCommunicator.Application.csproj`

**Step 1: 添加 NuGet 包引用**

```xml
<ItemGroup>
  <PackageReference Include="S7netplus" Version="0.20.0" />
</ItemGroup>
```

**Step 2: 恢复包验证**

```bash
dotnet restore module/SiemensCommunicator/SiemensCommunicator.Application/SiemensCommunicator.Application.csproj
```

Expected: 成功恢复 S7.Net Plus 包

**Step 3: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/SiemensCommunicator.Application.csproj
git commit -m "deps(siemens): add S7.Net Plus 0.20.0"
```

---

## Task 2: 创建枚举定义

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/Enums/CpuType.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/Enums/DataType.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/Enums/PlcConnectionState.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/Enums/PlcErrorType.cs`

**Step 1: 创建 CpuType 枚举**

```csharp
namespace SiemensCommunicator.Core.Models.Enums;

/// <summary>
/// 西门子 PLC CPU 类型
/// </summary>
public enum CpuType
{
    /// <summary>
    /// S7-200
    /// </summary>
    S7200 = 0,

    /// <summary>
    /// S7-300
    /// </summary>
    S7300 = 10,

    /// <summary>
    /// S7-400
    /// </summary>
    S7400 = 20,

    /// <summary>
    /// S7-1200
    /// </summary>
    S71200 = 30,

    /// <summary>
    /// S7-1500
    /// </summary>
    S71500 = 40
}
```

**Step 2: 创建 DataType 枚举**

```csharp
namespace SiemensCommunicator.Core.Models.Enums;

/// <summary>
/// PLC 数据类型
/// </summary>
public enum DataType
{
    /// <summary>
    /// 数据块
    /// </summary>
    DB = 0,

    /// <summary>
    /// 输入
    /// </summary>
    Input = 1,

    /// <summary>
    /// 输出
    /// </summary>
    Output = 2,

    /// <summary>
    /// 标志位/内存
    /// </summary>
    Merker = 3,

    /// <summary>
    /// 定时器
    /// </summary>
    Timer = 4,

    /// <summary>
    /// 计数器
    /// </summary>
    Counter = 5
}
```

**Step 3: 创建 PlcConnectionState 枚举**

```csharp
namespace SiemensCommunicator.Core.Models.Enums;

/// <summary>
/// PLC 连接状态
/// </summary>
public enum PlcConnectionState
{
    /// <summary>
    /// 未连接
    /// </summary>
    Disconnected = 0,

    /// <summary>
    /// 连接中
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// 已连接
    /// </summary>
    Connected = 2,

    /// <summary>
    /// 错误
    /// </summary>
    Error = 3
}
```

**Step 4: 创建 PlcErrorType 枚举**

```csharp
namespace SiemensCommunicator.Core.Models.Enums;

/// <summary>
/// PLC 错误类型
/// </summary>
public enum PlcErrorType
{
    /// <summary>
    /// 连接失败
    /// </summary>
    ConnectionFailed = 0,

    /// <summary>
    /// 连接中断
    /// </summary>
    ConnectionLost = 1,

    /// <summary>
    /// 读取失败
    /// </summary>
    ReadFailed = 2,

    /// <summary>
    /// 写入失败
    /// </summary>
    WriteFailed = 3,

    /// <summary>
    /// 配置错误
    /// </summary>
    InvalidConfig = 4,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 5
}
```

**Step 5: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Models/Enums/
git commit -m "feat(siemens): add enum definitions for PLC types and states"
```

---

## Task 3: 创建事件参数类

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Events/PlcStateChangedEventArgs.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Events/PlcErrorEventArgs.cs`

**Step 1: 创建状态变化事件参数**

```csharp
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Events;

/// <summary>
/// PLC 连接状态变化事件参数
/// </summary>
public class PlcStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 旧状态
    /// </summary>
    public PlcConnectionState OldState { get; set; }

    /// <summary>
    /// 新状态
    /// </summary>
    public PlcConnectionState NewState { get; set; }

    /// <summary>
    /// 变化时间
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
```

**Step 2: 创建错误事件参数**

```csharp
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Events;

/// <summary>
/// PLC 错误事件参数
/// </summary>
public class PlcErrorEventArgs : EventArgs
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 错误类型
    /// </summary>
    public PlcErrorType ErrorType { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 异常对象
    /// </summary>
    public Exception? Exception { get; set; }
}
```

**Step 3: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Events/
git commit -m "feat(siemens): add event args for state changes and errors"
```

---

## Task 4: 创建数据库实体模型

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/PlcConnection.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/PlcDataPoint.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/PlcGroup.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Models/PlcTag.cs`

**Step 1: 创建 PlcConnection 实体**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 连接配置实体
/// </summary>
[Table("PlcConnections")]
public class PlcConnection
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 连接名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// CPU 类型
    /// </summary>
    public int CpuType { get; set; }

    /// <summary>
    /// IP 地址
    /// </summary>
    [MaxLength(50)]
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// 机架号
    /// </summary>
    public short Rack { get; set; }

    /// <summary>
    /// 插槽号
    /// </summary>
    public short Slot { get; set; }

    /// <summary>
    /// 缓存刷新间隔（毫秒）
    /// </summary>
    public int CacheRefreshInterval { get; set; } = 1000;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 导航属性 - 数据点集合
    /// </summary>
    public ICollection<PlcDataPoint> DataPoints { get; set; } = new List<PlcDataPoint>();
}
```

**Step 2: 创建 PlcDataPoint 实体**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 数据点配置实体
/// </summary>
[Table("PlcDataPoints")]
public class PlcDataPoint
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 所属连接 ID
    /// </summary>
    [MaxLength(50)]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 分组 ID
    /// </summary>
    [MaxLength(50)]
    public string? GroupId { get; set; }

    /// <summary>
    /// 数据点名称
    /// </summary>
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 数据类型 (DB/Input/Output/Merker等)
    /// </summary>
    public int DataType { get; set; }

    /// <summary>
    /// DB 块号 (DataType 为 DB 时使用)
    /// </summary>
    public int? DbNumber { get; set; }

    /// <summary>
    /// 起始字节
    /// </summary>
    public int StartByte { get; set; }

    /// <summary>
    /// 位偏移 (bool 类型时使用)
    /// </summary>
    public int? BitOffset { get; set; }

    /// <summary>
    /// 变量类型 (对应 S7.Net Plus 的 VarType)
    /// </summary>
    public int VarType { get; set; }

    /// <summary>
    /// 数据数量
    /// </summary>
    public int DataCount { get; set; } = 1;

    /// <summary>
    /// 是否启用缓存
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// 单位
    /// </summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// 换算公式 (如 "x*10+100")
    /// </summary>
    [MaxLength(200)]
    public string? ConversionFormula { get; set; }

    /// <summary>
    /// 最小值 (校验用)
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// 最大值 (校验用)
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// 标签 (JSON 格式存储)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 导航属性 - 所属连接
    /// </summary>
    [ForeignKey("ConnectionId")]
    public PlcConnection? Connection { get; set; }
}
```

**Step 3: 创建 PlcGroup 实体**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 数据点分组
/// </summary>
[Table("PlcGroups")]
public class PlcGroup
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 分组名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 导航属性 - 数据点集合
    /// </summary>
    public ICollection<PlcDataPoint> DataPoints { get; set; } = new List<PlcDataPoint>();
}
```

**Step 4: 创建 PlcTag 实体**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiemensCommunicator.Core.Models;

/// <summary>
/// PLC 数据点标签
/// </summary>
[Table("PlcTags")]
public class PlcTag
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称（唯一）
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 显示颜色
    /// </summary>
    [MaxLength(20)]
    public string? Color { get; set; }
}
```

**Step 5: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Models/
git commit -m "feat(siemens): add database entity models"
```

---

## Task 5: 创建数据库上下文

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/SiemensCommunicatorDbContext.cs`
- Modify: `module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/SiemensCommunicator.EntityFramework.Core.csproj`

**Step 1: 添加 EF Core 包引用**

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
</ItemGroup>
```

**Step 2: 创建数据库上下文**

```csharp
using Furion.DependencyInjection;
using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;
using SiemensCommunicator.Core.Models;

namespace SiemensCommunicator.EntityFramework.Core;

/// <summary>
/// SiemensCommunicator 模块数据库上下文
/// </summary>
[AppDbContext("SiemensCommunicator", DbProvider.Sqlite)]
public class SiemensCommunicatorDbContext : AppDbContext<SiemensCommunicatorDbContext>
{
    /// <summary>
    /// PLC 连接配置
    /// </summary>
    public DbSet<PlcConnection> PlcConnections { get; set; } = null!;

    /// <summary>
    /// PLC 数据点配置
    /// </summary>
    public DbSet<PlcDataPoint> PlcDataPoints { get; set; } = null!;

    /// <summary>
    /// PLC 分组
    /// </summary>
    public DbSet<PlcGroup> PlcGroups { get; set; } = null!;

    /// <summary>
    /// PLC 标签
    /// </summary>
    public DbSet<PlcTag> PlcTags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PlcConnection 配置
        modelBuilder.Entity<PlcConnection>(entity =>
        {
            entity.HasIndex(e => e.Ip).HasDatabaseName("IX_PlcConnections_Ip");
            entity.Property(e => e.CpuType).HasDefaultValue(40); // S71500
            entity.Property(e => e.CacheRefreshInterval).HasDefaultValue(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // PlcDataPoint 配置
        modelBuilder.Entity<PlcDataPoint>(entity =>
        {
            entity.HasIndex(e => e.ConnectionId).HasDatabaseName("IX_PlcDataPoints_ConnectionId");
            entity.HasIndex(e => e.GroupId).HasDatabaseName("IX_PlcDataPoints_GroupId");
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_PlcDataPoints_Name");

            entity.Property(e => e.DataType).HasDefaultValue(0); // DB
            entity.Property(e => e.DataCount).HasDefaultValue(1);
            entity.Property(e => e.EnableCache).HasDefaultValue(true);

            entity.HasOne(e => e.Connection)
                .WithMany(c => c.DataPoints)
                .HasForeignKey(e => e.ConnectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlcGroup 配置
        modelBuilder.Entity<PlcGroup>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_PlcGroups_Name");
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });

        // PlcTag 配置
        modelBuilder.Entity<PlcTag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_PlcTags_Name");
        });
    }
}
```

**Step 3: 更新 GlobalUsings.cs**

在 `module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/GlobalUsings.cs` 添加：

```csharp
global using Furion.DependencyInjection;
global using Furion.DatabaseAccessor;
global using Microsoft.EntityFrameworkCore;
global using SiemensCommunicator.Core.Models;
```

**Step 4: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/
git commit -m "feat(siemens): add database context and configuration"
```

---

## Task 6: 创建配置仓储接口

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcConfigurationRepository.cs`

**Step 1: 创建配置仓储接口**

```csharp
using SiemensCommunicator.Core.Models;

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
    Task<List<PlcConnection>> GetAllConnectionsAsync();

    /// <summary>
    /// 根据 ID 获取连接配置
    /// </summary>
    Task<PlcConnection?> GetConnectionAsync(string id);

    #endregion

    #region 数据点配置

    /// <summary>
    /// 获取所有数据点配置
    /// </summary>
    Task<List<PlcDataPoint>> GetAllDataPointsAsync();

    /// <summary>
    /// 根据连接 ID 获取数据点
    /// </summary>
    Task<List<PlcDataPoint>> GetDataPointsByConnectionAsync(string connectionId);

    /// <summary>
    /// 根据分组 ID 获取数据点
    /// </summary>
    Task<List<PlcDataPoint>> GetDataPointsByGroupAsync(string groupId);

    /// <summary>
    /// 根据 ID 获取数据点
    /// </summary>
    Task<PlcDataPoint?> GetDataPointAsync(string id);

    /// <summary>
    /// 根据名称获取数据点
    /// </summary>
    Task<PlcDataPoint?> GetDataPointByNameAsync(string name);

    #endregion

    #region 分组与标签

    /// <summary>
    /// 获取所有分组
    /// </summary>
    Task<List<PlcGroup>> GetAllGroupsAsync();

    /// <summary>
    /// 获取所有标签
    /// </summary>
    Task<List<PlcTag>> GetAllTagsAsync();

    #endregion
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcConfigurationRepository.cs
git commit -m "feat(siemens): add configuration repository interface"
```

---

## Task 7: 创建配置仓储实现

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcConfigurationRepository.cs`

**Step 1: 创建配置仓储实现**

```csharp
using Microsoft.EntityFrameworkCore;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.EntityFramework.Core;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// PLC 配置仓储实现
/// </summary>
public class PlcConfigurationRepository : IPlcConfigurationRepository, ITransient
{
    private readonly SiemensCommunicatorDbContext _dbContext;

    public PlcConfigurationRepository(SiemensCommunicatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region 连接配置

    public async Task<List<PlcConnection>> GetAllConnectionsAsync()
    {
        return await _dbContext.PlcConnections.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<PlcConnection?> GetConnectionAsync(string id)
    {
        return await _dbContext.PlcConnections.FindAsync(id);
    }

    #endregion

    #region 数据点配置

    public async Task<List<PlcDataPoint>> GetAllDataPointsAsync()
    {
        return await _dbContext.PlcDataPoints
            .Include(d => d.Connection)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<List<PlcDataPoint>> GetDataPointsByConnectionAsync(string connectionId)
    {
        return await _dbContext.PlcDataPoints
            .Where(d => d.ConnectionId == connectionId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<List<PlcDataPoint>> GetDataPointsByGroupAsync(string groupId)
    {
        return await _dbContext.PlcDataPoints
            .Where(d => d.GroupId == groupId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<PlcDataPoint?> GetDataPointAsync(string id)
    {
        return await _dbContext.PlcDataPoints.FindAsync(id);
    }

    public async Task<PlcDataPoint?> GetDataPointByNameAsync(string name)
    {
        return await _dbContext.PlcDataPoints
            .FirstOrDefaultAsync(d => d.Name == name);
    }

    #endregion

    #region 分组与标签

    public async Task<List<PlcGroup>> GetAllGroupsAsync()
    {
        return await _dbContext.PlcGroups.OrderBy(g => g.SortOrder).ToListAsync();
    }

    public async Task<List<PlcTag>> GetAllTagsAsync()
    {
        return await _dbContext.PlcTags.OrderBy(t => t.Name).ToListAsync();
    }

    #endregion
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcConfigurationRepository.cs
git commit -m "feat(siemens): implement configuration repository"
```

---

## Task 8: 创建 PLC 连接服务接口

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcConnectionService.cs`

**Step 1: 创建连接服务接口**

```csharp
using SiemensCommunicator.Core.Events;
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 连接管理服务接口
/// </summary>
public interface IPlcConnectionService
{
    #region 连接管理

    /// <summary>
    /// 连接到指定 PLC
    /// </summary>
    Task<bool> ConnectAsync(string connectionId);

    /// <summary>
    /// 断开指定 PLC 连接
    /// </summary>
    Task<bool> DisconnectAsync(string connectionId);

    /// <summary>
    /// 断开所有 PLC 连接
    /// </summary>
    Task<bool> DisconnectAllAsync();

    #endregion

    #region 状态查询

    /// <summary>
    /// 获取连接状态
    /// </summary>
    Task<PlcConnectionState> GetStateAsync(string connectionId);

    /// <summary>
    /// 获取所有连接状态
    /// </summary>
    Task<Dictionary<string, PlcConnectionState>> GetAllStatesAsync();

    #endregion

    #region 当前连接切换

    /// <summary>
    /// 设置当前活动连接
    /// </summary>
    Task<bool> SetActiveConnectionAsync(string connectionId);

    /// <summary>
    /// 获取当前活动连接 ID
    /// </summary>
    Task<string> GetActiveConnectionAsync();

    #endregion

    #region 事件

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    event EventHandler<PlcStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// 错误发生事件
    /// </summary>
    event EventHandler<PlcErrorEventArgs>? ErrorOccurred;

    #endregion
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcConnectionService.cs
git commit -m "feat(siemens): add connection service interface"
```

---

## Task 9: 创建 PLC 数据服务接口

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcDataService.cs`

**Step 1: 创建数据服务接口**

```csharp
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 数据读写服务接口
/// </summary>
public interface IPlcDataService
{
    #region 读取单个数据点

    /// <summary>
    /// 读取单个数据点（返回 object）
    /// </summary>
    Task<object?> ReadAsync(string dataPointId);

    /// <summary>
    /// 读取单个数据点（返回指定类型）
    /// </summary>
    Task<T?> ReadAsync<T>(string dataPointId);

    #endregion

    #region 批量读取

    /// <summary>
    /// 批量读取数据点
    /// </summary>
    Task<Dictionary<string, object?>> ReadBatchAsync(params string[] dataPointIds);

    #endregion

    #region 写入

    /// <summary>
    /// 写入单个数据点
    /// </summary>
    Task<bool> WriteAsync(string dataPointId, object value);

    /// <summary>
    /// 批量写入数据点
    /// </summary>
    Task<Dictionary<string, bool>> WriteBatchAsync(Dictionary<string, object?> values);

    #endregion

    #region 原始读写

    /// <summary>
    /// 原始地址读取
    /// </summary>
    Task<object?> ReadRawAsync(string connectionId, DataType dataType, int dbNumber,
        int startByte, VarType varType, int count = 1);

    /// <summary>
    /// 原始地址写入
    /// </summary>
    Task<bool> WriteRawAsync(string connectionId, DataType dataType, int dbNumber,
        int startByte, object value);

    #endregion

    #region 缓存数据

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    Task<object?> GetCachedValueAsync(string dataPointId);

    #endregion
}

/// <summary>
/// 变量类型枚举（对应 S7.Net Plus）
/// </summary>
public enum VarType
{
    Byte,
    Word,
    DWord,
    LWord,
    S5Time,
    Date,
    TimeOfDay,
    Real,
    LReal,
    String,
    S7String,
    Bool,
    Int,
    DInt,
    LInt,
    USInt,
    UInt,
    UDInt,
    ULInt,
    Bcd16,
    Bcd32,
    Counter,
    Timer
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcDataService.cs
git commit -m "feat(siemens): add data service interface"
```

---

## Task 10: 创建缓存管理器接口

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcCacheManager.cs`

**Step 1: 创建缓存管理器接口**

```csharp
namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// PLC 缓存管理器接口
/// </summary>
public interface IPlcCacheManager
{
    #region 获取/设置缓存

    /// <summary>
    /// 获取缓存值
    /// </summary>
    Task<T?> GetAsync<T>(string dataPointId);

    /// <summary>
    /// 设置缓存值
    /// </summary>
    Task SetAsync(string dataPointId, object? value);

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量获取缓存值
    /// </summary>
    Task<Dictionary<string, object?>> GetBatchAsync(params string[] dataPointIds);

    #endregion

    #region 刷新控制

    /// <summary>
    /// 刷新单个数据点
    /// </summary>
    Task RefreshDataPointAsync(string dataPointId);

    /// <summary>
    /// 刷新指定连接的所有缓存数据
    /// </summary>
    Task RefreshConnectionAsync(string connectionId);

    /// <summary>
    /// 刷新所有缓存数据
    /// </summary>
    Task RefreshAllAsync();

    #endregion

    #region 缓存状态

    /// <summary>
    /// 获取最后更新时间
    /// </summary>
    DateTime? GetLastUpdateTime(string dataPointId);

    /// <summary>
    /// 检查缓存是否过期
    /// </summary>
    bool IsExpired(string dataPointId, int refreshInterval);

    #endregion
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/IPlcCacheManager.cs
git commit -m "feat(siemens): add cache manager interface"
```

---

## Task 11: 创建缓存管理器实现

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcCacheManager.cs`

**Step 1: 创建缓存管理器实现**

```csharp
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
    private readonly IPlcConfigurationRepository _configRepository;
    private readonly IPlcDataService _dataService;

    // 缓存最后更新时间存储
    private readonly Dictionary<string, DateTime> _updateTimes = new();

    public PlcCacheManager(
        IMemoryCache cache,
        IPlcConfigurationRepository configRepository,
        IPlcDataService dataService)
    {
        _cache = cache;
        _configRepository = configRepository;
        _dataService = dataService;
    }

    public async Task<T?> GetAsync<T>(string dataPointId)
    {
        var cacheKey = GetCacheKey(dataPointId);
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            var dataPoint = await _configRepository.GetDataPointAsync(dataPointId);
            if (dataPoint == null) return default;

            if (dataPoint.EnableCache)
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMilliseconds(dataPoint.Connection?.CacheRefreshInterval ?? 1000));
            }

            var value = await _dataService.ReadAsync(dataPointId);
            _updateTimes[dataPointId] = DateTime.Now;
            return value;
        });
    }

    public Task SetAsync(string dataPointId, object? value)
    {
        var cacheKey = GetCacheKey(dataPointId);
        _cache.Set(cacheKey, value);
        _updateTimes[dataPointId] = DateTime.Now;
        return Task.CompletedTask;
    }

    public async Task<Dictionary<string, object?>> GetBatchAsync(params string[] dataPointIds)
    {
        var result = new Dictionary<string, object?>();

        foreach (var id in dataPointIds)
        {
            result[id] = await GetAsync<object>(id);
        }

        return result;
    }

    public async Task RefreshDataPointAsync(string dataPointId)
    {
        var dataPoint = await _configRepository.GetDataPointAsync(dataPointId);
        if (dataPoint == null) return;

        var value = await _dataService.ReadAsync(dataPointId);
        await SetAsync(dataPointId, value);
    }

    public async Task RefreshConnectionAsync(string connectionId)
    {
        var dataPoints = await _configRepository.GetDataPointsByConnectionAsync(connectionId);
        var cacheablePoints = dataPoints.Where(dp => dp.EnableCache).ToList();

        foreach (var dataPoint in cacheablePoints)
        {
            await RefreshDataPointAsync(dataPoint.Id);
        }
    }

    public async Task RefreshAllAsync()
    {
        var allConnections = await _configRepository.GetAllConnectionsAsync();

        foreach (var connection in allConnections)
        {
            await RefreshConnectionAsync(connection.Id);
        }
    }

    public DateTime? GetLastUpdateTime(string dataPointId)
    {
        return _updateTimes.TryGetValue(dataPointId, out var time) ? time : null;
    }

    public bool IsExpired(string dataPointId, int refreshInterval)
    {
        if (!_updateTimes.TryGetValue(dataPointId, out var updateTime))
            return true;

        return (DateTime.Now - updateTime).TotalMilliseconds > refreshInterval;
    }

    private static string GetCacheKey(string dataPointId)
    {
        return $"PLC_DataPoint_{dataPointId}";
    }
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcCacheManager.cs
git commit -m "feat(siemens): implement cache manager"
```

---

## Task 12: 创建 PLC 连接服务实现

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcConnectionService.cs`

**Step 1: 创建连接服务实现**

```csharp
using S7.Net;
using SiemensCommunicator.Core.Events;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// PLC 连接管理服务实现
/// </summary>
public class PlcConnectionService : IPlcConnectionService, ITransient
{
    private readonly IPlcConfigurationRepository _configRepository;
    private readonly Dictionary<string, Plc> _connections = new();
    private readonly Dictionary<string, PlcConnectionState> _states = new();
    private string? _activeConnectionId;

    public PlcConnectionService(IPlcConfigurationRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public event EventHandler<PlcStateChangedEventArgs>? StateChanged;
    public event EventHandler<PlcErrorEventArgs>? ErrorOccurred;

    public async Task<bool> ConnectAsync(string connectionId)
    {
        var config = await _configRepository.GetConnectionAsync(connectionId);
        if (config == null)
        {
            RaiseError(connectionId, PlcErrorType.InvalidConfig, "连接配置不存在");
            return false;
        }

        if (_connections.ContainsKey(connectionId))
        {
            var existingState = _states[connectionId];
            if (existingState == PlcConnectionState.Connected)
                return true;
        }

        SetState(connectionId, PlcConnectionState.Connecting);

        try
        {
            var plc = new Plc(
                (CpuType)config.CpuType,
                config.Ip,
                config.Rack,
                config.Slot
            );

            await plc.OpenAsync();

            if (!plc.IsConnected)
            {
                SetState(connectionId, PlcConnectionState.Error);
                RaiseError(connectionId, PlcErrorType.ConnectionFailed, "PLC 连接失败");
                return false;
            }

            _connections[connectionId] = plc;
            SetState(connectionId, PlcConnectionState.Connected);
            return true;
        }
        catch (Exception ex)
        {
            SetState(connectionId, PlcConnectionState.Error);
            RaiseError(connectionId, PlcErrorType.ConnectionFailed, $"连接异常: {ex.Message}", ex);
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(string connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var plc))
        {
            return true;
        }

        try
        {
            await plc.CloseAsync();
            _connections.Remove(connectionId);
            _states.Remove(connectionId);

            if (_activeConnectionId == connectionId)
            {
                _activeConnectionId = null;
            }

            return true;
        }
        catch (Exception ex)
        {
            RaiseError(connectionId, PlcErrorType.ConnectionLost, $"断开连接异常: {ex.Message}", ex);
            return false;
        }
    }

    public async Task<bool> DisconnectAllAsync()
    {
        var connectionIds = _connections.Keys.ToList();
        var tasks = connectionIds.Select(DisconnectAsync);
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public Task<PlcConnectionState> GetStateAsync(string connectionId)
    {
        _states.TryGetValue(connectionId, out var state);
        return Task.FromResult(state);
    }

    public Task<Dictionary<string, PlcConnectionState>> GetAllStatesAsync()
    {
        return Task.FromResult(new Dictionary<string, PlcConnectionState>(_states));
    }

    public Task<bool> SetActiveConnectionAsync(string connectionId)
    {
        if (!_connections.ContainsKey(connectionId))
        {
            return Task.FromResult(false);
        }

        _activeConnectionId = connectionId;
        return Task.FromResult(true);
    }

    public Task<string> GetActiveConnectionAsync()
    {
        return Task.FromResult(_activeConnectionId ?? string.Empty);
    }

    private void SetState(string connectionId, PlcConnectionState newState)
    {
        var oldState = _states.GetValueOrDefault(connectionId);
        if (oldState != newState)
        {
            _states[connectionId] = newState;
            StateChanged?.Invoke(this, new PlcStateChangedEventArgs
            {
                ConnectionId = connectionId,
                OldState = oldState,
                NewState = newState
            });
        }
    }

    private void RaiseError(string connectionId, PlcErrorType errorType, string message, Exception? exception = null)
    {
        ErrorOccurred?.Invoke(this, new PlcErrorEventArgs
        {
            ConnectionId = connectionId,
            ErrorType = errorType,
            Message = message,
            Exception = exception
        });
    }

    // 内部方法：获取 Plc 实例供数据服务使用
    internal Plc? GetPlc(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var plc) ? plc : null;
    }
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcConnectionService.cs
git commit -m "feat(siemens): implement connection service"
```

---

## Task 13: 创建 PLC 数据服务实现

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcDataService.cs`

**Step 1: 创建数据服务实现**

```csharp
using S7.Net;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.Core.Models.Enums;
using System.DataTypes;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// PLC 数据读写服务实现
/// </summary>
public class PlcDataService : IPlcDataService, ITransient
{
    private readonly IPlcConnectionService _connectionService;
    private readonly IPlcConfigurationRepository _configRepository;
    private readonly IPlcCacheManager _cacheManager;

    public PlcDataService(
        IPlcConnectionService connectionService,
        IPlcConfigurationRepository configRepository,
        IPlcCacheManager cacheManager)
    {
        _connectionService = connectionService;
        _configRepository = configRepository;
        _cacheManager = cacheManager;
    }

    public async Task<object?> ReadAsync(string dataPointId)
    {
        var dataPoint = await _configRepository.GetDataPointAsync(dataPointId);
        if (dataPoint == null) return null;

        return await ReadRawAsync(
            dataPoint.ConnectionId,
            (DataType)dataPoint.DataType,
            dataPoint.DbNumber ?? 0,
            dataPoint.StartByte,
            (VarType)dataPoint.VarType,
            dataPoint.DataCount
        );
    }

    public async Task<T?> ReadAsync<T>(string dataPointId)
    {
        var value = await ReadAsync(dataPointId);
        return value == null ? default : (T)value;
    }

    public async Task<Dictionary<string, object?>> ReadBatchAsync(params string[] dataPointIds)
    {
        var result = new Dictionary<string, object?>();

        foreach (var id in dataPointIds)
        {
            result[id] = await ReadAsync(id);
        }

        return result;
    }

    public async Task<bool> WriteAsync(string dataPointId, object value)
    {
        var dataPoint = await _configRepository.GetDataPointAsync(dataPointId);
        if (dataPoint == null) return false;

        return await WriteRawAsync(
            dataPoint.ConnectionId,
            (DataType)dataPoint.DataType,
            dataPoint.DbNumber ?? 0,
            dataPoint.StartByte,
            value
        );
    }

    public async Task<Dictionary<string, bool>> WriteBatchAsync(Dictionary<string, object?> values)
    {
        var result = new Dictionary<string, bool>();

        foreach (var kvp in values)
        {
            result[kvp.Key] = await WriteAsync(kvp.Key, kvp.Value!);
        }

        return result;
    }

    public async Task<object?> ReadRawAsync(string connectionId, DataType dataType, int dbNumber,
        int startByte, VarType varType, int count = 1)
    {
        var plc = GetPlcFromConnectionService(connectionId);
        if (plc == null || !plc.IsConnected)
        {
            return null;
        }

        try
        {
            var s7DataType = ConvertToS7DataType(dataType);
            object result;

            if (count == 1)
            {
                result = await plc.ReadAsync(s7DataType, dbNumber, startByte, ConvertToS7VarType(varType));
            }
            else
            {
                result = await plc.ReadBytesAsync(s7DataType, dbNumber, startByte, count);
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> WriteRawAsync(string connectionId, DataType dataType, int dbNumber,
        int startByte, object value)
    {
        var plc = GetPlcFromConnectionService(connectionId);
        if (plc == null || !plc.IsConnected)
        {
            return false;
        }

        try
        {
            var s7DataType = ConvertToS7DataType(dataType);
            await plc.WriteAsync(s7DataType, dbNumber, startByte, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<object?> GetCachedValueAsync(string dataPointId)
    {
        return await _cacheManager.GetAsync<object>(dataPointId);
    }

    private Plc? GetPlcFromConnectionService(string connectionId)
    {
        // 通过反射或其他方式获取 ConnectionService 中的 Plc 实例
        // 这里简化处理，实际可以改进 ConnectionService 接口
        if (_connectionService is PlcConnectionService connService)
        {
            return connService.GetPlc(connectionId);
        }
        return null;
    }

    private static S7.Net.DataType ConvertToS7DataType(DataType dataType)
    {
        return dataType switch
        {
            DataType.DB => S7.Net.DataType.DataBlock,
            DataType.Input => S7.Net.DataType.Input,
            DataType.Output => S7.Net.DataType.Output,
            DataType.Merker => S7.Net.DataType.Memory,
            DataType.Timer => S7.Net.DataType.Timer,
            DataType.Counter => S7.Net.DataType.Counter,
            _ => S7.Net.DataType.DataBlock
        };
    }

    private static S7.Net.VarType ConvertToS7VarType(VarType varType)
    {
        return varType switch
        {
            VarType.Byte => S7.Net.VarType.Byte,
            VarType.Word => S7.Net.VarType.Word,
            VarType.DWord => S7.Net.VarType.DWord,
            VarType.Real => S7.Net.VarType.Real,
            VarType.Bool => S7.Net.VarType.Bit,
            VarType.Int => S7.Net.VarType.Int,
            VarType.DInt => S7.Net.VarType.DInt,
            VarType.String => S7.Net.VarType.String,
            _ => S7.Net.VarType.Byte
        };
    }
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/Services/PlcDataService.cs
git commit -m "feat(siemens): implement data service"
```

---

## Task 14: 创建后台缓存刷新服务

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Application/Background/PlcCacheRefreshBackgroundService.cs`

**Step 1: 创建后台刷新服务**

```csharp
using Microsoft.Extensions.Hosting;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;

namespace SiemensCommunicator.Application.Background;

/// <summary>
/// PLC 缓存定时刷新后台服务
/// </summary>
public class PlcCacheRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Timer> _timers = new();

    public PlcCacheRefreshBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 启动时初始化所有连接的定时器
        await InitializeTimersAsync(stoppingToken);

        // 等待停止信号
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeTimersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var configRepository = scope.ServiceProvider.GetRequiredService<IPlcConfigurationRepository>();

        var connections = await configRepository.GetAllConnectionsAsync();

        foreach (var connection in connections.Where(c => c.IsActive))
        {
            var timer = new Timer(
                async _ => await RefreshConnectionCacheAsync(connection.Id),
                null,
                TimeSpan.FromMilliseconds(connection.CacheRefreshInterval),
                TimeSpan.FromMilliseconds(connection.CacheRefreshInterval)
            );

            _timers[connection.Id] = timer;
        }
    }

    private async Task RefreshConnectionCacheAsync(string connectionId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheManager = scope.ServiceProvider.GetRequiredService<IPlcCacheManager>();
            var connectionService = scope.ServiceProvider.GetRequiredService<IPlcConnectionService>();

            // 检查连接状态
            var state = await connectionService.GetStateAsync(connectionId);
            if (state != Core.Models.Enums.PlcConnectionState.Connected)
            {
                return;
            }

            await cacheManager.RefreshConnectionAsync(connectionId);
        }
        catch
        {
            // 静默处理刷新错误，错误由各服务内部触发事件
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // 清理所有定时器
        foreach (var timer in _timers.Values)
        {
            await timer.DisposeAsync();
        }
        _timers.Clear();

        await base.StopAsync(cancellationToken);
    }
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/Background/PlcCacheRefreshBackgroundService.cs
git commit -m "feat(siemens): add background cache refresh service"
```

---

## Task 15: 创建初始化服务

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/ISiemensCommunicatorInitializer.cs`
- Create: `module/SiemensCommunicator/SiemensCommunicator.Application/Services/SiemensCommunicatorInitializer.cs`

**Step 1: 创建初始化接口**

```csharp
namespace SiemensCommunicator.Core.Interfaces;

/// <summary>
/// SiemensCommunicator 模块初始化接口
/// </summary>
public interface ISiemensCommunicatorInitializer
{
    /// <summary>
    /// 初始化模块
    /// </summary>
    Task InitializeAsync();
}
```

**Step 2: 创建初始化实现**

```csharp
using Microsoft.Extensions.Hosting;
using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Application.Services;

/// <summary>
/// SiemensCommunicator 模块初始化服务
/// </summary>
public class SiemensCommunicatorInitializer : ISiemensCommunicatorInitializer, ITransient
{
    private readonly IPlcConnectionService _connectionService;
    private readonly IPlcConfigurationRepository _configRepository;

    public SiemensCommunicatorInitializer(
        IPlcConnectionService connectionService,
        IPlcConfigurationRepository configRepository)
    {
        _connectionService = connectionService;
        _configRepository = configRepository;
    }

    public async Task InitializeAsync()
    {
        // 获取所有启用的连接
        var connections = await _configRepository.GetAllConnectionsAsync();
        var activeConnections = connections.Where(c => c.IsActive).ToList();

        // 自动连接启用的 PLC
        foreach (var connection in activeConnections)
        {
            try
            {
                await _connectionService.ConnectAsync(connection.Id);
            }
            catch
            {
                // 连接失败由事件通知，不中断初始化流程
            }
        }
    }
}
```

**Step 3: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/Interfaces/ISiemensCommunicatorInitializer.cs
git add module/SiemensCommunicator/SiemensCommunicator.Application/Services/SiemensCommunicatorInitializer.cs
git commit -m "feat(siemens): add module initializer service"
```

---

## Task 16: 更新 Application 项目 GlobalUsings.cs

**Files:**
- Modify: `module/SiemensCommunicator/SiemensCommunicator.Application/GlobalUsings.cs`

**Step 1: 更新 GlobalUsings.cs**

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Furion;
global using Furion.DependencyInjection;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using SiemensCommunicator.Core.Interfaces;
global using SiemensCommunicator.Core.Models;
global using SiemensCommunicator.Core.Models.Enums;
global using SiemensCommunicator.Core.Events;
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/GlobalUsings.cs
git commit -m "refactor(siemens): update GlobalUsings.cs"
```

---

## Task 17: 更新 Core 项目 GlobalUsings.cs

**Files:**
- Modify: `module/SiemensCommunicator/SiemensCommunicator.Core/GlobalUsings.cs`

**Step 1: 更新 GlobalUsings.cs**

```csharp
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Linq;
global using System.Threading.Tasks;

global using Furion;
global using Microsoft.EntityFrameworkCore;
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Core/GlobalUsings.cs
git commit -m "refactor(siemens): update Core GlobalUsings.cs"
```

---

## Task 18: 注册模块服务

**Files:**
- Modify: `module/SiemensCommunicator/SiemensCommunicator.Application/Startup.cs`

**Step 1: 添加服务注册**

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace SiemensCommunicator.Application;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 注册内存缓存
        services.AddMemoryCache();

        // 注册后台服务
        services.AddHostedService<Background.PlcCacheRefreshBackgroundService>();

        // Furion 会自动扫描标注了 ITransient 等特性的服务
    }
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/Startup.cs
git commit -m "feat(siemens): register module services"
```

---

## Task 19: 创建数据库迁移

**Files:**
- New: `module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/Migrations/`

**Step 1: 添加迁移设计时包**

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

**Step 2: 创建初始迁移**

```bash
dotnet ef migrations add InitialCreate --project module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core --startup-project src/ChemicalTransportor/ChemicalTransportor.Web.Entry
```

**Step 3: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/Migrations/
git add module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/SiemensCommunicator.EntityFramework.Core.csproj
git commit -m "feat(siemens): add initial database migration"
```

---

## Task 20: 创建初始化数据脚本

**Files:**
- Create: `module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/Data/SeedData.sql`

**Step 1: 创建种子数据脚本**

```sql
-- 示例 PLC 连接配置
INSERT INTO PlcConnections (Id, Name, CpuType, Ip, Rack, Slot, CacheRefreshInterval, IsActive, CreatedAt)
VALUES
('PLC-DEMO-01', '演示 PLC', 40, '192.168.1.100', 0, 1, 1000, 0, datetime('now'));

-- 示例分组
INSERT INTO PlcGroups (Id, Name, Description, SortOrder)
VALUES
('GROUP-01', '温度传感器', '温度相关数据点', 1),
('GROUP-02', '压力传感器', '压力相关数据点', 2);

-- 示例数据点
INSERT INTO PlcDataPoints (Id, ConnectionId, GroupId, Name, Description, DataType, DbNumber, StartByte, VarType, DataCount, EnableCache, Unit, CreatedAt)
VALUES
('DP-TEMP-01', 'PLC-DEMO-01', 'GROUP-01', '主温度', '主生产线温度', 0, 1, 0, 6, 1, 1, '°C', datetime('now')),
('DP-PRESS-01', 'PLC-DEMO-01', 'GROUP-02', '主压力', '主生产线压力', 0, 1, 4, 6, 1, 1, 'Bar', datetime('now'));

-- 示例标签
INSERT INTO PlcTags (Id, Name, Color)
VALUES
('TAG-01', '报警', '#FF0000'),
('TAG-02', '实时', '#00FF00'),
('TAG-03', '历史', '#0000FF');
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.EntityFramework.Core/Data/SeedData.sql
git commit -m "feat(siemens): add seed data script"
```

---

## Task 21: 更新 SystemAppService 添加健康检查

**Files:**
- Modify: `module/SiemensCommunicator/SiemensCommunicator.Application/SystemAppService.cs`

**Step 1: 更新健康检查 API**

```csharp
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using SiemensCommunicator.Core.Interfaces;

namespace SiemensCommunicator.Application;

/// <summary>
/// 系统服务
/// </summary>
[ApiDescriptionSettings("SiemensCommunicator", Order = 1)]
[Route("api/siemens")]
public class SystemAppService : IDynamicApiController
{
    private readonly IPlcConnectionService _connectionService;

    public SystemAppService(IPlcConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    /// <summary>
    /// 获取模块描述
    /// </summary>
    [HttpGet("description")]
    public string GetDescription()
    {
        return "SiemensCommunicator - 西门子 PLC 通讯模块";
    }

    /// <summary>
    /// 获取所有 PLC 连接状态
    /// </summary>
    [HttpGet("connections/states")]
    public async Task<Dictionary<string, int>> GetAllConnectionStates()
    {
        var states = await _connectionService.GetAllStatesAsync();
        return states.ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value);
    }
}
```

**Step 2: 提交**

```bash
git add module/SiemensCommunicator/SiemensCommunicator.Application/SystemAppService.cs
git commit -m "feat(siemens): add health check API"
```

---

## Task 22: 构建验证

**Files:**
- (验证步骤，无文件修改)

**Step 1: 构建整个解决方案**

```bash
dotnet build ChemicalTransportor.slnx
```

Expected: 成功构建，无错误

**Step 2: 运行应用**

```bash
dotnet run --project src/ChemicalTransportor/ChemicalTransportor.Web.Entry
```

Expected: 应用启动成功

**Step 3: 验证 API**

```bash
curl http://localhost:5000/api/siemens/description
```

Expected: 返回 "SiemensCommunicator - 西门子 PLC 通讯模块"

**Step 4: 提交构建验证标记**

```bash
git commit --allow-empty -m "chore(siemens): build verification passed"
```

---

## 验收标准

完成所有任务后，模块应满足：

1. ✅ 可以成功构建整个解决方案
2. ✅ 数据库迁移成功创建
3. ✅ 可以通过 API 查询 PLC 连接状态
4. ✅ 支持多 PLC 连接配置
5. ✅ 支持数据点分组和标签
6. ✅ 支持单位换算配置
7. ✅ 后台定时刷新缓存
8. ✅ 错误事件通知机制

---

## 后续工作

- 添加单元测试
- 添加集成测试（可选，需要 PLC 模拟器）
- 添加数据校验逻辑
- 添加日志记录
- 性能优化
