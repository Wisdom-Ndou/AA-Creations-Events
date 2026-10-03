using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class TermsAndConditions
    {
        [Key]
        public int Terms_ID { get; set; }

        [Required]
        [MaxLength(20)]
        public string Version { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}