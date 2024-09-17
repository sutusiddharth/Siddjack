using MongoDB.Bson;

namespace CLAPi.ExcelEngine.Api.DTOs
{
    public class UpdateOtpItemsDto : GenerateOtpDto
    {
        public ObjectId Id { get; set; }
    }
}
