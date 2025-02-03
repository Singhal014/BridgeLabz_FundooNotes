using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class Fundoonote
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Color { get; set; }

        public bool IsArchived { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
