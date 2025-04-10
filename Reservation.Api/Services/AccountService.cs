using System.Net;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Shared.Common;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class AccountService : IAccountService
{
    private readonly DataContext _dbContext;

    public AccountService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<string?> GetPathAsync(int ownerId)
    {
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (owner is null) { throw new CustomHttpException(HttpStatusCode.NotFound, "Uživatel nenalezen"); }
        return owner.Path;
    }
    
    public async Task<string> SetPathAsync(PathRequest request, int ownerId)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Adresa nesmí být prázdná");

        var owner = await _dbContext.Owners.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (owner is null)
            throw new CustomHttpException(HttpStatusCode.NotFound, "Uživatel nenalezen");
        
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
}