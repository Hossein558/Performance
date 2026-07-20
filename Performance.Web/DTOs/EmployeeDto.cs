namespace Performance.Web.DTOs;

/// <summary>
/// Lightweight data transfer object for displaying employee information in the UI.
/// </summary>
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonnelCode { get; set; } = string.Empty;

    /// <summary>Computed full name for display purposes.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Returns the first letter of each name part for avatar generation.</summary>
    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}"
                : FullName.Length > 0 ? FullName[0].ToString() : "?";
        }
    }
}
