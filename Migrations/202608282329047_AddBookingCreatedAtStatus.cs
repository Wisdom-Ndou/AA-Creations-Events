namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookingCreatedAtStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "CreatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
            AddColumn("dbo.Bookings", "Status", c => c.String(nullable: true, maxLength: 50, defaultValue: "Pending"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bookings", "Status");
            DropColumn("dbo.Bookings", "CreatedAt");
        }
    }
}
