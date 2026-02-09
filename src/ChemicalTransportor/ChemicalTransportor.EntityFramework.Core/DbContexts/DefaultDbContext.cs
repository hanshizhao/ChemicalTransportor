using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace ChemicalTransportor.EntityFramework.Core
{
    [AppDbContext("ChemicalTransportor", DbProvider.Sqlite)]
    public class DefaultDbContext : AppDbContext<DefaultDbContext>
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {
        }
    }
}
