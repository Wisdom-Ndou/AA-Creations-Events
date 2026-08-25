namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookings",
                c => new
                {
                    BookingId = c.Int(nullable: false, identity: true),
                    FirstName = c.String(nullable: false),
                    LastName = c.String(nullable: false),
                    Email = c.String(nullable: false),
                    Phone = c.String(nullable: false),
                    Occasion = c.String(nullable: false),
                    EventDate = c.DateTime(nullable: false),
                    EventTime = c.String(nullable: false),
                    Address = c.String(nullable: false),
                    City = c.String(nullable: false),
                    Notes = c.String(),
                    PackageId = c.String(nullable: false),
                    TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    CreatedAt = DateTime.Now,
                        Status = "Pending",
                })
                .PrimaryKey(t => t.BookingId);
            
            CreateTable(
                "dbo.Admins",
                c => new
                    {
                        admin_ID = c.Int(nullable: false, identity: true),
                        admin_FName = c.String(nullable: false),
                        admin_LName = c.String(nullable: false),
                        admin_Email = c.String(nullable: false),
                        admin_Passw = c.String(nullable: false, maxLength: 15),
                    })
                .PrimaryKey(t => t.admin_ID);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Cust_ID = c.Int(nullable: false, identity: true),
                        Cust_FName = c.String(nullable: false),
                        Cust_LName = c.String(nullable: false),
                        Cust_UName = c.String(),
                        Cust_Phone = c.String(nullable: false, maxLength: 10),
                        Cust_Email = c.String(nullable: false),
                        Cust_Passw = c.String(nullable: false, maxLength: 15),
                    })
                .PrimaryKey(t => t.Cust_ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Customers");
            DropTable("dbo.Admins");
            DropTable("dbo.Bookings");
        }
    }
}
