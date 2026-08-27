using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AddOn
    {
        [Key]
        [StringLength(50)]
        public string AddOnId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public virtual ICollection<BookingAddOn> BookingAddOns { get; set; }
    }
}