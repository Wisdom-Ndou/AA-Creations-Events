namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingModelChanges : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Admins", "admin_Passw", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Admins", "admin_Passw", c => c.String(nullable: false, maxLength: 15));
        }
    }
}
