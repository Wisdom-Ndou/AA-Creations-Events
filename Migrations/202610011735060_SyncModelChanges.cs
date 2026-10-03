namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncModelChanges : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "AlternativeContactPhone", c => c.String(maxLength: 9));
            AlterColumn("dbo.Customers", "AlternativeContactEmail", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "AlternativeContactEmail", c => c.String(nullable: false));
            AlterColumn("dbo.Customers", "AlternativeContactPhone", c => c.String(nullable: false, maxLength: 9));
        }
    }
}
