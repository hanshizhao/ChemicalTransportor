using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models;
using SiemensCommunicator.EntityFramework.Core.DbContexts;

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

    /// <inheritdoc />
    public async Task<List<PlcConnection>> GetAllConnectionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcConnections
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlcConnection?> GetConnectionAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcConnections
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    #endregion

    #region 数据点配置

    /// <inheritdoc />
    public async Task<List<PlcDataPoint>> GetAllDataPointsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcDataPoints
            .Include(d => d.Connection)
            .Include(d => d.Group)
            .OrderBy(d => d.ConnectionId)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PlcDataPoint>> GetDataPointsByConnectionAsync(long connectionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcDataPoints
            .Include(d => d.Connection)
            .Include(d => d.Group)
            .Where(d => d.ConnectionId == connectionId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PlcDataPoint>> GetDataPointsByGroupAsync(long groupId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcDataPoints
            .Include(d => d.Connection)
            .Include(d => d.Group)
            .Where(d => d.GroupId == groupId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlcDataPoint?> GetDataPointAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcDataPoints
            .Include(d => d.Connection)
            .Include(d => d.Group)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlcDataPoint?> GetDataPointByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcDataPoints
            .Include(d => d.Connection)
            .Include(d => d.Group)
            .FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
    }

    #endregion

    #region 分组配置

    /// <inheritdoc />
    public async Task<List<PlcGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcGroups
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region 标签配置

    /// <inheritdoc />
    public async Task<List<PlcTag>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlcTags
            .OrderBy(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
