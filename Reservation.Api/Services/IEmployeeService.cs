using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IEmployeeService
{
    Task<List<EmployeeResponse>> GetEmployeesAsync(int accountId);
    Task<EmployeeResponse> GetEmployeeAsync(int userId, int accountId);
    Task<EmployeeResponse> CreateEmployeeAsync(EmployeeCreateRequest request, int accountId);
    Task<EmployeeResponse> UpdateEmployeeAsync(int userId, EmployeeUpdateRequest request, int accountId);
    Task<EmployeeResponse> UpdateEmployeeAsync(int userId, EmployeeUpdateWithoutRoleRequest request, int accountId);
    Task DeleteEmployeeAsync(int userId, int accountId);
}