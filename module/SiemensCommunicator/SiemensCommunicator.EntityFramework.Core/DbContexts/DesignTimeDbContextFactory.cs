using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SiemensCommunicator.EntityFramework.Core.DbContexts;

/// <summary>
/// EF Core 设计时工厂（用于迁移）
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SiemensCommunicatorDbContext>
{
    public SiemensCommunicatorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SiemensCommunicatorDbContext>();
        optionsBuilder.UseSqlite("Data Source=SiemensCommunicator.db");

        return new SiemensCommunicatorDbContext(optionsBuilder.Options);
    }
}
