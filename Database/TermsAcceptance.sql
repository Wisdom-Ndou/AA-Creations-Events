/*
AA Creations & Events - Terms acceptance schema
Run this only if the equivalent tables do not already exist.
The script is idempotent so databases that already received the sihle schema
will not recreate the tables.
*/
IF OBJECT_ID(N'dbo.TermsAndConditions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TermsAndConditions
    (
        Terms_ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TermsAndConditions PRIMARY KEY,
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
        AgreementId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerAgreements PRIMARY KEY,
        CustomerId INT NOT NULL,
        TermsAccepted BIT NOT NULL,
        TermsVersion NVARCHAR(20) NOT NULL,
        AcceptedAt DATETIME NOT NULL,
        CookiePreference NVARCHAR(20) NOT NULL,
        CONSTRAINT FK_CustomerAgreements_Customers
            FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Cust_ID) ON DELETE CASCADE
    );

    CREATE INDEX IX_CustomerAgreements_CustomerId
        ON dbo.CustomerAgreements(CustomerId);
END;

/*
After creating the tables, insert your approved Terms text as an active version.
Example:
INSERT dbo.TermsAndConditions(Version, Content, EffectiveDate, IsActive, CreatedDate)
VALUES ('1.0', '<p>Approved terms text...</p>', GETDATE(), 1, GETDATE());
*/
