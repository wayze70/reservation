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
        if (await IsPathTakenAsync(request.Path)) { throw new CustomHttpException(HttpStatusCode.Conflict, "Adresa je zabraná"); }
        if (!Utils.IsPathValidate(request.Path)) throw new CustomHttpException(HttpStatusCode.BadRequest, "Adresa absahuje nepovolené znaky");
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (owner is null) { throw new CustomHttpException(HttpStatusCode.NotFound, "Uživatel nenalezen"); }
        owner.Path = request.Path;
        await _dbContext.SaveChangesAsync();
        return owner.Path;
    }
    
    public async Task<bool> IsPathTakenAsync(string path)
    {
        return await _dbContext.Owners.AnyAsync(a => a.Path == path);
    }
}