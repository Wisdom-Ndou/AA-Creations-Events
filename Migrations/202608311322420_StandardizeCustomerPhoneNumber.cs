namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StandardizeCustomerPhoneNumber : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "Cust_Phone", c => c.String(nullable: false, maxLength: 9));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "Cust_Phone", c => c.String(nullable: false));
        }
    }
}
