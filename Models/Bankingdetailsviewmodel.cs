using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
	public class Bankingdetailsviewmodel
	{
        public class BankingDetailsViewModel
        {

            [Required]
            [Display(Name = "Cardholder Name")]
            public string CardholderName { get; set; }

            [Required]
            [Display(Name = "Card Number")]
            [StringLength(19, MinimumLength = 13)]
            public string CardNumber { get; set; }

            [Required]
            [Display(Name = "Expiry Date")]
            [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY format.")]
            public string ExpiryDate { get; set; }

            [Required]
            [Display(Name = "CVV")]
            [StringLength(4, MinimumLength = 3)]
            public string Cvv { get; set; }

            [Required]
            [Display(Name = "Street Address")]
            public string StreetAddress { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            [Display(Name = "Postal Code")]
            [StringLength(4, MinimumLength = 4)]
            public string PostalCode { get; set; }

            // Set this from the booking/order before rendering the view,
            // e.g. BookingTotalDisplay = booking.Total.ToString("R#,##0", ...);
            public string BookingTotalDisplay { get; set; } = "R850";
        }


    }
}