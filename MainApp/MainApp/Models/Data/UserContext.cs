using Microsoft.EntityFrameworkCore;

namespace MainApp.Models
{
    // Database context for interaction between
    // the crew and user tables in the database and the application
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Admin> Crew { get; set; } = null!;


        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
