using MongoDB.Bson.Serialization.Attributes;

namespace CLAPi.ExcelEngine.Api.Models
{
    public class PageDetail
    {
        [BsonId]
        public Guid Id { get; set; }
        public string PageName { get; set; } = null!;
        public bool CanAccess { get; set; }
        public Dictionary<string, bool> Actions { get; set; } = null!;
    }
}
