namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbAdmins : DbMigration
    {
        public override void Up()
        {
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

            AlterColumn("dbo.Staffs", "staff_Phone", c => c.String(nullable: false, maxLength: 10));
            DropTable("dbo.Expenses");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Expenses",
                c => new
                    {
                        ExpenseId = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false, maxLength: 150),
                        Category = c.String(nullable: false, maxLength: 100),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExpenseDate = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ExpenseId);
            
            DropForeignKey("dbo.StaffTasks", "StaffId", "dbo.Staffs");
            DropForeignKey("dbo.StaffTasks", "BookingId", "dbo.Bookings");
            DropIndex("dbo.StaffTasks", new[] { "BookingId" });
            DropIndex("dbo.StaffTasks", new[] { "StaffId" });
            AlterColumn("dbo.Staffs", "staff_Phone", c => c.String(nullable: false, maxLength: 10));
            DropTable("dbo.StaffTasks");
        }
    }
}
