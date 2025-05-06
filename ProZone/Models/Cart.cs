using MongoDB.Bson;

namespace ProZone.Models
{
    public class Cart
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
