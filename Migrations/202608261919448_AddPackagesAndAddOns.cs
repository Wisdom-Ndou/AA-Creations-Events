namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPackagesAndAddOns : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AddOns",
                c => new
                    {
                        AddOnId = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 100),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.AddOnId);

            Sql(@"
                    INSERT INTO dbo.AddOns (AddOnId, Name, Price)
                    VALUES
                    ('balloons', 'Extra Balloon Bouquet', 50.00),
                    ('confetti', 'Confetti Cannon', 80.00),
                    ('lights', 'LED Fairy Lights', 120.00),
                    ('flowerwall', 'Flower Wall Backdrop', 200.00),
                    ('letters', 'Custom Letter/Number Balloons', 150.00),
                    ('candles', 'Scented Candle Set', 90.00)
                    ");

            CreateTable(
               "dbo.Packages",
               c => new
               {
                   PackageId = c.String(nullable: false, maxLength: 50),
                   Name = c.String(nullable: false, maxLength: 100),
                   Price = c.Decimal(nullable: false, precision: 18, scale: 2),
               })
               .PrimaryKey(t => t.PackageId);

            Sql(@"
             INSERT INTO dbo.Packages (PackageId, Name, Price)
             VALUES
                ('basic', 'Basic Package', 650.00),
                ('standard', 'Standard Package', 850.00),
                ('premium', 'Premium Package', 1000.00)
                ");

            CreateTable(
                "dbo.BookingAddOns",
                c => new
                    {
                        BookingAddOnId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        AddOnId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.BookingAddOnId)
                .ForeignKey("dbo.AddOns", t => t.AddOnId, cascadeDelete: false)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: false)
                .Index(t => t.BookingId)
                .Index(t => t.AddOnId);           
            
            AlterColumn("dbo.Bookings", "PackageId", c => c.String(nullable: false, maxLength: 50));
            CreateIndex("dbo.Bookings", "PackageId");
            AddForeignKey("dbo.Bookings", "PackageId", "dbo.Packages", "PackageId", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "PackageId", "dbo.Packages");
            DropForeignKey("dbo.BookingAddOns", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.BookingAddOns", "AddOnId", "dbo.AddOns");
            DropIndex("dbo.Bookings", new[] { "PackageId" });
            DropIndex("dbo.BookingAddOns", new[] { "AddOnId" });
            DropIndex("dbo.BookingAddOns", new[] { "BookingId" });
            AlterColumn("dbo.Bookings", "PackageId", c => c.String(nullable: false));
            DropTable("dbo.Packages");
            DropTable("dbo.BookingAddOns");
            DropTable("dbo.AddOns");
        }
    }
}
