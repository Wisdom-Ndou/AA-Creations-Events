namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAlternativeContactDetails1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "AlternativeContactName", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "AlternativeContactName", c => c.String(nullable: false));
        }
    }
}
