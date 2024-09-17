namespace CLAPi.ExcelEngine.Api.Models;

public class TemplateFileDownloadModel
{
    public string Folder_Nm { get; set; } = string.Empty;
    public string SubFolder_Nm { get; set; } = string.Empty; 
    public string Template_Cd { get; set; } = string.Empty;        
    public string Download_Type { get; set; } = string.Empty; 
    public string Password { get; set; } = string.Empty;
    public string? Record_Version { get; set; }
    public short? Actv_Ind { get; set; }
}
