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
        int accountId = HttpContext.GetAccountIdFromBearer();
        var employees = await _employeeService.GetEmployeesAsync(accountId);
        return Ok(employees);
    }
    
    [HttpGet("current")]
    public async Task<ActionResult<EmployeeResponse>> GetCurrentEmployee()
    {
        int userId = HttpContext.GetUserIdFromBearer();
        int accountId = HttpContext.GetAccountIdFromBearer();
        var employee = await _employeeService.GetEmployeeAsync(userId, accountId);
        return Ok(employee);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> GetEmployee([FromRoute] int id)
    {
        int accountId = HttpContext.GetAccountIdFromBearer();
        var employee = await _employeeService.GetEmployeeAsync(id, accountId);
        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee(EmployeeCreateRequest request)
    {
        int accountId = HttpContext.GetAccountIdFromBearer();
        var employee = await _employeeService.CreateEmployeeAsync(request, accountId);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployee([FromRoute] int id, EmployeeUpdateRequest request)
    {
        int accountId = HttpContext.GetAccountIdFromBearer();
        var employee = await _employeeService.UpdateEmployeeAsync(id, request, accountId);
        return Ok(employee);
    }
    
    [HttpPut]
    [Authorize]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployee(EmployeeUpdateWithoutRoleRequest request)
    {
        int accountId = HttpContext.GetAccountIdFromBearer();
        int userId = HttpContext.GetUserIdFromBearer();
        var employee = await _employeeService.UpdateEmployeeAsync(userId, request, accountId);
        return Ok(employee);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult> DeleteEmployee([FromRoute] int id)
    {
        int accountId = HttpContext.GetAccountIdFromBearer();
        int requestEmployeeId = HttpContext.GetUserIdFromBearer();
        
        if (id == requestEmployeeId)
        {
            throw new CustomHttpException(HttpStatusCode.Locked, "Nemůžete smazat sami sebe");
        }
        
        await _employeeService.DeleteEmployeeAsync(id, accountId);
        return NoContent();
    }
}