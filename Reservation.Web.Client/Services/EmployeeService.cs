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
    Task<ApiResponse<bool>> DeleteEmployee(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly IHttpClientService _httpClientService;
    private const string BaseUrl = "employee";

    public EmployeeService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<ApiResponse<List<EmployeeResponse>>> GetEmployees()
    {
        return await _httpClientService.GetAsync<List<EmployeeResponse>>(BaseUrl);
    }

    public async Task<ApiResponse<EmployeeResponse>> GetCurrentEmployee()
    {
        return await _httpClientService.GetAsync<EmployeeResponse>(BaseUrl + "/current");
    }

    public async Task<ApiResponse<EmployeeResponse>> GetEmployee(int id)
    {
        return await _httpClientService.GetAsync<EmployeeResponse>($"{BaseUrl}/{id}");
    }

    public async Task<ApiResponse<EmployeeResponse>> CreateEmployee(EmployeeCreateRequest request)
    {
        return await _httpClientService.PostAsync<EmployeeCreateRequest, EmployeeResponse>(BaseUrl, request);
    }

    public async Task<ApiResponse<EmployeeResponse>> UpdateEmployee(int id, EmployeeUpdateRequest request)
    {
        return await _httpClientService.PutAsync<EmployeeUpdateRequest, EmployeeResponse>($"{BaseUrl}/{id}", request);
    }

    public async Task<ApiResponse<bool>> DeleteEmployee(int id)
    {
        return await _httpClientService.DeleteAsync<bool>($"{BaseUrl}/{id}");
    }
}