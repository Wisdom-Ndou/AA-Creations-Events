namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTermsAndConditions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "AcceptTerms", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "AcceptTerms");
        }
    }
}
