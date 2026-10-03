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
        public string Cust_FName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Cust_LName { get; set; }

        public string Cust_UName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Phone number must contain exactly 9 digits.")]
        [RegularExpression(@"^[1-9][0-9]{8}$", ErrorMessage = "Phone number must contain exactly 9 digits and cannot start with 0.")]
        public string Cust_Phone { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Cust_Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Cust_Passw { get; set; }

        // Alternative contact fields
        [Display(Name = "Alternative Contact Name")]
        public string AlternativeContactName { get; set; }

        [Display(Name = "Relationship")]
        public string AlternativeContactRelationship { get; set; }

        [Display(Name = "Alternative Phone")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Alternative phone must contain exactly 9 digits.")]
        [RegularExpression(@"^[1-9][0-9]{8}$", ErrorMessage = "Alternative phone must contain exactly 9 digits and cannot start with 0.")]
        public string AlternativeContactPhone { get; set; }

        [Display(Name = "Alternative Email")]
        [EmailAddress(ErrorMessage = "Invalid alternative email address format")]
        public string AlternativeContactEmail { get; set; }

        // Terms acceptance tracking
        public int? AcceptedTermsID { get; set; }
        public DateTime? TermsAcceptedDate { get; set; }
    }
}


/*

*/