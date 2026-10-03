using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class StaffTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(1000)]
        public string CompletionReason { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}