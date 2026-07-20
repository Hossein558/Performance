using Performance.Web.Models;

namespace Performance.Web.Repositories;

/// <summary>Employee-specific repository operations.</summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Returns all employees where the given Guid appears in any of the
    /// Manager1Id–Manager4Id fields (the core subordinate query).
    /// </summary>
    Task<IEnumerable<Employee>> GetSubordinatesAsync(Guid managerId);

    /// <summary>Looks up an employee by their PersonnelCode (= AD sAMAccountName).</summary>
    Task<Employee?> GetByPersonnelCodeAsync(string personnelCode);
}
