using MongoDB.Bson.Serialization.Attributes;

namespace CLAPi.ExcelEngine.Api.DTOs;

[BsonIgnoreExtraElements]
public class TemplateFileDto : BaseDto
{
    public string Folder_Nm { get; set; } = string.Empty;
    public string SubFolder_Nm { get; set; } = string.Empty;
    public string Template_Cd { get; set; } = string.Empty;
    public string Record_Version { get; set; } = string.Empty;
    public string Input_Data { get; set; } = string.Empty;
    public string Output_Data { get; set; } = string.Empty;
    public string List_Data { get; set; } = string.Empty;
    public string Upload_Type { get; set; } = string.Empty;
    public DateOnly? Effective_From { get; set; }
    public DateOnly? Effective_Upto { get; set; }
    public string Release_Note { get; set; } = string.Empty;
    public int? Sheet_Count { get; set; }
    public int? Hidden_Sheet_Count { get; set; }
    public int? Doc_Count { get; set; }
    public decimal? Size { get; set; }
    public string? Correlation_Id { get; set; }
    public string Is_Api { get; set; } = string.Empty;
}
