using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class StaffDashboardViewModel
    {
        // ==========================================
        // LOGGED-IN STAFF MEMBER
        // ==========================================

        public Staff StaffMember { get; set; }


        // ==========================================
        // STAFF TEAM / CITY
        // ==========================================

        public string TeamCity { get; set; }


        // ==========================================
        // DASHBOARD STATISTICS
        // ==========================================

        public int TodayTasks { get; set; }

        public int HighPriorityTasks { get; set; }

        public double HoursLogged { get; set; }

        public int OpenComplaints { get; set; }

        public int EventsThisWeek { get; set; }


        // ==========================================
        // STAFF TASKS
        // ==========================================

        public List<StaffTask> Tasks { get; set; }


        // ==========================================
        // UPCOMING EVENTS
        // ==========================================

        public List<StaffEventDashboardItem> Events { get; set; }


        // ==========================================
        // ANNOUNCEMENTS
        // ==========================================

        public List<StaffAnnouncement> Announcements { get; set; }


        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public StaffDashboardViewModel()
        {
            Tasks = new List<StaffTask>();

            Events = new List<StaffEventDashboardItem>();

            Announcements = new List<StaffAnnouncement>();
        }
    }


    // ==============================================
    // STAFF EVENT DASHBOARD ITEM
    // ==============================================

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


    // ==============================================
    // STAFF ANNOUNCEMENT
    // ==============================================

    public class StaffAnnouncement
    {
        public int AnnouncementId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}