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

            // Seed the exact active Terms & Conditions version exported from
            // the working database on 05 October 2026. Never overwrite an
            // existing historical version; later terms updates must be added
            // as new rows/versions.
            var termsV1Content = @"  <h2>AA CREATIONS &amp; EVENTS</h2>  <h2>TERMS AND CONDITIONS</h2>    <p><strong>Effective Date:</strong> 03 October 2026</p>    <p>These Terms and Conditions govern the booking and provision of event decoration and related event services by <strong>AA Creations &amp; Events</strong> (""AA Creations & Events"", ""we"", ""us"" or ""our"") to the customer (""you"" or ""the client"").</p>    <p>By making a booking, paying a deposit or payment, or confirming an order with AA Creations & Events, you acknowledge that you have read, understood and agreed to these Terms and Conditions.</p>    <p>These Terms and Conditions are intended to operate in accordance with the applicable laws of the <strong>Republic of South Africa</strong>, including the <strong>Consumer Protection Act 68 of 2008</strong>, the <strong>Electronic Communications and Transactions Act 25 of 2002</strong>, and applicable privacy legislation.</p>    <h3>1. BOOKING AND CONFIRMATION</h3>    <p>1.1 A booking is considered confirmed once AA Creations & Events has received the required payment or deposit and the booking has been accepted by the business.</p>    <p>1.2 A quotation, package selection or booking request does not guarantee availability until it has been confirmed by AA Creations & Events.</p>    <p>1.3 The client is responsible for providing accurate information regarding:</p>    <ul>  <li>The event date;</li>  <li>Event venue;</li>  <li>Event start and setup times;</li>  <li>Number of guests, where applicable;</li>  <li>Selected package or services;</li>  <li>Decoration requirements; and</li>  <li>Any other information reasonably required to provide the service.</li>  </ul>    <p>1.4 Any changes to the booking must be communicated to AA Creations & Events as soon as reasonably possible.</p>    <p>1.5 Changes requested by the client may be subject to additional charges, depending on the nature and extent of the requested changes.</p>    <h3>2. PAYMENT TERMS</h3>    <p>2.1 The total price of the selected package or service will be communicated to the client before the booking is confirmed.</p>    <p>2.2 A deposit or full payment may be required to secure the booking, as specified in the quotation or invoice.</p>    <p>2.3 Where a deposit is required, the remaining balance must be paid by the deadline communicated by AA Creations & Events.</p>    <p>2.4 AA Creations & Events reserves the right not to commence preparation or provide the service where required payments have not been received by the agreed payment deadline, subject to applicable law and the terms of the booking agreement.</p>    <p>2.5 All prices communicated to the client should clearly indicate any applicable additional costs, including delivery, transportation, venue-related charges or additional services.</p>    <h3>3. EVENT DECORATION SERVICES</h3>    <p>3.1 AA Creations & Events will provide the decoration services described in the client's confirmed booking, quotation or selected package.</p>    <p>3.2 Images used on the website, social media or promotional material may be examples of previous work and may not be an exact representation of the final decoration.</p>    <p>3.3 Where certain flowers, fabrics, decorations, colours, furniture or other materials are unavailable, AA Creations & Events may use a reasonably similar alternative, where necessary, while taking the client's agreed design requirements into consideration.</p>    <p>3.4 Any major changes to the agreed design or package will be discussed with the client before implementation where reasonably practicable.</p>    <p>3.5 The final setup may reasonably differ from promotional photographs due to venue size, lighting, available space, weather conditions, material availability or other circumstances.</p>    <h3>4. VENUE AND ACCESS</h3>    <p>4.1 The client is responsible for ensuring that AA Creations & Events has reasonable access to the venue at the agreed setup time.</p>    <p>4.2 The client must inform AA Creations & Events of any venue rules, restrictions, access requirements, parking requirements or setup limitations that may affect the service.</p>    <p>4.3 Delays caused by restricted venue access, venue staff, late access or circumstances outside AA Creations & Events' reasonable control may affect setup times.</p>    <p>4.4 Where additional costs arise because of circumstances that were not disclosed before the booking, the client may be responsible for those additional costs where permitted by law and agreed with the client.</p>    <h3>5. CLIENT RESPONSIBILITIES</h3>    <p>The client agrees to:</p>    <p>5.1 Provide accurate information required for the booking.</p>  <p>5.2 Provide AA Creations & Events with reasonable access to the venue.</p>  <p>5.3 Ensure that the venue is suitable for the agreed decoration services.</p>  <p>5.4 Inform AA Creations & Events of any relevant venue restrictions before the event.</p>  <p>5.5 Make all required payments within the agreed timeframes.</p>  <p>5.6 Treat AA Creations & Events staff, contractors and representatives respectfully.</p>    <h3>6. CANCELLATION AND REFUND POLICY</h3>    <p>AA Creations & Events recognises that circumstances may change and allows clients to cancel their bookings subject to the cancellation terms below and applicable South African consumer-protection law.</p>    <h4>6.1 Cancellation by AA Creations & Events</h4>    <p>If <strong>AA Creations & Events cancels a confirmed booking</strong> and is unable to provide the agreed services, the client will be entitled to a <strong>full refund of all amounts paid for the cancelled service</strong>, unless the client agrees to an alternative arrangement.</p>    <p>The refund will be made to the client's nominated payment account or through the applicable payment method used for the booking, subject to reasonable payment-processing requirements.</p>    <p>AA Creations & Events will communicate the cancellation to the client as soon as reasonably possible.</p>    <h4>6.2 Cancellation by the Client — More Than 7 Days Before the Event</h4>    <p>If the client cancels the booking <strong>more than 7 days before the scheduled event date</strong>, AA Creations & Events will retain <strong>10% of the total amount paid</strong> as a cancellation charge, with the remaining eligible amount refunded to the client.</p>    <h4>6.3 Cancellation by the Client — 7 Days or Less Before the Event</h4>    <p>If the client cancels the booking <strong>7 days or less before the scheduled event date</strong>, AA Creations & Events will retain <strong>20% of the total amount paid</strong> as a cancellation charge, with the remaining eligible amount refunded to the client.</p>    <h4>6.4 Reasonableness and Consumer Rights</h4>    <p>The cancellation charges stated above are intended to compensate AA Creations & Events for reasonable administrative, preparation, scheduling and potential loss associated with cancelling an event booking.</p>    <p>The cancellation charges will be applied subject to the <strong>Consumer Protection Act 68 of 2008</strong> and any other applicable South African law. Where the law provides a consumer with a greater right or protection, that legal right will prevail.</p>    <p>A cancellation charge will not be applied where applicable South African law prohibits the charge, including circumstances where the CPA specifically protects the consumer from a cancellation fee.</p>    <h4>6.5 Refund Processing</h4>    <p>Where a refund is due, AA Creations & Events will process the refund within a reasonable period after confirming the cancellation and receiving any information reasonably required to process the refund.</p>    <p>Where a payment provider or banking institution is responsible for processing the refund, the time taken for the funds to reflect in the client's account may depend on that provider.</p>    <h3>7. RESCHEDULING OF EVENTS</h3>    <p>7.1 A client may request to move an event to another date.</p>  <p>7.2 Rescheduling is subject to the availability of AA Creations & Events on the requested new date.</p>  <p>7.3 A rescheduling request does not automatically guarantee availability.</p>  <p>7.4 Additional costs may apply where the new event date results in additional venue, transportation, equipment, staffing or material costs.</p>    <h3>8. FORCE MAJEURE / EVENTS BEYOND REASONABLE CONTROL</h3>    <p>8.1 AA Creations & Events will not be held responsible for delays or inability to perform caused by circumstances reasonably beyond its control, including but not limited to:</p>    <ul>  <li>Severe weather conditions;</li>  <li>Natural disasters;</li>  <li>Fire;</li>  <li>Flooding;</li>  <li>Government restrictions;</li>  <li>Civil unrest;</li>  <li>Power or infrastructure failures;</li>  <li>Venue closure;</li>  <li>Transport disruptions; or</li>  <li>Other circumstances that could not reasonably have been prevented or anticipated.</li>  </ul>    <p>8.2 Where such circumstances affect the booking, AA Creations & Events will communicate with the client and, where reasonably possible, attempt to arrange an alternative date or suitable solution.</p>    <p>8.3 Nothing in this clause removes or limits any consumer right that cannot lawfully be excluded under South African law.</p>    <h3>9. DAMAGE TO DECORATION, EQUIPMENT AND PROPERTY</h3>    <p>9.1 AA Creations & Events may provide decorations, furniture, props, equipment and other items for use during an event.</p>    <p>9.2 The client and/or event venue must take reasonable care of items supplied by AA Creations & Events.</p>    <p>9.3 The client may be responsible for reasonable costs resulting from intentional or negligent damage to AA Creations & Events' property, subject to applicable law.</p>    <p>9.4 The client will not be responsible for ordinary wear and tear or damage that was not caused by the client or persons under the client's responsibility.</p>    <h3>10. PHOTOGRAPHS AND PROMOTIONAL MATERIAL</h3>    <p>10.1 AA Creations & Events may photograph or record completed decoration setups for its portfolio, website and social media.</p>    <p>10.2 Where photographs contain identifiable clients, guests or other individuals, AA Creations & Events will obtain appropriate consent where required by applicable privacy law.</p>    <p>10.3 A client may inform AA Creations & Events that they do not wish identifiable photographs of themselves to be used for promotional purposes.</p>    <h3>11. PERSONAL INFORMATION AND PRIVACY</h3>    <p>11.1 AA Creations & Events may collect personal information necessary to process bookings and provide services.</p>    <p>11.2 Personal information may include the client's name, contact details, event information, billing information and other information reasonably required to fulfil the booking.</p>    <p>11.3 AA Creations & Events will handle personal information in accordance with applicable South African privacy legislation, including the <strong>Protection of Personal Information Act 4 of 2013 (POPIA)</strong>.</p>    <p>11.4 Personal information will not knowingly be sold or disclosed to unrelated third parties except where required to provide the service, process payments, comply with legal obligations, or where otherwise permitted by law.</p>    <h3>12. COMPLAINTS AND DISPUTE RESOLUTION</h3>    <p>12.1 Clients are encouraged to contact AA Creations & Events as soon as possible if they have a complaint regarding a booking or service.</p>    <p>12.2 Complaints should provide sufficient information to allow AA Creations & Events to investigate the matter.</p>    <p>12.3 AA Creations & Events will make reasonable efforts to resolve complaints fairly and promptly.</p>    <p>12.4 Nothing in these Terms and Conditions prevents a consumer from exercising any right available under South African consumer-protection law or approaching an appropriate regulatory or dispute-resolution body.</p>    <h3>13. LIMITATION OF LIABILITY</h3>    <p>13.1 AA Creations & Events will take reasonable care when providing its services.</p>    <p>13.2 Nothing in these Terms and Conditions is intended to exclude or limit any liability or consumer right that cannot legally be excluded or limited under South African law.</p>    <p>13.3 AA Creations & Events will not be responsible for losses caused by information supplied incorrectly by the client, venue restrictions that were not disclosed, or circumstances outside the reasonable control of the business, to the extent permitted by law.</p>    <h3>14. CHANGES TO THESE TERMS AND CONDITIONS</h3>    <p>14.1 AA Creations & Events may update these Terms and Conditions from time to time.</p>    <p>14.2 The version applicable to a confirmed booking will generally be the version accepted by the client at the time of that booking, subject to any changes required by law.</p>    <p>14.3 Clients will be given reasonable access to the applicable Terms and Conditions.</p>    <h3>15. GOVERNING LAW</h3>    <p>15.1 These Terms and Conditions are governed by the laws of the <strong>Republic of South Africa</strong>.</p>    <p>15.2 Any dispute arising from a booking or service will be dealt with in accordance with applicable South African law.</p>    <p>15.3 Nothing in these Terms and Conditions prevents a consumer from exercising rights provided by applicable South African consumer-protection legislation.</p>    <h3>16. ACCEPTANCE OF TERMS</h3>    <p>By confirming a booking, making a payment, submitting an online order or otherwise accepting the services of AA Creations & Events, the client confirms that they have had an opportunity to read and understand these Terms and Conditions.</p>    <p><strong>AA Creations & Events</strong></p>  <p>Event Decoration &amp; Event Services</p>    <p><strong>Last Updated:</strong> 03 October 2026</p>  ";

            var escapedTermsV1Content =
                termsV1Content.Replace("'", "''");

            Sql(@"
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.TermsAndConditions
    WHERE Version = N'1.0'
)
BEGIN
    INSERT INTO dbo.TermsAndConditions
    (
        Version,
        Content,
        EffectiveDate,
        IsActive,
        CreatedDate
    )
    VALUES
    (
        N'1.0',
        N'" + escapedTermsV1Content + @"',
        CONVERT(datetime, '2026-10-03T00:00:00.000', 126),
        1,
        CONVERT(datetime, '2026-10-03T22:46:02.637', 126)
    );
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
