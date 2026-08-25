using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Platform.DataAccess.Postgress
{
    public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
    {
        public PlatformDbContext CreateDbContext(string[] args)
        {
            var connectionString = PlatformDatabaseConnection.RequireFromEnvironment();

            var options = new DbContextOptionsBuilder<PlatformDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new PlatformDbContext(options);
        }
    }
}
