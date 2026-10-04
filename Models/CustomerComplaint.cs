using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CustomerComplaint
    {
        [Key]
        public int ComplaintId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [Required, StringLength(60)]
        public string Category { get; set; }

        [Required, StringLength(120)]
        public string Subject { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; }

        [Required, StringLength(30)]
        public string Status { get; set; }

        [StringLength(2000)]
        public string AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}