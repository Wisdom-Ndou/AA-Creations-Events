using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class BookingAddOn
    {
        [Key]
        public int BookingAddOnId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string AddOnId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("AddOnId")]
        public virtual AddOn AddOn { get; set; }
    }
}