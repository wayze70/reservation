using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.Models;
using Reservation.Api.Services;

namespace Reservation.Api.Workers;

public class ReservationReminderWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationReminderWorker> _logger;

    public ReservationReminderWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendReminders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při kontrole a odesílání připomenutí rezervací");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CheckAndSendReminders(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var now = DateTime.UtcNow;
        var tomorrow = now.AddHours(24);

        var reservations = await dbContext.Reservations
            .Include(r => r.Customers)
            .Where(r => r.StartTime > now && 
                        r.StartTime < tomorrow)
            .ToListAsync(stoppingToken);

        foreach (var reservation in reservations)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(reservation.CustomTimeZoneId);

            foreach (var customer in reservation.Customers)
            {
                bool reminderExists = await dbContext.ReservationReminders
                    .AnyAsync(r => r.ReservationId == reservation.Id &&
                                   r.CustomerEmail == customer.Email,
                        stoppingToken);

                if (reminderExists)
                    continue;

                try
                {
                    await emailService.SendReservationReminderEmailAsync(
                        customer.Email,
                        customer.FirstName,
                        customer.LastName,
                        reservation.Title,
                        reservation.StartTime,
                        reservation.EndTime - reservation.StartTime,
                        reservation.Id,
                        timeZoneInfo,
                        CultureInfo.GetCultureInfo("cs-CZ")
                    );

                    await dbContext.ReservationReminders.AddAsync(new ReservationReminder
                    {
                        ReservationId = reservation.Id,
                        CustomerEmail = customer.Email,
                        SentAt = DateTime.UtcNow
                    }, stoppingToken);

                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Odesláno připomenutí pro rezervaci {ReservationId} uživateli {UserEmail}",
                        reservation.Id,
                        customer.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Chyba při odesílání připomenutí pro rezervaci {ReservationId} uživateli {UserEmail}",
                        reservation.Id,
                        customer.Email);
                }
            }
        }
    }
}