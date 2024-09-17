using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class TemplateDocumentResponseObj : TemplateDocumentDto
{
    public ObjectId Id { get; set; }
    public string File_Url { get; set; } = string.Empty;
}
