using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Admin
    {
        [Key]
        public int admin_ID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string admin_FName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string admin_LName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string admin_Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string admin_Passw { get; set; }
    }
}