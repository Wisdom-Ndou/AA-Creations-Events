namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTermsAndConditions2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustomerAgreements", "Customer_Cust_ID", "dbo.Customers");
            DropIndex("dbo.CustomerAgreements", new[] { "Customer_Cust_ID" });
            DropColumn("dbo.CustomerAgreements", "CustomerId");
            RenameColumn(table: "dbo.CustomerAgreements", name: "Customer_Cust_ID", newName: "CustomerId");
            AlterColumn("dbo.CustomerAgreements", "TermsVersion", c => c.String(maxLength: 50));
            AlterColumn("dbo.CustomerAgreements", "CookiePreference", c => c.String(maxLength: 20));
            AlterColumn("dbo.CustomerAgreements", "CustomerId", c => c.Int(nullable: false));
            CreateIndex("dbo.CustomerAgreements", "CustomerId");
            AddForeignKey("dbo.CustomerAgreements", "CustomerId", "dbo.Customers", "Cust_ID", cascadeDelete: true);
            DropColumn("dbo.Customers", "AcceptTerms");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "TermsAcceptedDate");
            DropColumn("dbo.Customers", "AcceptedTermsID");
            DropColumn("dbo.Customers", "AlternativeContactEmail");
            DropColumn("dbo.Customers", "AlternativeContactPhone");
            DropColumn("dbo.Customers", "AlternativeContactRelationship");
            DropColumn("dbo.Customers", "AlternativeContactName");
            DropTable("dbo.CustomerAgreements");
            DropTable("dbo.TermsAndConditions");
        }
    }
}
