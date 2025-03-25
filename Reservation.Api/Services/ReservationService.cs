using System.Net;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class ReservationService : IReservationService
{
    private readonly DataContext _dbContext;

    public ReservationService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReservationResponse> CreateAsync(ReservationCreateRequest request, int ownerId)
    {
        var record = (await _dbContext.Reservations.AddAsync(new Models.Reservation()
        {
            OwnerId = ownerId,
            Capacity = request.Capacity,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.Start,
            EndTime = request.End,
            IsAvailable = request.IsAvailable,
            CancellationOffset = request.CancellationOffset,
            TimeDisplayMode = request.TimeDisplayMode,
            CustomTimeZone = request.CustomTimeZone
        })).Entity;

        await _dbContext.SaveChangesAsync();

        var response = new ReservationResponse()
        {
            Id = record.Id,
            Capacity = record.Capacity,
            CurrentCapacity = 0,
            Title = record.Title,
            Start = record.StartTime,
            End = record.EndTime,
            IsAvailable = record.IsAvailable,
            CancellationOffset = record.CancellationOffset,
            TimeDisplayMode = record.TimeDisplayMode,
            CustomTimeZone = record.CustomTimeZone
        };

        return response;
    }

    public async Task<List<ReservationResponse>> GetAsync(int ownerId)
    {
        if (!_dbContext.Owners.Any(o => o.Id == ownerId))
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nebyl nalezen");
        }

        var records = await _dbContext.Reservations
            .Where(r => r.OwnerId == ownerId)
            .Include(r => r.SignedUsers)
            .ToListAsync();

        if (records.Count == 0)
        {
            return new List<ReservationResponse>();
        }

        return records.Select(r => new ReservationResponse()
        {
            Id = r.Id,
            Capacity = r.Capacity,
            CurrentCapacity = r.SignedUsers.Count,
            Title = r.Title,
            Start = r.StartTime,
            End = r.EndTime,
            IsAvailable = r.IsAvailable,
            CancellationOffset = r.CancellationOffset,
            TimeDisplayMode = r.TimeDisplayMode,
            CustomTimeZone = r.CustomTimeZone
        }).ToList();
    }

    public async Task<ReservationSignUpResponse> SignUpAsync(int reservationId, ReservationSignUpRequest user)
    {
        // Najít rezervaci
        var reservation = await _dbContext.Set<Models.Reservation>()
            .Include(r => r.SignedUsers)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Rezerace nebyla nalezena");
        }

        if (reservation.SignedUsers.Count >= reservation.Capacity)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Rezerace je již plná");
        }

        // Ověřit, zda uživatel není již přihlášen
        if (reservation.SignedUsers.Any(u => u.Email == user.Email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Uživatel je již přihlášen na tuto rezervaci");
        }

        // Přidat uživatele k rezervaci s unikátním CancellationCode
        var newUser = new Models.User()
        {
            FirstName = user.FirstName, 
            LastName = user.LastName,
            Email = user.Email, 
            CancellationCode = Guid.NewGuid().ToString(),
            ReservationId = reservationId
        };

        reservation.SignedUsers.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return new ReservationSignUpResponse()
        {
            IsSuccess = true
        };
    }
}