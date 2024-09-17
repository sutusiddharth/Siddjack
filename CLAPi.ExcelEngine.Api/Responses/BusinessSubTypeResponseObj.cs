using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class BusinessSubTypeResponseObj : PostBusinessSubTypeDto
{
    public ObjectId Id { get; set; }
}
