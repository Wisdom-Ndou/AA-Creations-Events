using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    public class AllBookingsViewModel
    {
        // All bookings
        public List<Booking> AllBookings { get; set; }
            = new List<Booking>();

        // Past events
        public List<Booking> PastBookings { get; set; }
            = new List<Booking>();

        // Upcoming events
        public List<Booking> UpcomingBookings { get; set; }
            = new List<Booking>();

        // Cancelled bookings
        public List<Booking> CancelledBookings { get; set; }
            = new List<Booking>();

        // Summary counts
        public int TotalBookings { get; set; }

        public int PastBookingCount { get; set; }

        public int UpcomingBookingCount { get; set; }

        public int CancelledBookingCount { get; set; }
    }
}