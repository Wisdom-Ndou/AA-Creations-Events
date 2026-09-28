namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingModelChanges1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Admins", "admin_Phone", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Admins", "admin_Phone");
        }
    }
}
