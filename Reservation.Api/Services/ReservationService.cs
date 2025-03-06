using System.Net;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Dtos;
using Reservation.Api.Models;
using ReservationApi;

namespace Reservation.Api.Services;

public class ReservationService : IReservationService
{
    private readonly DataContext _dbContext;

    public ReservationService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ReservationResponse Create(ReservationCreateRequest request, int ownerId)
    {
        var record = _dbContext.Reservations.Add(new Models.Reservation()
        {
            OwnerId = ownerId,
            Capacity = request.Capacity,
            Title = request.Title ?? string.Empty,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Date = request.Day,
            IsAvailable = request.IsAvailable,
        }).Entity;

        _dbContext.SaveChanges();

        var response = new ReservationResponse()
        {
            Id = record.Id,
            Capacity = record.Capacity,
            CurrentCapacity = 0,
            Title = record.Title,
            StartTime = record.StartTime,
            EndTime = record.EndTime,
            DayOfWeek = record.Date,
            IsAvailable = record.IsAvailable,
        };

        return response;
    }

    public List<ReservationResponse> Get(int ownerId)
    {
        var records = _dbContext.Reservations.Where(r => r.OwnerId == ownerId)
            .Include(reservation => reservation.SignedUsers).ToList();

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
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            DayOfWeek = r.Date,
            IsAvailable = r.IsAvailable,
        }).ToList();
    }

    public ReservationSignUpResponse SignUp(int reservationId, ReservationSignUpRequest user)
    {
        // Najít rezervaci
        var reservation = _dbContext.Set<Models.Reservation>()
            .Include(r => r.SignedUsers)
            .FirstOrDefault(r => r.Id == reservationId);

        if (reservation == null)
        {
            throw new CustomHttpException(HttpStatusCode.NotFound, "Reservation not found.");
        }

        if (reservation.SignedUsers.Count >= reservation.Capacity)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Reservation is full.");
        }

        // Ověřit, zda uživatel není již přihlášen
        if (reservation.SignedUsers.Any(u => u.Mail == user.Mail))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "User is already signed up for this reservation.");
        }

        // Přidat uživatele k rezervaci
        var newUser = new User()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Mail = user.Mail,
            Mobile = user.Mobile,
            ReservationId = reservationId
        };

        reservation.SignedUsers.Add(newUser);

        _dbContext.SaveChanges();

        // Vrátit odpověď
        return null;
    }
}