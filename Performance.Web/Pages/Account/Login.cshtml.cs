using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Performance.Web.Services;
using System.Security.Claims;

namespace Performance.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILdapAuthService _ldapAuthService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        ILdapAuthService ldapAuthService,
        IEmployeeService employeeService,
        ILogger<LoginModel> logger)
    {
        _ldapAuthService = ldapAuthService;
        _employeeService = employeeService;
        _logger = logger;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet([FromQuery] string? error)
    {
        // Already logged in — skip login page
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect("/");

        if (!string.IsNullOrEmpty(error))
        {
            ErrorMessage = error;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // ── Basic input validation ────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "لطفاً نام کاربری و رمز عبور را وارد کنید.";
            return Page();
        }

        // ── Step 1: LDAP bind ─────────────────────────────────────────────────
        var (ldapSuccess, ldapError) = await _ldapAuthService.AuthenticateAsync(Username, Password);

        if (!ldapSuccess)
        {
            _logger.LogWarning("Login failed for user '{Username}'. LDAP error: {Error}",
                Username, ldapError);
            ErrorMessage = "نام کاربری یا رمز عبور اشتباه است. لطفاً دوباره تلاش کنید.";
            return Page();
        }

        // ── Step 2: Look up the employee record in the database ───────────────
        // PersonnelCode stores the bare sAMAccountName (e.g. "he110749").
        // Strip any domain prefix the user may have typed (he110749@crouseco.com
        // or CROUSECO\he110749) so the DB lookup always uses the plain username.
        var personnelCode = Username;
        if (personnelCode.Contains('@'))
            personnelCode = personnelCode.Split('@')[0];
        else if (personnelCode.Contains('\\'))
            personnelCode = personnelCode.Split('\\')[1];

        var employee = await _employeeService.GetEmployeeByPersonnelCodeAsync(personnelCode);

        if (employee == null)
        {
            _logger.LogWarning(
                "Authenticated AD user '{Username}' has no matching record in the Employees table.",
                Username);
            ErrorMessage = "حساب شما در سیستم ثبت نشده است. با واحد منابع انسانی تماس بگیرید.";
            return Page();
        }

        // ── Step 3: Issue authentication cookie ───────────────────────────────
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,   Username),
            new Claim("EmployeeId",      employee.Id.ToString()),
            new Claim("FullName",        $"{employee.FirstName} {employee.LastName}".Trim()),
            new Claim("PersonnelCode",   employee.PersonnelCode)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,               // session cookie — clears on browser close
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        _logger.LogInformation(
            "User '{Username}' (EmployeeId: {EmployeeId}) signed in successfully.",
            Username, employee.Id);

        // Honour returnUrl for post-auth redirects; default to dashboard
        var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
}
