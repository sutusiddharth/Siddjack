namespace CLAPi.ExcelEngine.Api.Models;

public class GetTemplateFileModel
{
    public string Folder_Nm { get; set; } = null!;     
    public string SubFolder_Nm { get; set; } = null!;
    public short? Actv_Ind { get; set; }
}
