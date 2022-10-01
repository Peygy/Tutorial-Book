using Microsoft.EntityFrameworkCore;

namespace MainApp.Models
{
    // Database context for interaction between
    // the parts tables in the database and the application
    public class TopicsContext : DbContext
    {
        // Section -> Subsection -> Chapter -> Subchapter -> Post(Content)

        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Subsection> Subsections { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;
        public DbSet<Subchapter> Subchapters { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;


        public TopicsContext(DbContextOptions<TopicsContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
