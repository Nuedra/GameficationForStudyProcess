using Microsoft.EntityFrameworkCore;

namespace Platform.DataAccess.Postgress
{
    public class AchievementRepository
    {
        private readonly AchievementDbContext _db;

        public AchievementRepository(AchievementDbContext db)
        {
            _db = db;
        }

            public Task<AchievementEntity?> GetAchievementFullAsync(Guid achievementId)
        {
            return _db.Achievements
                .AsNoTracking()
                .Include(a => a.Criteria)
                .Include(a => a.StudentAchievements)
                .FirstOrDefaultAsync(a => a.Id == achievementId);
        }

            public Task<List<AchievementEntity>> GetCourseAchievementsAsync(Guid courseId)
        {
            return _db.Achievements
                .AsNoTracking()
                .Where(a => a.CourseID == courseId)
                .Include(a => a.Criteria)
                .OrderBy(a => a.Year)
                .ToListAsync();
        }

            public Task<List<StudentAchievementEntity>> GetStudentAchievementsAsync(Guid studentId)
        {
            return _db.StudentAchievements
                .AsNoTracking()
                .Where(x => x.StudentID == studentId)
                .Include(x => x.Achievement)
                .OrderByDescending(x => x.AchievementGotDate)
                .ToListAsync();
        }
    }
}
