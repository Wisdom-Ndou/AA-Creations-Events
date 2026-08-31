namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncCustomerModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "Cust_Phone", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "Cust_Phone", c => c.String(nullable: false, maxLength: 10));
        }
    }
}
