namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomerToBooking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "CustomerId", c => c.Int());
            CreateIndex("dbo.Bookings", "CustomerId");
            AddForeignKey("dbo.Bookings", "CustomerId", "dbo.Customers", "Cust_ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "CustomerId", "dbo.Customers");
            DropIndex("dbo.Bookings", new[] { "CustomerId" });
            DropColumn("dbo.Bookings", "CustomerId");
        }
    }
}
