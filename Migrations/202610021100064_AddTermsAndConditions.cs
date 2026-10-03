namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTermsAndConditions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TermsAndConditions",
                c => new
                    {
                        Terms_ID = c.Int(nullable: false, identity: true),
                        Version = c.String(nullable: false, maxLength: 20),
                        Content = c.String(nullable: false),
                        EffectiveDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Terms_ID);
            
            AddColumn("dbo.Customers", "AcceptedTermsID", c => c.Int());
            AddColumn("dbo.Customers", "TermsAcceptedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "TermsAcceptedDate");
            DropColumn("dbo.Customers", "AcceptedTermsID");
            DropTable("dbo.TermsAndConditions");
        }
    }
}
