using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.DTOs;

public class UpdateTemplateFileDto : TemplateFileDto
{
    public ObjectId Id { get; set; }
}
