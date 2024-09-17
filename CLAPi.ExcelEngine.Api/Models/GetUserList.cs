namespace CLAPi.ExcelEngine.Api.Models;

public class GetUserList
{
    public string? User_Nm { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public short? Actv_Ind { get; set; }
}
