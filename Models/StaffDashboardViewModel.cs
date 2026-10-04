using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class StaffDashboardViewModel
    {
        public Staff StaffMember { get; set; }
        public string TeamCity { get; set; }

        public int TodayTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public int EventsThisWeek { get; set; }
        public int OpenComplaints { get; set; }

        // There is currently no time-entry entity in the application.
        // Null means the metric is not available rather than inventing a value.
        public double? HoursLogged { get; set; }

        public List<StaffTask> TasksDueToday { get; set; }
        public List<StaffEventDashboardItem> UpcomingEvents { get; set; }
        public List<StaffAnnouncement> Announcements { get; set; }

        public StaffDashboardViewModel()
        {
            TasksDueToday = new List<StaffTask>();
            UpcomingEvents = new List<StaffEventDashboardItem>();
            Announcements = new List<StaffAnnouncement>();
        }
    }

    public class StaffEventDashboardItem
    {
        public int BookingId { get; set; }
        public string Occasion { get; set; }
        public DateTime EventDate { get; set; }
        public string EventTime { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Status { get; set; }
    }

    // Announcement persistence has not yet been implemented.
    // This DTO keeps the dashboard ready for that feature without creating
    // a fake database table or hard-coded announcements.
    public class StaffAnnouncement
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
