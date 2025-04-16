// Controllers/EmployeeController.cs

using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.Services;
using Reservation.Shared.Authorization;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<List<EmployeeResponse>>> GetEmployees()
    {
        int accountId = HttpContext.GetAccountId();
        var employees = await _employeeService.GetEmployeesAsync(accountId);
        return Ok(employees);
    }
    
    [HttpGet("current")]
    public async Task<ActionResult<EmployeeResponse>> GetCurrentEmployee()
    {
        int userId = HttpContext.GetUserId();
        int accountId = HttpContext.GetAccountId();
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId, accountId);
        return Ok(employee);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> GetEmployee(int id)
    {
        int accountId = HttpContext.GetAccountId();
        var employee = await _employeeService.GetEmployeeAsync(id, accountId);
        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee(EmployeeCreateRequest request)
    {
        int accountId = HttpContext.GetAccountId();
        var employee = await _employeeService.CreateEmployeeAsync(request, accountId);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployee(int id, EmployeeUpdateRequest request)
    {
        int accountId = HttpContext.GetAccountId();
        var employee = await _employeeService.UpdateEmployeeAsync(id, request, accountId);
        return Ok(employee);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult> DeleteEmployee(int id)
    {
        int accountId = HttpContext.GetAccountId();
        int requestEmployeeId = HttpContext.GetUserId();
        
        if (id == requestEmployeeId)
        {
            throw new CustomHttpException(HttpStatusCode.Locked, "Nemůžete smazat sami sebe.");
        }
        
        await _employeeService.DeleteEmployeeAsync(id, accountId);
        return NoContent();
    }
}