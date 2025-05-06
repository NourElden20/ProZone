using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace ProZone.Models.VMs
{
    public class ProductItemViewModel
    {
        public ObjectId ProductId { get; set; }
        [Display(Name ="Variation Name")]
        public string OptionValue { get; set; } = string.Empty;

        public decimal Price { get; set; }

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
        public IFormFile Photo { get; set; }
    }
}
