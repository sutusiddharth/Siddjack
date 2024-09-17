namespace CLAPi.ExcelEngine.Api.Models;
public class GenerateApiModel
{
    public string Folder_Nm { get; set; } = string.Empty;
    public string SubFolder_Nm { get; set; } = string.Empty;
    public string Upload_Type { get; set; } = string.Empty;
    public DateOnly? Effective_From { get; set; }
    public DateOnly? Effective_Upto { get; set; }
    public string Release_Note { get; set; } = string.Empty;
    public string File_Nm { get; set; } = string.Empty;
    public string File_Stream { get; set; } = string.Empty;
    public string? Correlation_Id { get; set; }
    public string Is_Api { get; set; } = string.Empty;
}