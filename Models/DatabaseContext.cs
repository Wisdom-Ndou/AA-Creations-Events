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

        public DbSet<Admin> Admin { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<AddOn> AddOns { get; set; }

        public DbSet<BookingAddOn> BookingAddOns { get; set; }

    }

}