namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Bookings", "Status", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Bookings", "Status", c => c.String(nullable: false));
        }
    }
}
