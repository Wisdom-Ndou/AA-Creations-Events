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

        public DbSet<Customer> objCustomers { get; set; }
        public DbSet<Admin> objAdmin { get; set; }
        public DbSet<Booking> Bookings { get; set; }

    }

}