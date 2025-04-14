// Services/IEmployeeService.cs
public interface IEmployeeService
{
    Task<List<EmployeeResponse>> GetEmployeesAsync(int accountId);
    Task<EmployeeResponse> GetEmployeeByUserIdAsync(int userId, int accountId);
    Task<EmployeeResponse> GetEmployeeAsync(int id, int accountId);
    Task<EmployeeResponse> CreateEmployeeAsync(EmployeeCreateRequest request, int accountId);
    Task<EmployeeResponse> UpdateEmployeeAsync(int id, EmployeeUpdateRequest request, int accountId);
    Task DeleteEmployeeAsync(int id, int accountId);
}