using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Performance.Web.Services;
using System.Security.Claims;
using System.Security.Principal;

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
        var authenticateResult = await HttpContext.AuthenticateAsync(NegotiateDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            // Explicitly challenge the browser to send Windows credentials
            return Challenge(NegotiateDefaults.AuthenticationScheme);
        }

        var winIdentity = authenticateResult.Principal?.Identity as WindowsIdentity;
        if (winIdentity == null || !winIdentity.IsAuthenticated)
        {
            return RedirectToPage("/Account/Login", new { error = "windows_auth_failed" });
        }

        // Extract username (e.g., CROUSECO\he110749 -> he110749)
        var username = winIdentity.Name.Split('\\').Last();

        // Find in DB
        var employee = await _employeeService.GetEmployeeByPersonnelCodeAsync(username);
        if (employee == null)
        {
            return RedirectToPage("/Account/Login", new { error = "user_not_found" });
        }

        // Sign in with standard application Cookie
        var claims = new[] {
            new Claim(ClaimTypes.Name, username),
            new Claim("PersonnelCode", employee.PersonnelCode),
            new Claim("EmployeeId", employee.Id.ToString()),
            new Claim("FullName", $"{employee.FirstName} {employee.LastName}")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return LocalRedirect("/");
    }
}
