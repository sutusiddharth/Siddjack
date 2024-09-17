namespace CLAPi.ExcelEngine.Api.Models;

public class GetTemplateVersionModel
{
    public string Folder_Nm { get; set; } = null!; 
    public string SubFolder_Nm { get; set; } = null!;
    public string Template_Cd { get; set; } = null!;
    public short? Actv_Ind { get; set; }
}
