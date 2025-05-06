using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace ProZone.Models.VMs
{
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Field is required")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public string NameEn { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public string? DescriptionAr { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public string? DescriptionEn { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public IFormFile? Photo { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [Display(Name ="Category")]
        public ObjectId? CategoryId { get; set; }
        public IEnumerable<SelectListItem> categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
