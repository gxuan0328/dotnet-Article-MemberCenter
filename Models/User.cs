using System.ComponentModel.DataAnnotations;

public class Account
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}
public class UserDetail
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Status { get; set; }
}