namespace Platform.Core.Models;
public class StudentAchievement
{
    private StudentAchievement(Guid id, DateTime achievementGotDate, DateTime achievementFoundDate, bool isNotificationSeen,
    bool isFirstAnimationShown, Guid? labID, Guid achievementID, Achievement achievement, Guid studentID)
    {
        Id = id;
        AchievementGotDate = achievementGotDate;
        AchievementFoundDate = achievementFoundDate;
        IsNotificationSeen = isNotificationSeen;
        IsFirstAnimationShown = isFirstAnimationShown;
        LabID = labID;
        AchievementID = achievementID;
        Achievement = achievement;
        StudentID = studentID;
    }

    public StudentAchievement(Platform.DataAccess.Postgress.StudentAchievementEntity entity)
    : this(
        entity.Id,
        entity.AchievementGotDate,
        entity.AchievementFoundDate,
        entity.IsNotificationSeen,
        entity.IsFirstAnimationShown,
        entity.LabID,
        entity.AchievementID,
        entity.Achievement != null
            ? new Achievement(entity.Achievement)
            : null!,
        entity.StudentID
    )
    {
    }

    public Guid Id { get;}
    public DateTime AchievementGotDate { get;}
    public DateTime AchievementFoundDate { get;}   
    public bool IsNotificationSeen { get;} = false; 
    public bool IsFirstAnimationShown { get;} = false;
    public Guid? LabID { get; }
    public Guid AchievementID { get;}
    public Achievement Achievement { get;}
    public Guid StudentID { get;}

}
