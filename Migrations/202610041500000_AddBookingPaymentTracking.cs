namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddBookingPaymentTracking : DbMigration
    {
        public override void Up()
        {
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
            DropColumn("dbo.Bookings", "TermsAcceptedAt");
            DropColumn("dbo.Bookings", "TermsVersion");
            DropColumn("dbo.Bookings", "RefundAmount");
            DropColumn("dbo.Bookings", "CancellationCharge");
            DropColumn("dbo.Bookings", "PaymentStatus");
            DropColumn("dbo.Bookings", "BalanceDueDate");
            DropColumn("dbo.Bookings", "AmountPaid");
        }
    }
}