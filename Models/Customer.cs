using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Customer
    {
        [Key]
        public int Cust_ID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
       // [RegularExpression(@"^[a-zA-Z])+$", ErrorMessage = "First Name must only conatain letters.")]
        public string Cust_FName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
       // [RegularExpression(@"^[a-zA-Z])+$", ErrorMessage = "Last Name must only conatain letters.")]
        public string Cust_LName { get; set; }

        public string Cust_UName { get; set; } //UName is Username

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(10)]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Cust_Phone { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Cust_Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Cust_Passw { get; set; }
    }
}


/*

*/