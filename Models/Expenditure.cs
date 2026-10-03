using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Expenditure
    {
        [Key]
        public int ExpenditureId { get; set; }

        [Required, StringLength(120)]
        public string Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [StringLength(80)]
        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
