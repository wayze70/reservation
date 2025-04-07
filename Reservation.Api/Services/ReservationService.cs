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
        if (request == null)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Žádná rezervace nebyla poskytnuta");
        }

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
            CustomTimeZoneId = request.CustomTimeZoneId
        })).Entity;

        await _dbContext.SaveChangesAsync();

        return new ReservationResponse()
        {
            Id = record.Id,
            Capacity = record.Capacity,
            CurrentCapacity = 0,
            Title = record.Title,
            Description = record.Description,
            Start = record.StartTime,
            End = record.EndTime,
            IsAvailable = record.IsAvailable,
            CancellationOffset = record.CancellationOffset,
            CustomTimeZoneId = record.CustomTimeZoneId
        };
    }

    public async Task<List<ReservationResponse>> CreateAsync(List<ReservationCreateRequest> listRequest, int ownerId)
    {
        if (listRequest is null || listRequest.Count == 0)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Žádná rezervace nebyla poskytnuta");
        }

        var reservations = new List<ReservationResponse>();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var request in listRequest)
            {
                // Přidáme rezervaci, ale nevoláme SaveChangesAsync uvnitř cyklu
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
                    CustomTimeZoneId = request.CustomTimeZoneId
                })).Entity;

                reservations.Add(new ReservationResponse()
                {
                    Id = record.Id,
                    Capacity = record.Capacity,
                    CurrentCapacity = 0,
                    Title = record.Title,
                    Description = record.Description,
                    Start = record.StartTime,
                    End = record.EndTime,
                    IsAvailable = record.IsAvailable,
                    CancellationOffset = record.CancellationOffset,
                    CustomTimeZoneId = record.CustomTimeZoneId
                });
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return reservations;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw new CustomHttpException(HttpStatusCode.InternalServerError, "Chyba při vytváření rezervací. Žádná rezervace nebyla vytvořena. Zkuste to prosím znovu.");
        }
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
            return [];
        }

        return records.Select(r => new ReservationResponse()
        {
            Id = r.Id,
            Capacity = r.Capacity,
            CurrentCapacity = r.SignedUsers.Count,
            Description = r.Description,
            Title = r.Title,
            Start = r.StartTime,
            End = r.EndTime,
            IsAvailable = r.IsAvailable,
            CancellationOffset = r.CancellationOffset,
            CustomTimeZoneId = r.CustomTimeZoneId
        }).ToList();
    }

    public async Task<List<ReservationResponse>> GetAsync(string path)
    {
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(owner => owner.Path == path) ??
                    throw new CustomHttpException(HttpStatusCode.NotFound, "Cesta nebyla nalezen");

        return await GetAsync(owner.Id);
    }

    public async Task<ReservationResponse> GetAsync(string path, int reservationId)
    {
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(owner => owner.Path == path) ??
                    throw new CustomHttpException(HttpStatusCode.NotFound, "Cesta nebyla nalezen");

        var record = await _dbContext.Reservations
            .Include(r => r.SignedUsers)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.OwnerId == owner.Id);

        if (record == null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Rezervace nebyla nalezena");
        }

        return new ReservationResponse()
        {
            Id = record.Id,
            Capacity = record.Capacity,
            CurrentCapacity = record.SignedUsers.Count,
            Title = record.Title,
            Description = record.Description,
            Start = record.StartTime,
            End = record.EndTime,
            IsAvailable = record.IsAvailable,
            CancellationOffset = record.CancellationOffset,
            CustomTimeZoneId = record.CustomTimeZoneId
        };
    }

    public async Task<ReservationResponse> SignUpAsync(int reservationId, ReservationSignUpRequest user)
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

        if (!reservation.IsAvailable)
        {
            throw new CustomHttpException(HttpStatusCode.Locked, "K rezervaci se není možné přihlásit");
        }

        // Ověřit, zda uživatel není již přihlášen
        if (reservation.SignedUsers.Any(u => u.Email == user.Email))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Uživatel je již přihlášen na tuto rezervaci");
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

        return new ReservationResponse()
        {
            Id = reservation.Id,
            Capacity = reservation.Capacity,
            CurrentCapacity = reservation.SignedUsers.Count,
            Title = reservation.Title,
            Description = reservation.Description,
            Start = reservation.StartTime,
            End = reservation.EndTime,
            IsAvailable = reservation.IsAvailable,
            CancellationOffset = reservation.CancellationOffset,
            CustomTimeZoneId = reservation.CustomTimeZoneId
        };
    }

    public async Task<ReservationResponse> CancelReservationAsync(int reservationId, string cancalationCode)
    {
        var reservation = await _dbContext.Set<Models.Reservation>()
            .Include(r => r.SignedUsers)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null) throw new CustomHttpException(HttpStatusCode.NotFound, "Rezervace nebyla nalezena");

        if (reservation.StartTime - reservation.CancellationOffset > DateTime.UtcNow)
        {
            throw new CustomHttpException(HttpStatusCode.Locked, "Rezervace již nelze zrušit");
        }

        var user = reservation.SignedUsers.FirstOrDefault(o => o.CancellationCode == cancalationCode);

        if (user == null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Uživatel není již přihlášen na tuto rezervaci");

        reservation.SignedUsers.Remove(user);

        await _dbContext.SaveChangesAsync();

        return new ReservationResponse()
        {
            Id = reservation.Id,
            Capacity = reservation.Capacity,
            CurrentCapacity = reservation.SignedUsers.Count,
            Title = reservation.Title,
            Description = reservation.Description,
            Start = reservation.StartTime,
            End = reservation.EndTime,
            IsAvailable = reservation.IsAvailable,
            CancellationOffset = reservation.CancellationOffset,
            CustomTimeZoneId = reservation.CustomTimeZoneId
        };
    }

    public async Task<ReservationResponseWithUser> GetWithUserAsync(int ownerId, int reservationId)
    {
        var owner = await _dbContext.Owners.Include(owner => owner.Reservations)
                        .ThenInclude(reservation => reservation.SignedUsers)
                        .FirstOrDefaultAsync(owner => owner.Id == ownerId) ??
                    throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastnik nebyl nalezen");

        var reservation = owner.Reservations.FirstOrDefault(o => o.Id == reservationId);

        if (reservation == null) throw new CustomHttpException(HttpStatusCode.BadRequest, "Rezervace nebyla nalezena");

        return new ReservationResponseWithUser()
        {
            Id = reservation.Id,
            Capacity = reservation.Capacity,
            CurrentCapacity = reservation.SignedUsers.Count,
            Title = reservation.Title,
            Description = reservation.Description,
            Start = reservation.StartTime,
            End = reservation.EndTime,
            IsAvailable = reservation.IsAvailable,
            CancellationOffset = reservation.CancellationOffset,
            CustomTimeZoneId = reservation.CustomTimeZoneId,
            Users = reservation.SignedUsers.Select(u => new UserResponse()
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Note = u.Note ?? string.Empty,
            }).ToList()
        };
    }

    public async Task<ReservationResponse> UpdateAsync(ReservationCreateRequest request, int reservationId)
    {
        var reservation = _dbContext.Reservations.Include(reservation => reservation.SignedUsers)
            .FirstOrDefault(r => r.Id == reservationId);

        if (reservation == null) throw new CustomHttpException(HttpStatusCode.NotFound, "Rezervace nebyla nalezena");

        reservation.Capacity = request.Capacity;
        reservation.Title = request.Title;
        reservation.Description = request.Description;
        reservation.StartTime = request.Start;
        reservation.EndTime = request.End;
        reservation.IsAvailable = request.IsAvailable;
        reservation.CancellationOffset = request.CancellationOffset;
        reservation.CustomTimeZoneId = request.CustomTimeZoneId;

        await _dbContext.SaveChangesAsync();

        return new ReservationResponse()
        {
            Id = reservation.Id,
            Capacity = reservation.Capacity,
            CurrentCapacity = reservation.SignedUsers.Count,
            Title = reservation.Title,
            Description = reservation.Description,
            Start = reservation.StartTime,
            End = reservation.EndTime,
            IsAvailable = reservation.IsAvailable,
            CancellationOffset = reservation.CancellationOffset,
            CustomTimeZoneId = reservation.CustomTimeZoneId
        };
    }

    public async Task<bool> DeleteAsync(int ownerId, int reservationId)
    {
        var owner = await _dbContext.Owners.Include(owner => owner.Reservations)
            .FirstOrDefaultAsync(owner => owner.Id == ownerId);

        if (owner == null) throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastnik nebyl nalezen");

        var reservation = owner.Reservations.FirstOrDefault(r => r.Id == reservationId);

        if (reservation == null) throw new CustomHttpException(HttpStatusCode.NotFound, "Rezervace nebyla nalezena");

        _dbContext.Reservations.Remove(reservation);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveUserFromReservationAsync(int reservationId, string userEmail)
    {
        var reservation = await _dbContext.Set<Models.Reservation>()
            .Include(r => r.SignedUsers)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null) throw new CustomHttpException(HttpStatusCode.NotFound, "Rezervace nebyla nalezena");

        var user = reservation.SignedUsers.FirstOrDefault(u => u.Email == userEmail);

        if (user == null) throw new CustomHttpException(HttpStatusCode.NotFound, "Uživatel nebyl nalezen");

        reservation.SignedUsers.Remove(user);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> OwnerOwnsReservation(int ownerId, int reservationId)
    {
        return await _dbContext.Reservations.AnyAsync(r => r.OwnerId == ownerId && r.Id == reservationId);
    }
}