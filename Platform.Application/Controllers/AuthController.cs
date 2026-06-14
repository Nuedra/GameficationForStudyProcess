using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Services;
using Platform.Core.Models;

namespace Platform.Application.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IStudentIdentityService studentIdentityService) : ControllerBase
{
    [HttpPost("student/login")]
    [AllowAnonymous]
    public async Task<ActionResult<StudentDto>> Login(
        StudentLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return BadRequest();

        var student = await studentIdentityService.FindByIdAsync(request.Id, cancellationToken);
        if (student is null)
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new Claim(ClaimTypes.Name, student.FullName),
            new Claim(ClaimTypes.Role, UserRoleDictionary.Values[UserRole.Student])
        };
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true
            });

        return Ok(student);
    }

    [HttpGet("me")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<StudentDto>> GetCurrentStudent(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var studentId))
            return Unauthorized();

        var student = await studentIdentityService.FindByIdAsync(studentId, cancellationToken);
        return student is null ? Unauthorized() : Ok(student);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
