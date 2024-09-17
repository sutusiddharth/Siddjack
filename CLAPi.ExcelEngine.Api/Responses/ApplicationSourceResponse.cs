using CLAPi.ExcelEngine.Api.Models;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class ApplicationSourceResponse : PostApplicationSource
{
    public ObjectId Id { get; set; }
}