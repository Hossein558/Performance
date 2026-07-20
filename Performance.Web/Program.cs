using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Performance.Web.Components;
using Performance.Web.Data;
using Performance.Web.Repositories;
using Performance.Web.Services;

// ═══════════════════════════════════════════════════════════════════════════════
// SYNCFUSION LICENSE — MUST be the very first call before builder or any component
// ═══════════════════════════════════════════════════════════════════════════════
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("@32392e302e303b32393bKq35AiUSRDJT5uIaFzRCrJWDo7gKUKH1Rwb6jH+WX4o=");

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════════
// KESTREL CONFIGURATION
// Fix HTTP 400 with Windows Auth: Allow large Kerberos tokens in headers
// ═══════════════════════════════════════════════════════════════════════════════
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 1048576; // 1MB
    options.Limits.MaxRequestLineSize = 1048576;
});

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
    .AddNegotiate(options => 
    {
        options.Events = new NegotiateEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Negotiate authentication failed.");
                return Task.CompletedTask;
            }
        };
    });

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
// SYNCFUSION BLAZOR
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddSyncfusionBlazor();

// (License already registered at top of file)

// ═══════════════════════════════════════════════════════════════════════════════
// HTTP PIPELINE
// Order is critical: Auth → Authorization → Antiforgery → Endpoints
// ═══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

var supportedCultures = new[] { "fa-IR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

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

// ═══════════════════════════════════════════════════════════════════════════════
// AVATAR IMAGE ENDPOINT
// Serves employee profile photos from the UNC share.
// UNC paths are inaccessible to browsers directly — this proxies them safely.
// Path format: \\datap2\Crouse\Services-Support-P2\Personel\1-{personnelCode}.JPG
// ═══════════════════════════════════════════════════════════════════════════════
app.MapGet("/api/avatar/{personnelCode}", (string personnelCode, ILogger<Program> logger) =>
{
    // Sanitize: reject any path traversal characters
    if (string.IsNullOrWhiteSpace(personnelCode) ||
        personnelCode.Contains('\\') ||
        personnelCode.Contains('/') ||
        personnelCode.Contains(".", StringComparison.Ordinal))
    {
        logger.LogWarning("Avatar request rejected — invalid personnelCode: '{Code}'", personnelCode);
        return Results.NotFound();
    }

    var filePath = @"\\datap2\Crouse\Services-Support-P2\Personel\1-" + personnelCode + ".JPG";
    logger.LogInformation("Avatar request for personnelCode '{Code}' — resolved path: {Path}", personnelCode, filePath);

    if (!File.Exists(filePath))
    {
        logger.LogWarning("Avatar file NOT found for personnelCode '{Code}' at: {Path}", personnelCode, filePath);
        return Results.NotFound();
    }

    logger.LogInformation("Avatar file FOUND for personnelCode '{Code}' — serving.", personnelCode);
    var stream = File.OpenRead(filePath);
    return Results.File(stream, contentType: "image/jpeg", enableRangeProcessing: false);
}).AllowAnonymous(); // Images do not carry sensitive data; auth handled at page level

app.Run();
