using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class PostOtpObj : GenerateOtpDto
{
    public ObjectId Id { get; set; }
}
