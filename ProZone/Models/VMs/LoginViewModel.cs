using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProZone.Models.VMs
{
    public class LoginViewModel
    {
        [BsonElement("email")]
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [BsonElement("password")]
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }      
    }
}
