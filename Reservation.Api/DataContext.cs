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
    public DbSet<User> Users { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Owner> Owners { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account -> Owner relationship
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Owners)
            .WithOne(o => o.Account)
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Account -> Device relationship
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Devices)
            .WithOne(d => d.Account)
            .HasForeignKey(d => d.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Account -> Reservation relationship
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Reservations)
            .WithOne(r => r.Account)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Reservation -> SignedUsers relationship
        modelBuilder.Entity<Models.Reservation>()
            .HasMany(r => r.SignedUsers)
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
            // Load related data before deletion
            await Entry(account)
                .Collection(a => a.Owners)
                .LoadAsync(cancellationToken);

            await Entry(account)
                .Collection(a => a.Reservations)
                .LoadAsync(cancellationToken);

            foreach (var reservation in account.Reservations)
            {
                await Entry(reservation)
                    .Collection(r => r.SignedUsers)
                    .LoadAsync(cancellationToken);
            }
        }

        int result = await base.SaveChangesAsync(cancellationToken);

        foreach (var account in deletedAccounts)
        {
            // Notify owners
            foreach (var owner in account.Owners)
            {
                await _emailService.SendDeleteAccountEmailAsync(
                    owner.Email,
                    owner.FirstName,
                    owner.LastName);
            }

            // Notify users signed up for reservations
            foreach (var reservation in account.Reservations)
            {
                foreach (var user in reservation.SignedUsers)
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