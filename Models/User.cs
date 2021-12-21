using System.ComponentModel.DataAnnotations;

public class Account
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }

}