using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebApplication1.Models
{
    public class BusinessAnalyticsViewModel
    {
        // Main financial figures
        public decimal TotalRevenue { get; set; }

        public decimal TotalExpenditure { get; set; }

        public decimal NetProfit { get; set; }

        // Booking figures
        public int TotalBookings { get; set; }

        public int ApprovedBookings { get; set; }

        public int PendingBookings { get; set; }

        public int DeclinedBookings { get; set; }

        // Average approved booking value
        public decimal AverageBookingValue { get; set; }

        // Revenue grouped by occasion
        public List<AnalyticsCategory> RevenueByOccasion { get; set; }
            = new List<AnalyticsCategory>();

        // Recent expenses
        public List<Expense> RecentExpenses { get; set; }
            = new List<Expense>();
    }

    public class AnalyticsCategory
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }
    }
}