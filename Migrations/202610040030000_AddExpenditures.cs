namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddExpenditures : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Expenditures",
                c => new
                {
                    ExpenditureId = c.Int(nullable: false, identity: true),
                    Description = c.String(nullable: false, maxLength: 120),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    ExpenseDate = c.DateTime(nullable: false),
                    Category = c.String(maxLength: 80),
                    CreatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.ExpenditureId);
        }

        public override void Down()
        {
            DropTable("dbo.Expenditures");
        }
    }
}
