using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApplication1.Models
{
    public class OtpVerification
    {
        [Key]
        public int OtpId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        [StringLength(255)]
        public string OtpHash { get; set; }

        [Required]
        [StringLength(20)]
        public string DeliveryMethod { get; set; }

        [Required]
        [StringLength(30)]
        public string Purpose { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public int FailedAttempts { get; set; }
    }
}