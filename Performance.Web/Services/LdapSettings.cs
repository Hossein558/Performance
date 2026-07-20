namespace Performance.Web.Services;

/// <summary>
/// Strongly-typed settings for LDAP connection and licensing.
/// Bound from the "LdapSettings" section in appsettings.json.
/// LicenseKey is injected at startup from IANJA.lic via Program.cs.
/// </summary>
public class LdapSettings
{
    public string ServerName { get; set; } = "172.25.2.2";
    public string Domain { get; set; } = "crouseco.com";
    public int Port { get; set; } = 389;

    /// <summary>
    /// Content of IANJA.lic, injected into Configuration at startup.
    /// Do NOT store in source control.
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;
}
