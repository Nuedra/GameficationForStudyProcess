using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Platform.Application.Services;

namespace Platform.Application.Pages;

public sealed class LoginModel(
    IUserSessionService userSessionService,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public string UserId { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true
            ? LocalRedirect(NormalizeReturnUrl(ReturnUrl))
            : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(UserId?.Trim(), out var userId) || userId == Guid.Empty)
        {
            ErrorMessage = "Укажите корректный ID пользователя в формате GUID.";
            return Page();
        }

        var user = await userSessionService.SignInAsync(HttpContext, userId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning(
                "GUID form login rejected. UserIdPrefix={UserIdPrefix}, RemoteIp={RemoteIp}",
                userId.ToString("N")[..8],
                HttpContext.Connection.RemoteIpAddress);
            ErrorMessage = "Пользователь с таким ID не найден или его вход отключён.";
            return Page();
        }

        return LocalRedirect(NormalizeReturnUrl(ReturnUrl));
    }

    private string NormalizeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/";
    }
}
