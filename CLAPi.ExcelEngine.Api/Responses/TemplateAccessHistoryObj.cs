using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class TemplateAccessHistoryObj : TemplateAccessHistoryDto
{
    public ObjectId Id { get; set; }
}
