using Microsoft.EntityFrameworkCore;
using RepoLayer.Entity;

namespace DataAccessLayer.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Label> Labels { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Note-User Relationship
            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many Relationship between Notes and Labels
            modelBuilder.Entity<Note>()
                .HasMany(n => n.Labels)
                .WithMany(l => l.Notes)
                .UsingEntity(j => j.ToTable("NoteLabels"));
        }
    }
}