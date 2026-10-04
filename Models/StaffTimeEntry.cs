using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class StaffTimeEntry
    {
        [Key]
        public int TimeEntryId { get; set; }


        // ==========================================
        // STAFF MEMBER
        // ==========================================

        [Required]
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; }


        // ==========================================
        // CLOCK TIMES
        // ==========================================

        [Required]
        public DateTime ClockInTime { get; set; }

        public DateTime? ClockOutTime { get; set; }


        // ==========================================
        // HOURS WORKED
        // ==========================================

        public double? HoursWorked { get; set; }
    }
}