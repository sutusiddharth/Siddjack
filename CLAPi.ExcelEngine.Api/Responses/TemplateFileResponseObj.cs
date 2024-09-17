using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class TemplateFileResponseObj : TemplateFileDto
{
    public ObjectId Id { get; set; }
}
