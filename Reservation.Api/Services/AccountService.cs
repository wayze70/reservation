using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Models;
using Reservation.Shared.Common;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class AccountService : IAccountService
{
    private readonly DataContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;


    public AccountService(DataContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<List<AccountInfoResponse>> GetAccountsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Email je prázdný");

        if (!Utils.TryProcessEmail(email, out email))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Email nemá validní formát");

        var accounts = await _dbContext.Accounts
            .Include(a => a.Users)
            .Where(a => a.Users.Any(o => o.Email == email))
            .Select(a => new AccountInfoResponse
            {
                Organization = a.Organization,
                Description = a.Description,
                Identifier = a.Path,
                ContactEmail = a.ContactEmail
            })
            .ToListAsync();

        if (accounts.Count == 0)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Pro tento email neexistují žádné účty");

        return accounts;
    }
    
    public async Task<string?> GetPathAsync(int accountId)
    {
        var account = await FindAccountById(accountId);
        return account.Path;
    }
    
    public async Task<string> SetPathAsync(PathRequest request, int accountId)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Adresa nesmí být prázdná");

        var account = await FindAccountById(accountId);
        
        string normalizedPath = request.Path.Trim().ToLowerInvariant();
        
        if (!Utils.IsPathValidate(normalizedPath))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Adresa obsahuje nepovolené znaky");

        // Kontrola, zda nová cesta není již obsazená (pokud se liší od té aktuální)
        if (!string.Equals(account.Path, normalizedPath, StringComparison.OrdinalIgnoreCase) &&
            await _dbContext.Accounts.AnyAsync(a => a.Path == normalizedPath))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Adresa je zabraná");
        }

        account.Path = normalizedPath;
        await _dbContext.SaveChangesAsync();
        return account.Path;
    }
    
    public async Task<bool> IsPathTakenAsync(string path)
    {
        return await _dbContext.Accounts.AnyAsync(a => a.Path == path);
    }
    
    public async Task<AccountDescriptionResponse> GetAccountDescriptionAsync(string path)
    {
        var owner = await _dbContext.Accounts.FirstOrDefaultAsync(o => o.Path == path);

        if (owner is null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nenalezen");
        }
        
        return new AccountDescriptionResponse
        {
            Organization = owner.Organization,
            Description = owner.Description,
            ContactEmail = owner.ContactEmail,
        };
    }

    public async Task<AccountInfoResponse> GetAccountInfoAsync(int accountId)
    {
        var owner = await FindAccountById(accountId);
        
        return new AccountInfoResponse()
        {
            Organization = owner.Organization,
            Description = owner.Description,
            Identifier = owner.Path ?? string.Empty,
            ContactEmail = owner.ContactEmail
        };
    }

    public async Task<AccountInfoResponse> UpdateAccountInfoAsync(UpdateAccountInfoRequest request, int accountId)
    {
        var owner = await FindAccountById(accountId);
        
        owner.Organization = request.Organization;
        owner.Description = request.Description;
        owner.ContactEmail = request.ContactEmail;
        await _dbContext.SaveChangesAsync();
        
        return new AccountInfoResponse()
        {
            Organization = owner.Organization,
            Description = owner.Description,
            ContactEmail = owner.ContactEmail
        };
    }

    public async Task<bool> UpdatePasswordAsync(UpdatePasswordRequest request, int userId)
    {
        if (request.NewPassword.Length < 6)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Heslo musí mít alespoň 6 znaků");
        }

        var owner = await FindUserById(userId);

        var verificationResult = _passwordHasher.VerifyHashedPassword(owner, owner.PasswordHash, request.OldPassword);
        if (verificationResult != PasswordVerificationResult.Success &&
            verificationResult != PasswordVerificationResult.SuccessRehashNeeded)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Zadali jste špatné staré heslo");
        }

        owner.PasswordHash = _passwordHasher.HashPassword(owner, request.NewPassword);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAccountAsync(DeleteAccountRequest request, int accountId, int userId)
    {
        var account = await FindAccountById(accountId);
        var user = await FindUserById(userId);
        
        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task<Account> FindAccountById(int ownerId)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (account is null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Účet nenalezen");
        }

        return account;
    }
    
    private async Task<User> FindUserById(int ownerId)
    {
        var owner = await _dbContext.Users.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (owner is null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Účet nenalezen");
        }

        return owner;
    }
}