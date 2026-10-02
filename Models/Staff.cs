using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Staff
    {
        [Key]
        public int staff_ID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string staff_FName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string staff_LName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string staff_Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string staff_Passw { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(10, MinimumLength = 10,
    ErrorMessage = "Phone number must contain exactly 10 digits")]
        [RegularExpression(@"^[0-9]{10}$",
    ErrorMessage = "Phone number must contain exactly 10 digits")]
        public string staff_Phone { get; set; }

        [StringLength(50)]
        public string staff_Type { get; set; }

    }
}
