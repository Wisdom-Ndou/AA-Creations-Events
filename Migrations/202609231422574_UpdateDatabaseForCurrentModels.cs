namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseForCurrentModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "CreatedAt");
        }
    }
}
