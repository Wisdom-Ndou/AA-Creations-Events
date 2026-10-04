namespace WebApplication1.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class SyncTermsAcceptanceSchema : DbMigration
    {
        public override void Up()
        {
            // Intentionally empty.
            //
            // TermsAndConditions and CustomerAgreements were created manually
            // using Database/TermsAcceptance.sql.
            //
            // This migration exists to synchronize Entity Framework's model
            // snapshot with the existing database schema without attempting
            // to recreate those tables.
        }

        public override void Down()
        {
            // Intentionally empty.
            //
            // Do not drop the Terms tables when rolling back this metadata
            // synchronization migration because they were created outside
            // this EF migration.
        }
    }
}