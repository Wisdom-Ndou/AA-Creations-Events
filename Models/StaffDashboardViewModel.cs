using System;
using System.Collections.Generic;


namespace WebApplication1.Models
{
    public class StaffDashboardViewModel
    {
        // Logged-in staff member
        public Staff StaffMember { get; set; }

        // Staff's assigned team/city
        public string TeamCity { get; set; }

        // Dashboard statistics
        public int TasksDueToday { get; set; }

        public int HighPriorityTasks { get; set; }

        public double HoursLogged { get; set; }

        public int OpenComplaints { get; set; }

        public int EventsThisWeek { get; set; }

        // Tasks displayed on dashboard
        public List<StaffTask> Tasks { get; set; }

        // Events assigned to this staff member
        public List<StaffEventDashboardItem> UpcomingEvents { get; set; }

        // Announcements
        public List<StaffAnnouncement> Announcements { get; set; }

        public StaffDashboardViewModel()
        {
            Tasks = new List<StaffTask>();

            UpcomingEvents = new List<StaffEventDashboardItem>();

            Announcements = new List<StaffAnnouncement>();
        }
    }

    public class StaffEventDashboardItem
    {
        public int BookingId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Occasion { get; set; }

        public DateTime EventDate { get; set; }

        public string EventTime { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Status { get; set; }
    }

    public class StaffAnnouncement
    {
        public int AnnouncementId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}