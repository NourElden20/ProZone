using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CartItem
{
    public ObjectId ProductItemId { get; set; }
    public string SKU { get; set; }
    public int Quantity { get; set; }
    [BsonRepresentation(BsonType.Decimal128)]

    public decimal Price { get; set; }
    public string Photo { get; set; }
}
