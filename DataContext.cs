using Microsoft.EntityFrameworkCore;
using ReservationApi.Models;

namespace ReservationApi;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<User> User { get; set; }
}