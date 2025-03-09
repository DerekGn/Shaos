using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shaos.Repository
{
    public class DatabaseDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShaosDbContext>
    {
        public ShaosDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ShaosDbContext>();
            builder.UseSqlite("Data Source=shaos.db;");
            return new ShaosDbContext(builder.Options);
        }
    }
}