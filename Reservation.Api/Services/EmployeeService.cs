using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Models;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly DataContext _context;
    private readonly IPasswordHasher<Owner> _passwordHasher;

    public EmployeeService(DataContext context, IPasswordHasher<Owner> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<EmployeeResponse>> GetEmployeesAsync(int accountId)
    {
        var employees = await _context.Owners
            .Where(e => e.AccountId == accountId)
            .ToListAsync();

        return employees.Select(ToUserResponse).ToList();
    }

    public async Task<EmployeeResponse> GetEmployeeByUserIdAsync(int userId, int accountId)
    {
        var employee = await _context.Owners
            .FirstOrDefaultAsync(e => e.Id == userId && e.AccountId == accountId);

        if (employee == null)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Zaměstnanec nebyl nalezen");

        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> GetEmployeeAsync(int id, int accountId)
    {
        var employee = await GetEmployeeEntity(id, accountId);
        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(EmployeeCreateRequest request, int accountId)
    {
        var employee = new Owner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = request.Role,
            PasswordHash = _passwordHasher.HashPassword(new Owner(), request.Password),
            AccountId = accountId,
        };

        _context.Owners.Add(employee);
        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }
    
    public async Task<EmployeeResponse> UpdateEmployeeAsync(int id, EmployeeUpdateWithoutRoleRequest request, int accountId)
    {
        var employee = await GetEmployeeEntity(id, accountId);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;

        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int id, EmployeeUpdateRequest request, int accountId)
    {
        var employee = await GetEmployeeEntity(id, accountId);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Role = request.Role;

        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }

    public async Task DeleteEmployeeAsync(int id, int accountId)
    {
        var employee = await GetEmployeeEntity(id, accountId);
        _context.Owners.Remove(employee);
        await _context.SaveChangesAsync();
    }

    private async Task<Owner> GetEmployeeEntity(int id, int accountId)
    {
        var employee = await _context.Owners
            .FirstOrDefaultAsync(e => e.Id == id && e.AccountId == accountId);

        if (employee == null)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Zaměstnanec nebyl nalezen");

        return employee;
    }

    private static EmployeeResponse ToUserResponse(Owner employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Role = employee.Role,
        };
    }
}