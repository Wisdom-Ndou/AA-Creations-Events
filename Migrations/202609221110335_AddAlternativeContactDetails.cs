namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAlternativeContactDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "AlternativeContactName", c => c.String(nullable: false));
            AddColumn("dbo.Customers", "AlternativeContactRelationship", c => c.String());
            AddColumn("dbo.Customers", "AlternativeContactPhone", c => c.String(nullable: false, maxLength: 9));
            AddColumn("dbo.Customers", "AlternativeContactEmail", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "AlternativeContactEmail");
            DropColumn("dbo.Customers", "AlternativeContactPhone");
            DropColumn("dbo.Customers", "AlternativeContactRelationship");
            DropColumn("dbo.Customers", "AlternativeContactName");
        }
    }
}
