namespace CLAPi.ExcelEngine.Api.DTOs;

public class ApplicationSourceDto : BaseDto
{
    public string Application_Nm { get; set; } = null!;
    public string Secret_Key { get; set; } = null!;
}
