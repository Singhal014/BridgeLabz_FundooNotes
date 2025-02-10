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
            // Note-User Relationship (One-to-Many)
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

            // Many-to-Many Relationship for Note Collaborators 
            modelBuilder.Entity<Note>()
        .HasMany(n => n.Collaborators)
        .WithMany(u => u.CollaboratedNotes)
        .UsingEntity<Dictionary<string, object>>(
            "NoteCollaborators",
            j => j.HasOne<User>().WithMany().HasForeignKey("CollaboratorId").OnDelete(DeleteBehavior.NoAction),
            j => j.HasOne<Note>().WithMany().HasForeignKey("NoteId").OnDelete(DeleteBehavior.Cascade),
            j => j.HasKey("NoteId", "CollaboratorId") // Composite primary key
        );
        }
    }
}
