using System;
using System.Data.Entity.Migrations;

namespace WebApplication1.Migrations
{
    public partial class AddAlternativeContactToCustomer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "Cust_AltEmail", c => c.String());
            AddColumn("dbo.Customers", "Cust_AltPhone", c => c.String(maxLength: 9));
        }

        public override void Down()
        {
            DropColumn("dbo.Customers", "Cust_AltPhone");
            DropColumn("dbo.Customers", "Cust_AltEmail");
        }
    }
}