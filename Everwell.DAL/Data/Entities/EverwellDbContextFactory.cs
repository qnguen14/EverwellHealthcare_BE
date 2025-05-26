using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Everwell.DAL.Data.Entities
{
    public class EverwellDbContextFactory : IDesignTimeDbContextFactory<EverwellDbContext>
    {
        public EverwellDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Everwell.API");


            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("SupabaseConnection");


            var optionsBuilder = new DbContextOptionsBuilder<EverwellDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new EverwellDbContext(optionsBuilder.Options);
        }
    }
}
