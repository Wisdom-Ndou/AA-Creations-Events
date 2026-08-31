namespace WebApplication1.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddBookingCreatedAtStatus : DbMigration
    {
        public override void Up()
        {
            // CreatedAt already exists in some versions of the database,
            // so only add it if it does not exist.
            Sql(@"
                IF COL_LENGTH('dbo.Bookings', 'CreatedAt') IS NULL
                BEGIN
                    ALTER TABLE dbo.Bookings
                    ADD CreatedAt datetime NOT NULL
                        CONSTRAINT DF_Bookings_CreatedAt
                        DEFAULT (GETDATE());
                END
            ");

            // Only add Status if it does not already exist.
            Sql(@"
                IF COL_LENGTH('dbo.Bookings', 'Status') IS NULL
                BEGIN
                    ALTER TABLE dbo.Bookings
                    ADD Status nvarchar(50) NULL
                        CONSTRAINT DF_Bookings_Status
                        DEFAULT ('Pending');
                END
            ");
        }

        public override void Down()
        {
            Sql(@"
                IF COL_LENGTH('dbo.Bookings', 'Status') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.Bookings
                    DROP COLUMN Status;
                END
            ");

            Sql(@"
                IF COL_LENGTH('dbo.Bookings', 'CreatedAt') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.Bookings
                    DROP COLUMN CreatedAt;
                END
            ");
        }
    }
}