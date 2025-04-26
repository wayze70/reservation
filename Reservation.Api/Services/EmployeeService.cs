using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Models;
using Reservation.Shared.Authorization;
using Reservation.Shared.Common;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly DataContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;

    public EmployeeService(DataContext context, IPasswordHasher<User> passwordHasher, IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
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

        if (await _context.Users.AnyAsync(e => e.Email == email && e.AccountId == accountId))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Zaměstnanec s tímto emailem již existuje");
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account is null) throw new CustomHttpException(HttpStatusCode.BadRequest, "Účet nenalezen");

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

        await _emailService.SendNewEmployeeWelcomeEmailAsync(employee.Email, employee.FirstName, employee.LastName,
            request.Password, account.Path, account.Organization, employee.Role);

        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int userId, EmployeeUpdateWithoutRoleRequest request,
        int accountId)
    {
        if (!Utils.TryProcessEmail(request.Email, out string email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatný formát emailu");
        }

        if (await _context.Users.AnyAsync(e => e.Email == email && e.AccountId == accountId && e.Id != userId))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Zaměstnanec s tímto emailem již existuje");
        }

        var employee = await GetEmployeeEntity(userId, accountId);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;

        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int userId, EmployeeUpdateRequest request, int accountId)
    {
        if (!Utils.TryProcessEmail(request.Email, out string email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatný formát emailu");
        }

        if (await _context.Users.AnyAsync(e => e.Email == email && e.AccountId == accountId && e.Id != userId))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Zaměstnanec s tímto emailem již existuje");
        }

        var employee = await _context.Users
            .Include(u => u.Account) // Přidáno načtení Account
            .FirstOrDefaultAsync(e => e.Id == userId && e.AccountId == accountId);

        if (employee == null)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Zaměstnanec nebyl nalezen");

        if (employee.Role == Role.Admin && request.Role != Role.Admin)
        {
            int adminCount = await _context.Users
                .CountAsync(u => u.AccountId == accountId && u.Role == Role.Admin);

            if (adminCount <= 1)
            {
                throw new CustomHttpException(
                    HttpStatusCode.Locked,
                    "Nelze změnit roli posledního administrátora účtu.");
            }
        }

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Role = request.Role;

        await _context.SaveChangesAsync();

        return ToUserResponse(employee);
    }

    public async Task DeleteEmployeeAsync(int userId, int accountId)
    {
        var employee = await _context.Users.Include(user => user.Account)
            .FirstOrDefaultAsync(e => e.Id == userId && e.AccountId == accountId);

        if (employee == null)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Zaměstnanec nebyl nalezen");

        if (employee.Role == Role.Admin)
        {
            int adminCount = await _context.Users
                .CountAsync(u => u.AccountId == accountId && u.Role == Role.Admin);

            if (adminCount <= 1)
            {
                throw new CustomHttpException(
                    HttpStatusCode.Locked,
                    "Nelze smazat posledního administrátora účtu.");
            }
        }

        var devices = await _context.Devices
            .Where(d => d.UserId == userId)
            .ToListAsync();

        if (devices.Count != 0)
        {
            _context.Devices.RemoveRange(devices);
        }

        _context.Users.Remove(employee);
        await _context.SaveChangesAsync();
        await _emailService.SendEmployeeDeletionEmailAsync(employee.Email, employee.FirstName, employee.LastName,
            employee.Account.Organization, employee.Account.Path);
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