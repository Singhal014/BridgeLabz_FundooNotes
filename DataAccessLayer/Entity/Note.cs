using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        [ForeignKey("User")] // Define the foreign key to the User table
        public int CreatedBy { get; set; } // This will link to User's Id

        [JsonIgnore]
        public virtual User User { get; set; } // Navigation property to access the User object

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

