using System.ComponentModel.DataAnnotations;

public class NewPasswordModel
{
    [Required, MinLength(6)]
    public string NewPassword { get; set; }
}