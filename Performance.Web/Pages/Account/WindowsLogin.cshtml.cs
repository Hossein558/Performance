using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Performance.Web.Services;
using System.Security.Claims;

namespace Performance.Web.Pages.Account;

[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
public class WindowsLoginModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<WindowsLoginModel> _logger;

    public WindowsLoginModel(IEmployeeService employeeService, ILogger<WindowsLoginModel> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var windowsIdentity = User.Identity;

        if (windowsIdentity == null || !windowsIdentity.IsAuthenticated || string.IsNullOrWhiteSpace(windowsIdentity.Name))
        {
            _logger.LogWarning("Windows authentication failed or missing identity.");
            return RedirectToPage("/Account/Login", new { error = "Windows authentication failed." });
        }

        // Extract just the username part from DOMAIN\username or username@domain.com
        var username = windowsIdentity.Name;
        if (username.Contains('\\'))
        {
            username = username.Split('\\')[1];
        }
        else if (username.Contains('@'))
        {
            username = username.Split('@')[0];
        }

        var employee = await _employeeService.GetEmployeeByPersonnelCodeAsync(username);

        if (employee == null)
        {
            _logger.LogWarning("Authenticated Windows user '{Username}' has no matching record in the Employees table.", username);
            return RedirectToPage("/Account/Login", new { error = "حساب شما در سیستم ثبت نشده است. با واحد منابع انسانی تماس بگیرید." });
        }

        // Issue our standard application Cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("EmployeeId", employee.Id.ToString()),
            new Claim("FullName", $"{employee.FirstName} {employee.LastName}".Trim()),
            new Claim("PersonnelCode", employee.PersonnelCode)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        _logger.LogInformation("User '{Username}' signed in successfully via Windows SSO.", username);

        var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
}
