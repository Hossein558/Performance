using Performance.Web.DTOs;
using Performance.Web.Models;
using Performance.Web.Repositories;

namespace Performance.Web.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmployeeDto>> GetMySubordinatesAsync(Guid currentUserId)
    {
        var employees = await _employeeRepository.GetSubordinatesAsync(currentUserId);

        return employees.Select(e => new EmployeeDto
        {
            Id           = e.Id,
            FirstName    = e.FirstName,
            LastName     = e.LastName,
            PersonnelCode = e.PersonnelCode
        });
    }

    /// <inheritdoc />
    public async Task<Employee?> GetEmployeeByPersonnelCodeAsync(string personnelCode)
    {
        return await _employeeRepository.GetByPersonnelCodeAsync(personnelCode);
    }
}
