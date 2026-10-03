using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApplication1.Models
{
    public class DatabaseContext : DbContext
    {

        public DatabaseContext() : base("AndiswaDB")
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<BookingAddOn> BookingAddOns { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        // Added DbSets referenced by the controller
        public DbSet<CustomerAgreement> CustomerAgreements { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; }
    }
}