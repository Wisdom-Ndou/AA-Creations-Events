namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStaffsTable1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Staffs", "staff_Phone", c => c.String(nullable: false, maxLength: 10));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Staffs", "staff_Phone", c => c.String(nullable: false, maxLength: 9));
        }
    }
}
