namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomerAgreement : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerAgreements",
                c => new
                    {
                        AgreementId = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        TermsAccepted = c.Boolean(nullable: false),
                        TermsVersion = c.String(nullable: false, maxLength: 20),
                        AcceptedAt = c.DateTime(nullable: false),
                        CookiePreference = c.String(nullable: false, maxLength: 20),
                        Customer_Cust_ID = c.Int(),
                    })
                .PrimaryKey(t => t.AgreementId)
                .ForeignKey("dbo.Customers", t => t.Customer_Cust_ID)
                .Index(t => t.Customer_Cust_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomerAgreements", "Customer_Cust_ID", "dbo.Customers");
            DropIndex("dbo.CustomerAgreements", new[] { "Customer_Cust_ID" });
            DropTable("dbo.CustomerAgreements");
        }
    }
}
