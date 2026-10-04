namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateStaffTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Staffs", "staff_Type", c => c.String(maxLength: 50));
            DropColumn("dbo.Staffs", "staff_Department");
            DropColumn("dbo.Staffs", "staff_Position");
            DropColumn("dbo.Staffs", "staff_CreatedAt");
            DropColumn("dbo.Staffs", "staff_IsActive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Staffs", "staff_IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Staffs", "staff_CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Staffs", "staff_Position", c => c.String(maxLength: 100));
            AddColumn("dbo.Staffs", "staff_Department", c => c.String(maxLength: 50));
            DropColumn("dbo.Staffs", "staff_Type");
        }
    }
}
