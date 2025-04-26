using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Models;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services
{
    public class ReservationService : IReservationService
    {
        private readonly DataContext _dbContext;
        private readonly IEmailService _emailService;

        public ReservationService(DataContext dbContext, IEmailService emailService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        public async Task<ReservationResponse> CreateReservationAsync(ReservationCreateRequest request, int accountId)
        {
            if (request == null)
            {
                throw new CustomHttpException(HttpStatusCode.BadRequest, "Žádná událost nebyla poskytnuta");
            }

            var reservationEntity =
                (await _dbContext.Reservations.AddAsync(CreateReservationEntity(request, accountId))).Entity;

            await _dbContext.SaveChangesAsync();

            return MapToDto(reservationEntity);
        }

        public async Task<List<ReservationResponse>> CreateReservationsAsync(List<ReservationCreateRequest> requests,
            int accountId)
        {
            if (requests is null || requests.Count == 0)
            {
                throw new CustomHttpException(HttpStatusCode.BadRequest, "Žádná událost nebyla poskytnuta");
            }

            var reservationsDto = new List<ReservationResponse>();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var request in requests)
                {
                    var reservationEntity =
                        (await _dbContext.Reservations.AddAsync(CreateReservationEntity(request, accountId))).Entity;

                    reservationsDto.Add(MapToDto(reservationEntity));
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return reservationsDto;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new CustomHttpException(HttpStatusCode.InternalServerError,
                    "Chyba při vytváření událostí. Žádná událost nebyla vytvořena. Zkuste to prosím znovu.");
            }
        }

        public async Task<List<ReservationResponse>> GetReservationsByAccountAsync(int accountId)
        {
            if (!await _dbContext.Accounts.AnyAsync(o => o.Id == accountId))
            {
                throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nebyl nalezen");
            }

            var records = await _dbContext.Reservations
                .Where(r => r.AccountId == accountId)
                .Include(r => r.Customers)
                .ToListAsync();

            return records.Count == 0 ? [] : records.Select(MapToDto).ToList();
        }

        public async Task<List<ReservationResponse>> GetActiveReservationsByPathAsync(string path)
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(o => o.Path == path)
                          ?? throw new CustomHttpException(HttpStatusCode.NotFound, "Cesta nebyla nalezena");

            var records = await _dbContext.Reservations
                .Where(r => (r.AccountId == account.Id && r.IsAvailable && r.StartTime > DateTime.UtcNow))
                .Include(r => r.Customers)
                .ToListAsync();

            return records.Count == 0 ? [] : records.Select(MapToDto).ToList();
        }

        public async Task<ReservationResponse> GetReservationByPathAndIdAsync(string path, int accountId)
        {
            var owner = await _dbContext.Accounts.FirstOrDefaultAsync(o => o.Path == path)
                        ?? throw new CustomHttpException(HttpStatusCode.NotFound, "Cesta nebyla nalezena");

            var reservation = await _dbContext.Reservations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.Id == accountId && r.AccountId == owner.Id);

            if (reservation == null)
            {
                throw new CustomHttpException(HttpStatusCode.NotFound, "Událost nebyla nalezena");
            }

            return MapToDto(reservation);
        }

        public async Task<ReservationResponseWithCustomers> GetReservationWithUsersAsync(int accountId,
            int reservationId)
        {
            var owner = await _dbContext.Accounts
                            .Include(o => o.Reservations)
                            .ThenInclude(r => r.Customers)
                            .FirstOrDefaultAsync(o => o.Id == accountId)
                        ?? throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nebyl nalezen");

            var reservation = owner.Reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation == null)
            {
                throw new CustomHttpException(HttpStatusCode.BadRequest, "Událost nebyla nalezena");
            }

            return MapToWithUsersDto(reservation);
        }

        public async Task<ReservationResponse> SignUpForReservationAsync(int reservationId,
            ReservationSignUpRequest request, CultureInfo cultureInfo)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var reservation = await _dbContext.Reservations
                    .Include(r => r.Customers)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                if (reservation == null)
                    throw new CustomHttpException(HttpStatusCode.NotFound, "Událost nebyla nalezena");

                if (reservation.Customers.Count >= reservation.Capacity || !reservation.IsAvailable)
                    throw new CustomHttpException(HttpStatusCode.Locked, "K události se již není možné přihlásit");

                if (reservation.Customers.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                    throw new CustomHttpException(HttpStatusCode.Conflict,
                        "Uživatel je již přihlášen na tuto událost");

                var newUser = new Customer
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    CancellationCode = Guid.NewGuid().ToString(),
                    ReservationId = reservationId,
                    Note = request.Note,
                };

                reservation.Customers.Add(newUser);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _emailService.SendReservationConfirmationEmailWithUnsubscribeLinkAsync(
                    newUser.Email,
                    newUser.FirstName,
                    newUser.LastName,
                    reservation.Title,
                    reservation.StartTime,
                    reservation.EndTime - reservation.StartTime,
                    reservation.Id,
                    newUser.CancellationCode,
                    cultureInfo
                );

                return MapToDto(reservation);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new CustomHttpException(HttpStatusCode.InternalServerError,
                    "Chyba při přihlašování k události. Zkuste to prosím znovu.");
            }
        }

        public async Task<ReservationResponse> CancelReservationAsync(int reservationId, string cancellationCode,
            CultureInfo
                cultureInfo)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new CustomHttpException(HttpStatusCode.NotFound, "Událost nebyla nalezena");

            if (DateTime.UtcNow > reservation.StartTime - reservation.CancellationOffset)
            {
                throw new CustomHttpException(HttpStatusCode.Locked, "Rezervace již nelze zrušit");
            }

            var user = reservation.Customers.FirstOrDefault(u => u.CancellationCode == cancellationCode);
            if (user == null)
                throw new CustomHttpException(HttpStatusCode.BadRequest,
                    "Uživatel není již přihlášen na tuto událost");

            reservation.Customers.Remove(user);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendReservationCancellationByUserEmailAsync(user.Email, user.FirstName, user.LastName,
                reservation.Title, reservation.StartTime, cultureInfo);

            return MapToDto(reservation);
        }

        public async Task<ReservationResponse> UpdateReservationAsync(ReservationCreateRequest request,
            int reservationId)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.Id == reservationId);


            if (reservation == null)
                throw new CustomHttpException(HttpStatusCode.NotFound, "Událost nebyla nalezena");

            reservation.Capacity = request.Capacity;
            reservation.Title = request.Title;
            reservation.Description = request.Description;
            reservation.StartTime = request.Start;
            reservation.EndTime = request.End;
            reservation.IsAvailable = request.IsAvailable;
            reservation.CancellationOffset = request.CancellationOffset;
            reservation.CustomTimeZoneId = request.CustomTimeZoneId;

            await _dbContext.SaveChangesAsync();

            return MapToDto(reservation);
        }

        public async Task<bool> DeleteReservationAsync(int accountId, int reservationId)
        {
            var owner = await _dbContext.Accounts
                .Include(o => o.Reservations)
                .FirstOrDefaultAsync(o => o.Id == accountId);

            if (owner == null)
                throw new CustomHttpException(HttpStatusCode.NotFound, "Vlastník nebyl nalezen");

            var reservation = owner.Reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation == null)
                throw new CustomHttpException(HttpStatusCode.NotFound, "Událost nebyla nalezena");

            _dbContext.Reservations.Remove(reservation);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveUserFromReservationAsync(int reservationId, string userEmail,
            CultureInfo cultureInfo)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.Customers).Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new CustomHttpException(HttpStatusCode.NotFound, "Událost nebyla nalezena");

            var user = reservation.Customers.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                throw new CustomHttpException(HttpStatusCode.NotFound, "Uživatel nebyl nalezen");

            reservation.Customers.Remove(user);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendCustomerRemovedFromReservationEmailAsync(
                user.Email,
                user.FirstName,
                user.LastName,
                reservation.Title,
                reservation.StartTime,
                reservation.EndTime - reservation.StartTime,
                TimeZoneInfo.FindSystemTimeZoneById(reservation.CustomTimeZoneId),
                cultureInfo,
                reservation.Account.Organization,
                reservation.Account.Path
            );

            return true;
        }

        public async Task<bool> AccountOwnsReservationAsync(int accountId, int reservationId)
        {
            return await _dbContext.Reservations.AnyAsync(r => r.AccountId == accountId && r.Id == reservationId);
        }

        private static ReservationResponseWithCustomers MapToWithUsersDto(Models.Reservation reservation)
        {
            return new ReservationResponseWithCustomers()
            {
                Id = reservation.Id,
                Capacity = reservation.Capacity,
                CurrentCapacity = reservation.Customers.Count,
                Title = reservation.Title,
                Description = reservation.Description,
                Start = reservation.StartTime,
                End = reservation.EndTime,
                IsAvailable = reservation.IsAvailable,
                CancellationOffset = reservation.CancellationOffset,
                CustomTimeZoneId = reservation.CustomTimeZoneId,
                Users = reservation.Customers.Select(u => new CustomerResponse
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Note = u.Note ?? string.Empty,
                }).ToList()
            };
        }


        private static ReservationResponse MapToDto(Models.Reservation reservation)
        {
            return new ReservationResponse
            {
                Id = reservation.Id,
                Capacity = reservation.Capacity,
                CurrentCapacity = reservation.Customers?.Count ?? 0,
                Title = reservation.Title,
                Description = reservation.Description,
                Start = reservation.StartTime,
                End = reservation.EndTime,
                IsAvailable = reservation.IsAvailable,
                CancellationOffset = reservation.CancellationOffset,
                CustomTimeZoneId = reservation.CustomTimeZoneId
            };
        }

        private static Models.Reservation CreateReservationEntity(ReservationCreateRequest request, int accountId)
        {
            return new Models.Reservation
            {
                AccountId = accountId,
                Capacity = request.Capacity,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.Start,
                EndTime = request.End,
                IsAvailable = request.IsAvailable,
                CancellationOffset = request.CancellationOffset,
                CustomTimeZoneId = request.CustomTimeZoneId
            };
        }
    }
}