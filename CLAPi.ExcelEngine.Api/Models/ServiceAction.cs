using MongoDB.Bson.Serialization.Attributes;

namespace CLAPi.ExcelEngine.Api.Models
{
    public class RoleAction
    {
        [BsonId]
        public Guid Id { get; set; }
        public string RoleName { get; set; } = null!;
        public List<PagePermission> PagePermissions { get; set; } = null!;
    }
}
