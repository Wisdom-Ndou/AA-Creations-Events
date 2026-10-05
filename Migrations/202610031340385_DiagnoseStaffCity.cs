namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DiagnoseStaffCity : DbMigration
    {
        public override void Up()
        {
            // Some development databases received dbo.Staffs manually or through
            // an untracked migration. A clean production database does not.
            //
            // Make this migration safe in both situations:
            // 1. Fresh database: create Staffs with the current required schema.
            // 2. Existing database: add staff_City only when it is missing.
            Sql(@"
IF OBJECT_ID(N'dbo.Staffs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Staffs
    (
        staff_ID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Staffs PRIMARY KEY,
        staff_FName NVARCHAR(MAX) NOT NULL,
        staff_LName NVARCHAR(MAX) NOT NULL,
        staff_Email NVARCHAR(MAX) NOT NULL,
        staff_Passw NVARCHAR(MAX) NOT NULL,
        staff_Phone NVARCHAR(9) NOT NULL,
        staff_Type NVARCHAR(50) NULL,
        staff_City NVARCHAR(100) NOT NULL
    );
END
ELSE IF COL_LENGTH('dbo.Staffs', 'staff_City') IS NULL
BEGIN
    ALTER TABLE dbo.Staffs
    ADD staff_City NVARCHAR(100) NOT NULL
        CONSTRAINT DF_Staffs_staff_City DEFAULT ('');
END
");

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

            // Preserve dbo.Staffs because older development databases may have
            // created it outside this migration. Only remove the city column.
            Sql(@"
IF OBJECT_ID(N'dbo.Staffs', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.Staffs', 'staff_City') IS NOT NULL
BEGIN
    DECLARE @constraintName nvarchar(128);

    SELECT @constraintName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Staffs')
      AND c.name = N'staff_City';

    IF @constraintName IS NOT NULL
        EXEC(N'ALTER TABLE dbo.Staffs DROP CONSTRAINT [' + @constraintName + N']');

    ALTER TABLE dbo.Staffs DROP COLUMN staff_City;
END
");
        }
    }
}
