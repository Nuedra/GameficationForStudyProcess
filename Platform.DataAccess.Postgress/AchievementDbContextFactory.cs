using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Platform.DataAccess.Postgress;

public sealed class AchievementDbContextFactory
    : IDesignTimeDbContextFactory<AchievementDbContext>
{
    public AchievementDbContext CreateDbContext(string[] args)
    {
        var connectionString = PlatformDatabaseConnection.RequireFromEnvironment();

        var options = new DbContextOptionsBuilder<AchievementDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AchievementDbContext(options);
    }
}
