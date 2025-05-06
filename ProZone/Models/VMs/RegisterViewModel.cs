using System.ComponentModel.DataAnnotations;

namespace ProZone.Models.VMs
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Minimum 8 characters length")]
        public string Password { get; set; }

        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; }
    }
}
