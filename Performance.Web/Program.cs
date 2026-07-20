using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Performance.Web.Components;
using Performance.Web.Data;
using Performance.Web.Repositories;
using Performance.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════════
// LICENSE LOADING
// Reads IANJA.lic relative to the executable directory.
// This path resolves correctly on both Windows (dev) and Linux (production).
// ═══════════════════════════════════════════════════════════════════════════════
var licPath = Path.Combine(AppContext.BaseDirectory, "IANJA.lic");
if (File.Exists(licPath))
{
    var licContent = File.ReadAllText(licPath);
    var rtkMatch = System.Text.RegularExpressions.Regex.Match(licContent, "\"RTK\"=\"([^\"]+)\"");
    if (rtkMatch.Success)
    {
        builder.Configuration["LdapSettings:LicenseKey"] = rtkMatch.Groups[1].Value;
    }
    else
    {
        // Fallback in case it's just a plain text key
        builder.Configuration["LdapSettings:LicenseKey"] = licContent.Trim();
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[WARN] IANJA.lic not found at: {licPath}");
    Console.WriteLine("[WARN] LDAP authentication will not function until the license file is in place.");
    Console.ResetColor();
}

// ═══════════════════════════════════════════════════════════════════════════════
// AUTHENTICATION & AUTHORIZATION
// Cookie-based auth so the session survives Blazor SignalR circuit reconnects.
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath           = "/Account/Login";
        options.LogoutPath          = "/Account/Logout";
        options.AccessDeniedPath    = "/Account/Login";
        options.ExpireTimeSpan      = TimeSpan.FromHours(8);
        options.SlidingExpiration   = true;
        options.Cookie.HttpOnly     = true;
        options.Cookie.SameSite     = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    .AddNegotiate();

builder.Services.AddAuthorization();

// Makes Task<AuthenticationState> available as a cascading value to all
// Blazor components without manually wrapping in <CascadingAuthenticationState>.
builder.Services.AddCascadingAuthenticationState();

// ═══════════════════════════════════════════════════════════════════════════════
// RAZOR PAGES  (for Login and Logout — need real HttpContext.SignInAsync)
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddRazorPages();

// ═══════════════════════════════════════════════════════════════════════════════
// DATABASE
// ASPNETCORE_ENVIRONMENT=Development  →  appsettings.json  (Test DB)
// ASPNETCORE_ENVIRONMENT=Production   →  appsettings.Production.json overrides DefaultConnection
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ═══════════════════════════════════════════════════════════════════════════════
// REPOSITORIES
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IEmployeeRepository,   EmployeeRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();

// ═══════════════════════════════════════════════════════════════════════════════
// APPLICATION SERVICES
// LdapSettings includes LicenseKey injected above from the .lic file.
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.Configure<Performance.Web.Services.LdapSettings>(builder.Configuration.GetSection("LdapSettings"));
builder.Services.AddScoped<ILdapAuthService,  LdapAuthService>();
builder.Services.AddScoped<IEmployeeService,  EmployeeService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();

// ═══════════════════════════════════════════════════════════════════════════════
// BLAZOR — Interactive Server (global render mode set in App.razor)
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ═══════════════════════════════════════════════════════════════════════════════
// HTTP PIPELINE
// Order is critical: Auth → Authorization → Antiforgery → Endpoints
// ═══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages();                           // Login & Logout Razor Pages
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
