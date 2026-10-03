using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class TermsAndConditions
    {
        [Key]
        public int Terms_ID { get; set; }

        [Required, StringLength(20)]
        public string Version { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
