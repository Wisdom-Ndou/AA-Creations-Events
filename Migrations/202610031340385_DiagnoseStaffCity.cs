namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DiagnoseStaffCity : DbMigration
    {
        public override void Up()
        {
            // Add the required city field to the existing Staffs table.
            AddColumn(
                "dbo.Staffs",
                "staff_City",
                c => c.String(nullable: false, maxLength: 100)
            );

            // Create StaffTasks.
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
                    CompletionReason = c.String(maxLength: 1000),
                    CompletedAt = c.DateTime(),
                    CreatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Staffs", t => t.StaffId, cascadeDelete: true)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .Index(t => t.StaffId)
                .Index(t => t.BookingId);

            // Create StaffComplaints.
            CreateTable(
                "dbo.StaffComplaints",
                c => new
                {
                    ComplaintId = c.Int(nullable: false, identity: true),
                    StaffId = c.Int(nullable: false),
                    BookingId = c.Int(),
                    Title = c.String(nullable: false, maxLength: 200),
                    Description = c.String(nullable: false, maxLength: 2000),
                    Priority = c.String(nullable: false, maxLength: 20),
                    Status = c.String(nullable: false, maxLength: 20),
                    AdminResponse = c.String(maxLength: 2000),
                    ResolvedAt = c.DateTime(),
                    CreatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.ComplaintId)
                .ForeignKey("dbo.Staffs", t => t.StaffId, cascadeDelete: true)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .Index(t => t.StaffId)
                .Index(t => t.BookingId);
        }

        public override void Down()
        {
            // Drop dependent tables first.
            DropForeignKey("dbo.StaffComplaints", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.StaffComplaints", "StaffId", "dbo.Staffs");
            DropForeignKey("dbo.StaffTasks", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.StaffTasks", "StaffId", "dbo.Staffs");

            DropIndex("dbo.StaffComplaints", new[] { "BookingId" });
            DropIndex("dbo.StaffComplaints", new[] { "StaffId" });
            DropIndex("dbo.StaffTasks", new[] { "BookingId" });
            DropIndex("dbo.StaffTasks", new[] { "StaffId" });

            DropTable("dbo.StaffComplaints");
            DropTable("dbo.StaffTasks");

            // Remove the city field from the existing Staffs table.
            DropColumn("dbo.Staffs", "staff_City");
        }
    }
}