namespace CLAPi.ExcelEngine.Api.Models;

public class CopyApiModel
{
    public string Template_Cd { get; set; } = null!;
    public string Folder_Nm { get; set; } = null!; 
    public string SubFolder_Nm { get; set; } = null!;
    public string? Record_Version { get; set; }
    public short? Actv_Ind { get; set; }
}
