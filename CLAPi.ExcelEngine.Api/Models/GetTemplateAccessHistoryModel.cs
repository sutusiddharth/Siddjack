namespace CLAPi.ExcelEngine.Api.Models;

public class GetTemplateAccessHistoryModel
{
    public string Folder_Nm { get; set; } = null!; 
    public string SubFolder_Nm { get; set; } = null!;
    public string Template_Cd { get; set; } = null!;
    public string? Record_Version { get; set; }
}
