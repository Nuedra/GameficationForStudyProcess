using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Platform.Application.Services;

namespace Platform.Application.Pages;

[Authorize]
public sealed class LogoutModel(IUserSessionService userSessionService) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await userSessionService.SignOutAsync(HttpContext);
        return LocalRedirect("/");
    }
}
