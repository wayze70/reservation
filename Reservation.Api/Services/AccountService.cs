using System.Net;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class AccountService : IAccountService
{
    private readonly DataContext _dbContext;

    public AccountService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task SetPathAsync(PathRequest request, int ownerId)
    {
        if (await IsPathTakenAsync(request)) { throw new CustomHttpException(HttpStatusCode.Conflict, "Adresa je zabraná"); }
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(a => a.Id == ownerId);
        if (owner is null) { throw new CustomHttpException(HttpStatusCode.NotFound, "Uživatel nenalezen"); }
        owner.Path = request.Path;
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<bool> IsPathTakenAsync(PathRequest request)
    {
        return await _dbContext.Owners.AnyAsync(a => a.Path == request.Path);
    }
}