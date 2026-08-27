using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class CreateBookingRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Occasion { get; set; }

        public DateTime EventDate { get; set; }

        public string EventTime { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Notes { get; set; }

        public string PackageId { get; set; }

        public List<string> AddOns { get; set; }
    }
}