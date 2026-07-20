using Microsoft.EntityFrameworkCore;
using Performance.Web.Data;
using Performance.Web.Models;

namespace Performance.Web.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Phase 5 core query: returns employees where the logged-in manager's Guid
    /// matches ANY of the four Manager fields.
    /// </summary>
    public async Task<IEnumerable<Employee>> GetSubordinatesAsync(Guid managerId)
    {
        return await _dbSet
            .Where(e =>
                e.Manager1Id == managerId ||
                e.Manager2Id == managerId ||
                e.Manager3Id == managerId ||
                e.Manager4Id == managerId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<Employee?> GetByPersonnelCodeAsync(string personnelCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.PersonnelCode == personnelCode);
    }
}
