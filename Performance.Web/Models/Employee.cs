namespace Performance.Web.Models;

/// <summary>
/// Represents a company employee.
/// Manager1Id through Manager4Id allow querying who reports to a given manager
/// across up to four management chains without FK constraints (to avoid cycle issues).
/// PhotoPath is intentionally omitted and will be added in a future phase.
/// </summary>
public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Matches the sAMAccountName in Active Directory.
    /// Used as the lookup key after LDAP authentication.
    /// </summary>
    public string PersonnelCode { get; set; } = string.Empty;

    // Up to four management levels — nullable, no FK constraints
    public Guid? Manager1Id { get; set; }
    public Guid? Manager2Id { get; set; }
    public Guid? Manager3Id { get; set; }
    public Guid? Manager4Id { get; set; }
}
