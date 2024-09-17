using CLAPi.ExcelEngine.Api.DTOs;
using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.Responses;

public class UserListingObj : UserRegistrationDto
{
    public ObjectId Id { get; set; }
    public DateTime Lst_Updated_Dt { get; set; }
}
