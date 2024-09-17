namespace CLAPi.ExcelEngine.Api.Models;

public class GetTemplateDocument
{
    public string Folder_Nm { get; set; } = null!;
    public string SubFolder_Nm { get; set; } = null!;
    public string Template_Cd { get; set; } = null!;
    public string? Template_Nm { get; set; }
    public string? Record_Version { get; set; }
    public short? Actv_Ind { get; set; }
}
