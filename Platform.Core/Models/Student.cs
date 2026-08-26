using System.Diagnostics.Eventing.Reader;
namespace Platform.Core.Models;

public class Student
{
    private Student(Guid id, string name, string surname, string group, List<StudentAchievement> studentAchievements)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Group = group;
        StudentAchievements = studentAchievements;
    }

    public Student(Platform.DataAccess.Postgress.StudentEntity entity)
    : this(
        entity.Id,
        entity.Name,
        entity.Surname,
        entity.Group,
        []
    )
    {
    }

    public Guid Id { get;}
    public string Name { get;} = string.Empty;
    public string Surname { get;} = string.Empty;
    public string Group { get;} = string.Empty;
    public List<StudentAchievement> StudentAchievements { get;} = [];

    public static (Student student, string error) Create(Guid id, string name, string surname, string group, List<StudentAchievement> studentAchievements)
    {
        var error = string.Empty;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(group))
        {
            error = "Name/surname/group cannot be empty";
        }
        var student = new Student(id, name, surname, group, studentAchievements);
        return (student, error);
    }
    
}
