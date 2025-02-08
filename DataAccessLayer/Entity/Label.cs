using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RepoLayer.Entity
{
    public class Label
    {
        internal object LabelName;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CreatedBy { get; set; }

        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}