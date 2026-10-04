namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncModelChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "Latitude", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Bookings", "Longitude", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bookings", "Longitude");
            DropColumn("dbo.Bookings", "Latitude");
        }
    }
}
