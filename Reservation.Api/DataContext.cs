using Microsoft.EntityFrameworkCore;
using Reservation.Api.Models;

namespace Reservation.Api;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Owner> Owners { get; set; }
    public DbSet<Models.Reservation> Reservations { get; set; }
    public DbSet<User> Users { get; set; }
}