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

    public DbSet<Owner> Owners { get; set; }
    public DbSet<Models.Reservation> Reservations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Device> Devices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Owner>()
            .HasMany(o => o.Reservations)
            .WithOne(r => r.Owner)
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Models.Reservation>()
            .HasMany(r => r.SignedUsers)
            .WithOne(u => u.Reservation)
            .HasForeignKey(u => u.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Najdeme všechny entity Owner, které jsou označené ke smazání.
        var deletedOwners = ChangeTracker.Entries<Owner>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity)
            .ToList();
    
        // Uložíme změny (smazání proběhne v databázi i kaskádově).
        int result = await base.SaveChangesAsync(cancellationToken);

        // Po dokončení smazání můžeme pro každého smazaného vlastníka volat notifikační logiku.
        foreach (var owner in deletedOwners)
        {
            // Příklad: Pošli e-mail vlastníkovi (pokud to dává smysl)
            await _emailService.SendDeleteAccountEmailAsync(owner.Email, owner.FirstName, owner.LastName);

            // Pokud chcete posílat e-mail také všem uživatelům, kteří byli přihlášeni k rezervacím,
            // je třeba iterovat přes rezervace a jejich SignedUsers.
            foreach (var reservation in owner.Reservations)
            {
                foreach (var user in reservation.SignedUsers)
                {
                    await _emailService.SendReservationCancellationByOwnerDeletingAccountEmailAsync(user.Email, user
                        .FirstName, user.LastName, reservation.Title, owner.Organization);
                }
            }
        }

        return result;
    }
}