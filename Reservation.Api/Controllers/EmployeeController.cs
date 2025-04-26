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
        return Ok(await _employeeService.GetEmployeesAsync(HttpContext.GetAccountIdFromBearer()));
    }

    [HttpGet("current")]
    public async Task<ActionResult<EmployeeResponse>> GetCurrentEmployee()
    {
        return Ok(await _employeeService.GetEmployeeAsync(HttpContext.GetUserIdFromBearer(),
            HttpContext.GetAccountIdFromBearer()));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> GetEmployee([FromRoute] int id)
    {
        return Ok(await _employeeService.GetEmployeeAsync(id, HttpContext.GetAccountIdFromBearer()));
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee([FromBody] EmployeeCreateRequest request)
    {
        var employee = await _employeeService.CreateEmployeeAsync(request, HttpContext.GetAccountIdFromBearer());
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployee([FromRoute] int id, EmployeeUpdateRequest request)
    {
        return Ok(await _employeeService.UpdateEmployeeAsync(id, request, HttpContext.GetAccountIdFromBearer()));
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployee([FromBody] EmployeeUpdateWithoutRoleRequest request)
    {
        return Ok(await _employeeService.UpdateEmployeeAsync(HttpContext.GetUserIdFromBearer(), request,
            HttpContext.GetAccountIdFromBearer()));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult> DeleteEmployee([FromRoute] int id)
    {
        if (id == HttpContext.GetUserIdFromBearer())
        {
            throw new CustomHttpException(HttpStatusCode.Locked, "Nemůžete smazat sami sebe");
        }

        await _employeeService.DeleteEmployeeAsync(id, HttpContext.GetAccountIdFromBearer());
        return NoContent();
    }
}