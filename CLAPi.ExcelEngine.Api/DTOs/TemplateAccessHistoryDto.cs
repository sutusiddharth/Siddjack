namespace CLAPi.ExcelEngine.Api.DTOs;

public class TemplateAccessHistoryDto : BaseDto
{
    public string Folder_Nm { get; set; } = null!;
    public string SubFolder_Nm { get; set; } = null!;
    public string Template_Cd { get; set; } = null!;
    public string Record_Version { get; set; } = null!;
    public string Input_Json { get; set; } = null!;
    public string Request_Json { get; set; } = null!;
    public string Response_Json { get; set; } = null!;
    public string Correlation_Id { get; set; } = null!;
}
