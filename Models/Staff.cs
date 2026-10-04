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
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "First Name must only contain letters.")]
        public string staff_FName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Last Name must only contain letters.")]
        public string staff_LName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string staff_Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string staff_Passw { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Phone number must contain exactly 9 digits")]
        [RegularExpression(@"^[1-9][0-9]{8}$", ErrorMessage = "Phone number must contain exactly 9 digits and cannot start with 0")]
        public string staff_Phone { get; set; }

        [StringLength(50)]
        public string staff_Type { get; set; }

        // Municipality/city this staff member is responsible for.
        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string staff_City { get; set; }

    }
}