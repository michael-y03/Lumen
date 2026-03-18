using Lumen.Domain;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure
{
    public class LumenDbContext : DbContext
    {
        public LumenDbContext(DbContextOptions<LumenDbContext> options) : base(options)
        {
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Album> Albums { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<Photo>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Photos);

            modelBuilder.Entity<Photo>()
                .HasMany(p => p.Albums)
                .WithMany(a => a.Photos);
        }
    }
}