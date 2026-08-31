namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncCurrentModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "Cust_Passw", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "Cust_Passw", c => c.String(nullable: false, maxLength: 15));
        }
    }
}
