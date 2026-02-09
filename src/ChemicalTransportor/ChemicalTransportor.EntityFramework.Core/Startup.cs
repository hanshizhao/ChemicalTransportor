using Furion;
using Microsoft.Extensions.DependencyInjection;

namespace ChemicalTransportor.EntityFramework.Core
{
    public class Startup : AppStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseAccessor(options =>
            {
                options.AddDbPool<DefaultDbContext>();
            }, "ChemicalTransportor.Database.Migrations");
        }
    }
}
