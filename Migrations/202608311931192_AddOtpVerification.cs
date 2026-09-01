namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOtpVerification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OtpVerifications",
                c => new
                    {
                        OtpId = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        OtpHash = c.String(nullable: false, maxLength: 255),
                        DeliveryMethod = c.String(nullable: false, maxLength: 20),
                        Purpose = c.String(nullable: false, maxLength: 30),
                        CreatedAt = c.DateTime(nullable: false),
                        ExpiresAt = c.DateTime(nullable: false),
                        IsUsed = c.Boolean(nullable: false),
                        FailedAttempts = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OtpId)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OtpVerifications", "CustomerId", "dbo.Customers");
            DropIndex("dbo.OtpVerifications", new[] { "CustomerId" });
            DropTable("dbo.OtpVerifications");
        }
    }
}
