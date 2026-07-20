namespace Performance.Web.Services;

/// <summary>
/// Abstracts LDAP authentication so the Razor Page login handler
/// is decoupled from the nsoftware library directly.
/// </summary>
public interface ILdapAuthService
{
    /// <summary>
    /// Attempts a direct LDAP bind using username@domain.com format.
    /// Returns (true, null) on success or (false, errorMessage) on failure.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> AuthenticateAsync(string username, string password);
}
