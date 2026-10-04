using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebApplication1.Models
{
    public class AdminDashboardViewModel
    {
        // ===============================
        // SUMMARY WIDGETS
        // ===============================

        public int TotalBookings { get; set; }

        public int UpcomingEvents { get; set; }

        public int CancelledBookings { get; set; }

        public int PendingBookings { get; set; }

        public decimal ConfirmedRevenue { get; set; }

        public int TotalStaff { get; set; }
        public int AvailableStaff { get; set; }
        public int BusyStaff { get; set; }


        // ===============================
        // CUSTOMER STATISTICS
        // ===============================

        public int RegisteredCustomers { get; set; }


        // ===============================
        // BOOKINGS
        // ===============================

        public List<Booking> RecentBookings { get; set; }
            = new List<Booking>();


        // ===============================
        // RECENT ACTIVITY
        // ===============================

        public List<DashboardActivity> RecentActivities { get; set; }
            = new List<DashboardActivity>();


        // ===============================
        // WEEKLY LOAD
        // ===============================

        public int WeddingLoad { get; set; }

        public int CorporateLoad { get; set; }

        public int BirthdayLoad { get; set; }
    }


    public class DashboardActivity
    {
        public string Description { get; set; }

        public DateTime ActivityDate { get; set; }
    }
}