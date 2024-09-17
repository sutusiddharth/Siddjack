namespace CLAPi.ExcelEngine.Api.DTOs;

public class UserRegistrationDto:BaseDto
{
    public string First_Nm { get; set; } = null!;
    public string Last_Nm { get; set; } = string.Empty;
    public string User_Nm { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
