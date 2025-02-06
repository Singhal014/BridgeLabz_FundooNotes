using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the foreign key relationship explicitly
            modelBuilder.Entity<Note>()
                .HasOne(n => n.User) // Navigation property to User
                .WithMany(u => u.Notes) // One User can have many Notes
                .HasForeignKey(n => n.CreatedBy) // Foreign key property
                .OnDelete(DeleteBehavior.Cascade); // When User is deleted, related Notes are also deleted
        }
    }
}
