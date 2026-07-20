using Performance.Web.DTOs;
using Performance.Web.Models;

namespace Performance.Web.Services;

public interface IEmployeeService
{
    /// <summary>
    /// Phase 5: Returns all employees where currentUserId matches
    /// Manager1Id, Manager2Id, Manager3Id, or Manager4Id.
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetMySubordinatesAsync(Guid currentUserId);

    /// <summary>
    /// Looks up an employee by PersonnelCode (= AD sAMAccountName).
    /// Used after LDAP auth to get the employee's Guid and full name.
    /// </summary>
    Task<Employee?> GetEmployeeByPersonnelCodeAsync(string personnelCode);
}
