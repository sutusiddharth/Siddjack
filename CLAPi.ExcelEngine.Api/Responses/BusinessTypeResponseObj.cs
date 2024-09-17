using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class BusinessTypeResponseObj : PostBusinessTypeDto
{
    public ObjectId Id { get; set; }
}
