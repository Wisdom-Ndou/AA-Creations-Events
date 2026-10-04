namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStaffTimeEntries : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StaffTimeEntries",
                c => new
                    {
                        TimeEntryId = c.Int(nullable: false, identity: true),
                        StaffId = c.Int(nullable: false),
                        ClockInTime = c.DateTime(nullable: false),
                        ClockOutTime = c.DateTime(),
                        HoursWorked = c.Double(),
                    })
                .PrimaryKey(t => t.TimeEntryId)
                .ForeignKey("dbo.Staffs", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.StaffId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StaffTimeEntries", "StaffId", "dbo.Staffs");
            DropIndex("dbo.StaffTimeEntries", new[] { "StaffId" });
            DropTable("dbo.StaffTimeEntries");
        }
    }
}
