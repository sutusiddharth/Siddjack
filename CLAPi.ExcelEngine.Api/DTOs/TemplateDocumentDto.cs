namespace CLAPi.ExcelEngine.Api.DTOs;

public class TemplateDocumentDto : BaseDto
{
    public string Folder_Nm { get; set; } = null!;
    public string SubFolder_Nm { get; set; } = null!;
    public string Template_Cd { get; set; } = null!;
    public string Record_Version { get; set; } = string.Empty;
    public string Folder_Path_Nm { get; set; } = string.Empty;
    public string Template_Nm { get; set; } = string.Empty;
    public string Template_Type { get; set; } = string.Empty;
    public string Folder_Path { get; set; } = string.Empty;
    public int? Filter_Year { get; set; }
    public string Correlation_Id { get; set; } = string.Empty;
}
