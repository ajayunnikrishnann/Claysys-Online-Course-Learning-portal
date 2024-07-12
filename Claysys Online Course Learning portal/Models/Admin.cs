using System.ComponentModel.DataAnnotations;

namespace Claysys_Online_Course_Learning_portal.Models
{
    public class Admin
    {
        public int AdminID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
