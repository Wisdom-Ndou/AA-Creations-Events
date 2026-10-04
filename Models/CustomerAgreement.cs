using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CustomerAgreement
    {
        [Key]
        public int AgreementId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        public bool TermsAccepted { get; set; }

        [Required, StringLength(20)]
        public string TermsVersion { get; set; }

        [Required]
        public DateTime AcceptedAt { get; set; }

        [Required, StringLength(20)]
        public string CookiePreference { get; set; }
    }
}
