using Microsoft.Extensions.Options;
using nsoftware.IPWorksAuth;

namespace Performance.Web.Services;

/// <summary>
/// Authenticates users against Active Directory via the nsoftware IPWorks Auth
/// Ldap component using a direct UPN bind (username@domain.com).
///
/// STRICT REQUIREMENT: Do NOT use System.DirectoryServices.
/// Do NOT query thumbnailPhoto, jpegPhoto, or any other attributes — bind only.
///
/// The RuntimeLicense is loaded from IANJA.lic at startup via Program.cs and
/// injected through IOptions&lt;LdapSettings&gt;.LicenseKey.
/// </summary>
public class LdapAuthService : ILdapAuthService
{
    private readonly LdapSettings _settings;
    private readonly ILogger<LdapAuthService> _logger;

    public LdapAuthService(IOptions<LdapSettings> settings, ILogger<LdapAuthService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage)> AuthenticateAsync(
        string username, string password)
    {
        // Wrap the synchronous nsoftware Ldap.Bind() call in Task.Run so it
        // does not block the ASP.NET Core request thread pool.
        return await Task.Run<(bool, string?)>(() =>
        {
            var ldap = new LDAP();
            try
            {
                // Apply the runtime license (content of IANJA.lic)
                if (!string.IsNullOrWhiteSpace(_settings.LicenseKey))
                    ldap.RuntimeLicense = _settings.LicenseKey;

                ldap.ServerName = _settings.ServerName;
                ldap.ServerPort = _settings.Port;
                ldap.Timeout = 30; // seconds — prevents hanging on unreachable server

                // Direct bind: Active Directory accepts UPN format as the bind DN
                ldap.DN = $"{username}@{_settings.Domain}";
                ldap.Password = password;

                // Attempt the bind — throws an exception if credentials are invalid
                ldap.Bind();

                _logger.LogInformation(
                    "LDAP bind successful for user '{Username}' on {Server}:{Port}",
                    username, _settings.ServerName, _settings.Port);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "LDAP bind failed for user '{Username}'. Reason: {Error}",
                    username, ex.Message);

                return (false, ex.Message);
            }
            finally
            {
                // Always disconnect cleanly — ignore errors during disconnect
                try { ldap.Unbind(); } catch { /* intentionally swallowed */ }
            }
        });
    }
}
