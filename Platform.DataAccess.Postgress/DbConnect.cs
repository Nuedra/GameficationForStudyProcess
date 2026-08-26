using Microsoft.EntityFrameworkCore;

namespace Platform.DataAccess.Postgress
{
    public static class PlatformDatabase
    {
        public static AchievementDbContext Connect(string connectionString)
        {
            var options = new DbContextOptionsBuilder<AchievementDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AchievementDbContext(options);
        }
    }
}
