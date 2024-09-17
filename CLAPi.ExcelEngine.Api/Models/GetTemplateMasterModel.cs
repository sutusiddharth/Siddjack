namespace CLAPi.ExcelEngine.Api.Models;

public class GetTemplateMasterModel
{
    public string Folder_Nm { get; set; } = null!; 
    public string SubFolder_Nm { get; set; } = null!;
    public string? Record_Version { get; set; }
    public string Template_Cd { get; set; } = null!;
    public string Master_Key { get; set; } = null!;
    public short? Actv_Ind { get; set; }
}
