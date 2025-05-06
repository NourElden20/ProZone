using MongoDB.Bson.Serialization.Attributes;

namespace ProZone.Models
{
    public class LocalizedName
    {
        [BsonElement("en")]
        public string en { get; set; } = "";
        [BsonElement("ar")]
        public string ar { get; set; } = "";
    }
}
