using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProZone.Models
{
    public class Order
    {
        [BsonId]
        public ObjectId Id { get; set; }  // MongoDB ObjectId
        public string UserId { get; set; }  // User who created the order
        public List<CartItem> Items { get; set; }  // List of cart items
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalAmount { get; set; }  // Total order value
        public string Status { get; set; } = "Pending";  // Order status (Pending, Delivered, Canceled)
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Order creation date
    }
}
