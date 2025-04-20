using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Models;
using Reservation.Shared.Common;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly DataContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public EmployeeService(DataContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<EmployeeResponse>> GetEmployeesAsync(int accountId)
    {
        var employees = await _context.Users
            .Where(e => e.AccountId == accountId)
            .ToListAsync();

        return employees.Select(ToUserResponse).ToList();
    }

    public async Task<EmployeeResponse> GetEmployeeAsync(int userId, int accountId)
    {
        var employee = await GetEmployeeEntity(userId, accountId);
        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(EmployeeCreateRequest request, int accountId)
    {
        if (!Utils.IsPasswordLongEnough(request.Password))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Heslo musí mít alespoň 6 znaků");
        }

        if (!Utils.TryProcessEmail(request.Email, out string email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatný formát emailu");
        }
        
        var employee = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            Role = request.Role,
            PasswordHash = _passwordHasher.HashPassword(new User(), request.Password),
            AccountId = accountId,
        };

        _context.Users.Add(employee);
        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }
    
    public async Task<EmployeeResponse> UpdateEmployeeAsync(int userId, EmployeeUpdateWithoutRoleRequest request, int accountId)
    {
        var employee = await GetEmployeeEntity(userId, accountId);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;

        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int userId, EmployeeUpdateRequest request, int accountId)
    {
        var employee = await GetEmployeeEntity(userId, accountId);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Role = request.Role;

        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }

    public async Task DeleteEmployeeAsync(int userId, int accountId)
    {
        var employee = await GetEmployeeEntity(userId, accountId);
        _context.Users.Remove(employee);
        await _context.SaveChangesAsync();
    }

    private async Task<User> GetEmployeeEntity(int id, int accountId)
    {
        var employee = await _context.Users
            .FirstOrDefaultAsync(e => e.Id == id && e.AccountId == accountId);

        if (employee == null)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Zaměstnanec nebyl nalezen");

        return employee;
    }

    private static EmployeeResponse ToUserResponse(User employee)
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