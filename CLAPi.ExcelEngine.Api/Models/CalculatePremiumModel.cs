namespace CLAPi.ExcelEngine.Api.Models;

public class CalculatePremiumModel
{
    public string? Folder_Nm { get; set; }
    public string? SubFolder_Nm { get; set; }
    public string? Record_Version { get; set; }
    public string? Template_Cd { get; set; }
    public string Json { get; set; } = null!;
    public string? Source_System_Nm { get; set; }
    public string? Correlation_Id { get; set; }
    public bool Is_Doc { get; set; } = false;
}
