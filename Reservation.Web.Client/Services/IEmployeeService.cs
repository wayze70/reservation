using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IEmployeeService
{
    Task<ApiResponse<List<EmployeeResponse>>> GetEmployees();
    Task<ApiResponse<EmployeeResponse>> GetCurrentEmployee();
    Task<ApiResponse<EmployeeResponse>> GetEmployee(int id);
    Task<ApiResponse<EmployeeResponse>> CreateEmployee(EmployeeCreateRequest request);
    Task<ApiResponse<EmployeeResponse>> UpdateEmployee(int id, EmployeeUpdateRequest request);
    Task<ApiResponse<EmployeeResponse>> UpdateSigninEmployee(EmployeeUpdateWithoutRoleRequest request);
    Task<ApiResponse<bool>> DeleteEmployee(int id);
}