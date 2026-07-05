using Microsoft.EntityFrameworkCore;
using Restaurant.API.Models;

namespace Restaurant.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // This line tells EF Core to create a 'Users' table based on our User model
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);            
        }
    }
}