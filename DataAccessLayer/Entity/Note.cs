using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace RepoLayer.Entity
{
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Color { get; set; } = "DefaultColor";

        public bool IsArchived { get; set; }

        public bool IsTrashed { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("User")]
        public int CreatedBy { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Label> Labels { get; set; } = new List<Label>();

        public ICollection<User> Collaborators { get; set; } = new List<User>();
    }
}

