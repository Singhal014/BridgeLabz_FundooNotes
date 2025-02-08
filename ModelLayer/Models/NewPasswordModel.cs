
using System.ComponentModel.DataAnnotations;
namespace ModelLayer.Models { 
public class NewPasswordModel
{
    [Required, MinLength(6)]
    public string NewPassword { get; set; }
}
}