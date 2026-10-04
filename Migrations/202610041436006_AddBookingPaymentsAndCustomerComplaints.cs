namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookingPaymentsAndCustomerComplaints : DbMigration
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
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: true)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.BookingId);
            
            AddColumn("dbo.Bookings", "AmountPaid", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Bookings", "BalanceDueDate", c => c.DateTime());
            AddColumn("dbo.Bookings", "PaymentStatus", c => c.String(maxLength: 30));
            AddColumn("dbo.Bookings", "CancellationCharge", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Bookings", "RefundAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Bookings", "TermsVersion", c => c.String(maxLength: 30));
            AddColumn("dbo.Bookings", "TermsAcceptedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomerComplaints", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.CustomerComplaints", "BookingId", "dbo.Bookings");
            DropIndex("dbo.CustomerComplaints", new[] { "BookingId" });
            DropIndex("dbo.CustomerComplaints", new[] { "CustomerId" });
            DropColumn("dbo.Bookings", "TermsAcceptedAt");
            DropColumn("dbo.Bookings", "TermsVersion");
            DropColumn("dbo.Bookings", "RefundAmount");
            DropColumn("dbo.Bookings", "CancellationCharge");
            DropColumn("dbo.Bookings", "PaymentStatus");
            DropColumn("dbo.Bookings", "BalanceDueDate");
            DropColumn("dbo.Bookings", "AmountPaid");
            DropTable("dbo.CustomerComplaints");
        }
    }
}
