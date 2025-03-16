using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RepoLayer.Entity
{
    public class User
    {
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [JsonIgnore]
        public bool IsVerified { get; set; } = false;

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Note> Notes { get; set; } = new List<Note>();

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Note> CollaboratedNotes { get; set; } = new List<Note>();
        
        public string RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
