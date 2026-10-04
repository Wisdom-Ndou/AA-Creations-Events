using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class StaffComplaint
    {
        [Key]
        public int ComplaintId { get; set; }


        // ==========================================
        // STAFF MEMBER WHO REPORTED THE ISSUE
        // ==========================================

        [Required]
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; }


        // ==========================================
        // EVENT / BOOKING
        // ==========================================

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }


        // ==========================================
        // ISSUE INFORMATION
        // ==========================================

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; }


        // ==========================================
        // COMPLAINT STATUS
        // ==========================================

        [Required]
        [StringLength(20)]
        public string Status { get; set; }


        // ==========================================
        // DATES
        // ==========================================

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}