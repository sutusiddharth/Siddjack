namespace CLAPi.ExcelEngine.Api.Models;

public class PostGenerateOtp
{
    public string Flag { get; set; } = null!;
    public long Order_Id { get; set; }
    public string? Mobile_No { get; set; }
    public string? Email { get; set; }
}