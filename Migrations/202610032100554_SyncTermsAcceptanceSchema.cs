namespace WebApplication1.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class SyncTermsAcceptanceSchema : DbMigration
    {
        public override void Up()
        {
            // Development databases originally received these tables through
            // Database/TermsAcceptance.sql. Fresh production/staging databases
            // must be able to reproduce the schema using EF migrations alone.
            Sql(@"
IF OBJECT_ID(N'dbo.TermsAndConditions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TermsAndConditions
    (
        Terms_ID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TermsAndConditions PRIMARY KEY,
        Version NVARCHAR(20) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        EffectiveDate DATETIME NOT NULL,
        IsActive BIT NOT NULL,
        CreatedDate DATETIME NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.CustomerAgreements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerAgreements
    (
        AgreementId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_CustomerAgreements PRIMARY KEY,
        CustomerId INT NOT NULL,
        TermsAccepted BIT NOT NULL,
        TermsVersion NVARCHAR(20) NOT NULL,
        AcceptedAt DATETIME NOT NULL,
        CookiePreference NVARCHAR(20) NOT NULL,
        CONSTRAINT FK_CustomerAgreements_Customers
            FOREIGN KEY (CustomerId)
            REFERENCES dbo.Customers(Cust_ID)
            ON DELETE CASCADE
    );

    CREATE INDEX IX_CustomerAgreements_CustomerId
        ON dbo.CustomerAgreements(CustomerId);
END;
");
        }

        public override void Down()
        {
            // Preserve legal/audit data when rolling back application code.
            // Terms and customer acceptance records must never be dropped
            // automatically by a routine deployment rollback.
        }
    }
}
