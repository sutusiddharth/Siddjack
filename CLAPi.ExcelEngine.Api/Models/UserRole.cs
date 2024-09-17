using MongoDB.Bson.Serialization.Attributes;

namespace CLAPi.ExcelEngine.Api.Models
{
    public class UserRole
    {
        [BsonId]
        public Guid Id { get; set; }
        public List<Guid> Roles { get; set; } = null!;
    }
}
