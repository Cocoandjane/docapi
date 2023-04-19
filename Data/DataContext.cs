using DocShareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DocShareAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
      : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
  
            builder.Entity<User>().ToTable("user");
            builder.Entity<Document>().ToTable("document");
            builder.Entity<Invitation>().ToTable("invitation");

        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Invitation> Invitations => Set<Invitation>();

    }
}