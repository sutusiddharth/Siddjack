using MongoDB.Bson.Serialization.Attributes;

namespace CLAPi.ExcelEngine.Api.Models
{
    public class PagePermission
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid PageDetailId { get; set; }  // This will link to the PageDetail table
        public bool CanAccess { get; set; }
        public Dictionary<string, bool> Actions { get; set; } = null!;
    }
}
