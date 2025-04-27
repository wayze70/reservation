using Microsoft.EntityFrameworkCore;
using Reservation.Api.Models;
using Reservation.Api.Services;

namespace Reservation.Api;

public class DataContext : DbContext
{
    private readonly IEmailService _emailService;

    public DataContext(DbContextOptions<DataContext> options, IEmailService emailService) : base(options)
    {
        _emailService = emailService;
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Models.Reservation> Reservations { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ReservationReminder> ReservationReminders { get; set; }

    public override int SaveChanges()
        => SaveChangesAsync().GetAwaiter().GetResult();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account -> Users
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Users)
            .WithOne(o => o.Account)
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Devices
        modelBuilder.Entity<User>()
            .HasMany(u => u.Devices)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Account -> Reservations
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Reservations)
            .WithOne(r => r.Account)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Reservation -> Customers
        modelBuilder.Entity<Models.Reservation>()
            .HasMany(r => r.Customers)
            .WithOne(u => u.Reservation)
            .HasForeignKey(u => u.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var deletedAccounts = ChangeTracker.Entries<Account>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity)
            .ToList();

        foreach (var account in deletedAccounts)
        {
            await Entry(account)
                .Collection(a => a.Users)
                .LoadAsync(cancellationToken);

            await Entry(account)
                .Collection(a => a.Reservations)
                .LoadAsync(cancellationToken);

            foreach (var reservation in account.Reservations)
            {
                await Entry(reservation)
                    .Collection(r => r.Customers)
                    .LoadAsync(cancellationToken);
            }
        }

        int result = await base.SaveChangesAsync(cancellationToken);

        foreach (var account in deletedAccounts)
        {
            foreach (var owner in account.Users)
            {
                await _emailService.SendDeleteAccountEmailAsync(
                    owner.Email,
                    owner.FirstName,
                    owner.LastName,
                    account.Path);
            }

            foreach (var reservation in account.Reservations)
            {
                foreach (var user in reservation.Customers)
                {
                    await _emailService.SendReservationCancellationByOwnerDeletingAccountEmailAsync(
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        reservation.Title,
                        account.Organization);
                }
            }
        }

        return result;
    }
}