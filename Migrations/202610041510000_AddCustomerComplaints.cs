namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddCustomerComplaints : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerComplaints",
                c => new
                    {
                        ComplaintId = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        BookingId = c.Int(nullable: false),
                        Category = c.String(nullable: false, maxLength: 60),
                        Subject = c.String(nullable: false, maxLength: 120),
                        Description = c.String(nullable: false, maxLength: 2000),
                        Status = c.String(nullable: false, maxLength: 30),
                        AdminResponse = c.String(maxLength: 2000),
                        CreatedAt = c.DateTime(nullable: false),
                        ResolvedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.ComplaintId)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: false)
                .Index(t => t.CustomerId)
                .Index(t => t.BookingId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.CustomerComplaints", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.CustomerComplaints", "CustomerId", "dbo.Customers");
            DropIndex("dbo.CustomerComplaints", new[] { "BookingId" });
            DropIndex("dbo.CustomerComplaints", new[] { "CustomerId" });
            DropTable("dbo.CustomerComplaints");
        }
    }
}