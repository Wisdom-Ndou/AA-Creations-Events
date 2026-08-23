using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Admin
    {
        [Key]
        public int admin_ID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression(@"^[a-zA-Z])+$", ErrorMessage = "First Name must only conatain letters.")]
        public string admin_FName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[a-zA-Z])+$", ErrorMessage = "Last Name must only conatain letters.")]
        public string admin_LName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string admin_Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.[^a-zA-Z0-9]).$", ErrorMessage = "Password must contain at least one special character.")]
        [DataType(DataType.Password)]
        public string admin_Passw { get; set; }
    }
}