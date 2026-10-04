namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStaffComplaints : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StaffComplaints",
                c => new
                    {
                        ComplaintId = c.Int(nullable: false, identity: true),
                        StaffId = c.Int(nullable: false),
                        BookingId = c.Int(nullable: false),
                        Subject = c.String(nullable: false, maxLength: 150),
                        Description = c.String(nullable: false, maxLength: 1000),
                        Priority = c.String(nullable: false, maxLength: 20),
                        Status = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                        ResolvedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.ComplaintId)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: true)
                .ForeignKey("dbo.Staffs", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.StaffId)
                .Index(t => t.BookingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StaffComplaints", "StaffId", "dbo.Staffs");
            DropForeignKey("dbo.StaffComplaints", "BookingId", "dbo.Bookings");
            DropIndex("dbo.StaffComplaints", new[] { "BookingId" });
            DropIndex("dbo.StaffComplaints", new[] { "StaffId" });
            DropTable("dbo.StaffComplaints");
        }
    }
}
