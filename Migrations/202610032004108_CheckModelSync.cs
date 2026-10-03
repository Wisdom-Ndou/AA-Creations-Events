namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CheckModelSync : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Staffs",
                c => new
                    {
                        staff_ID = c.Int(nullable: false, identity: true),
                        staff_FName = c.String(nullable: false),
                        staff_LName = c.String(nullable: false),
                        staff_Email = c.String(nullable: false),
                        staff_Passw = c.String(nullable: false),
                        staff_Phone = c.String(nullable: false, maxLength: 9),
                        staff_Type = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.staff_ID);
            
            CreateTable(
                "dbo.StaffTasks",
                c => new
                    {
                        TaskId = c.Int(nullable: false, identity: true),
                        StaffId = c.Int(nullable: false),
                        TaskName = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        BookingId = c.Int(),
                        DueDate = c.DateTime(nullable: false),
                        Priority = c.String(nullable: false, maxLength: 20),
                        Status = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .ForeignKey("dbo.Staffs", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.StaffId)
                .Index(t => t.BookingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StaffTasks", "StaffId", "dbo.Staffs");
            DropForeignKey("dbo.StaffTasks", "BookingId", "dbo.Bookings");
            DropIndex("dbo.StaffTasks", new[] { "BookingId" });
            DropIndex("dbo.StaffTasks", new[] { "StaffId" });
            DropTable("dbo.StaffTasks");
            DropTable("dbo.Staffs");
        }
    }
}
