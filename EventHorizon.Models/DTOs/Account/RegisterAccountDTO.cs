namespace EventHorizon.Models.DTOs.Account;
public class RegisterAccountDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string UserName { get; set; } = null!;
}
