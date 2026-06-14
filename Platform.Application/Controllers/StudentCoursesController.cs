using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;

namespace Platform.Application.Controllers;

[ApiController]
[Authorize(Roles = "student")]
[Route("api/student/courses")]
public sealed class StudentCoursesController(IStudentCourseService studentCourseService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetCourses(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized(ApiErrors.AuthenticationRequired);

        var courses = await studentCourseService.GetCoursesAsync(
            studentId,
            cancellationToken);

        return Ok(courses);
    }
}
