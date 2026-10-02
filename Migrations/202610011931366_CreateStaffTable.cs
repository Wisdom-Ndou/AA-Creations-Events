namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateStaffTable : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Staffs");
        }
    }
}
