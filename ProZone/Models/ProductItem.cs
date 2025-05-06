using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProZone.Models
{
    public class ProductItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("sku")]
        public string? SKU { get; set; }

        [Required]
        [BsonElement("variationName")]
        public string VariationName { get; set; } = string.Empty;

        [BsonElement("photo")]
        public string? Photo { get; set; }

        [Required]
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]

        public decimal Price { get; set; }

        [Required]
        [BsonElement("stockQuantity")]
        public int StockQuantity { get; set; }


        [BsonIgnore]
        public int ReorderThreshold { get; set; } = 3;

        [BsonIgnore]
        public decimal DiscountVariantPrice { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("productId")]
        public ObjectId ProductId { get; set; }

        
    }
}
