using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProZone.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [Required]
        [BsonElement("name")]
        public LocalizedName Name { get; set; }

        [BsonElement("description")]
        public LocalizedDescription? Description { get; set; }

        [BsonElement("mainPhoto")]
        public string? Photo { get; set; }

        [Required]
        [Range(0, 100000)]
        [BsonElement("minPrice")]
        [BsonRepresentation(BsonType.Decimal128)]

        public decimal MinPrice { get; set; } = 5000;

        [BsonElement("soldTimes")]
        public int SoldTimes { get; set; }

        // Foreign key reference to Category (ObjectId type)
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId CategoryId { get; set; }

        // Navigation property to Category (this allows you to load the related category)
        [BsonIgnore] // This is ignored for MongoDB storage but used in application logic
        public Category Category { get; set; }

        [BsonElement("productItemIds")]
        public List<ObjectId> ProductItemIds { get; set; } = new();
    }
}
