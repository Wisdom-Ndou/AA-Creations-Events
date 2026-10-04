using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Occasion { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string EventTime { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        public string Notes { get; set; }

        [Required]
        public string PackageId { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }

        public virtual ICollection<BookingAddOn> BookingAddOns { get; set; }
        = new List<BookingAddOn>();

        [Required]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }

        // Payment tracking. Card details are never stored; only business payment state is persisted.
        public decimal AmountPaid { get; set; }

        public DateTime? BalanceDueDate { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; }

        public decimal CancellationCharge { get; set; }

        public decimal RefundAmount { get; set; }

        [StringLength(30)]
        public string TermsVersion { get; set; }

        public DateTime? TermsAcceptedAt { get; set; }

        [NotMapped]
        public decimal BalanceOutstanding
        {
            get { return Math.Max(0m, TotalPrice - AmountPaid); }
        }

    }
}