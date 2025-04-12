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
    private readonly IPasswordHasher<Owner> _passwordHasher;


    public AccountService(DataContext dbContext)
    {
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher<Owner>();
    }
    
    public async Task<string?> GetPathAsync(int ownerId)
    {
        var owner = await FindOwnerById(ownerId);
        return owner.Path;
    }
    
    public async Task<string> SetPathAsync(PathRequest request, int ownerId)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Adresa nesmí být prázdná");

        var owner = await FindOwnerById(ownerId);
        
        string normalizedPath = request.Path.Trim().ToLowerInvariant();
        
        if (!Utils.IsPathValidate(normalizedPath))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Adresa obsahuje nepovolené znaky");

        // Kontrola, zda nová cesta není již obsazená (pokud se liší od té aktuální)
        if (!string.Equals(owner.Path, normalizedPath, StringComparison.OrdinalIgnoreCase) &&
            await _dbContext.Owners.AnyAsync(a => a.Path == normalizedPath))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Adresa je zabraná");
        }

        owner.Path = normalizedPath;
        await _dbContext.SaveChangesAsync();
        return owner.Path;
    }
    
    public async Task<bool> IsPathTakenAsync(string path)
    {
        return await _dbContext.Owners.AnyAsync(a => a.Path == path);
    }
    
    public async Task<AccountDescriptionResponse> GetAccountDescriptionAsync(string path)
    {
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(o => o.Path == path);

        if (owner is null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nenalezen");
        }
        
        return new AccountDescriptionResponse
        {
            Organization = owner.Organization,
            Description = owner.Description,
            Email = owner.Email,
        };
    }

    public async Task<AccountInfoResponse> GetAccountInfoAsync(int ownerId)
    {
        var owner = await FindOwnerById(ownerId);
        
        return new AccountInfoResponse()
        {
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Organization = owner.Organization,
            Description = owner.Description,
            Email = owner.Email
        };
    }

    public async Task<AccountInfoResponse> UpdateAccountInfoAsync(UpdateAccountInfoRequest request, int ownerId)
    {
        var owner = await FindOwnerById(ownerId);
        
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Jméno a příjmení nesmí být prázdné");
        }
        
        if (!Utils.IsValidEmail(request.Email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Email nemá validní formát");
        }
        
        if (await _dbContext.Owners.AnyAsync(a => a.Email == request.Email && a.Id != ownerId))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Email je již obsazen");
        }
        
        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.Organization = request.Organization;
        owner.Description = request.Description;
        owner.Email = request.Email;
        await _dbContext.SaveChangesAsync();
        
        return new AccountInfoResponse()
        {
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Organization = owner.Organization,
            Description = owner.Description,
            Email = owner.Email
        };
    }

    public async Task<bool> UpdatePasswordAsync(UpdatePasswordRequest request, int ownerId)
    {
        if (request.NewPassword.Length < 6)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Heslo musí mít alespoň 6 znaků");
        }
        
        var owner = await FindOwnerById(ownerId);
        
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
    
    private async Task<Owner> FindOwnerById(int ownerId)
    {
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (owner is null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nenalezen");
        }

        return owner;
    }
}