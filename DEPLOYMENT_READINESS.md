# AA Creations & Events — Deployment Readiness

Branch: `chore/deployment-hardening`

Base branch: `fix/booking-city-location-on-chatgpt`

## Current deployment target

- Azure App Service (Windows)
- ASP.NET MVC 5 / .NET Framework
- Entity Framework 6
- Azure SQL Database
- Azure Key Vault / App Service settings for production secrets
- Application Insights for production monitoring
- Staging slot before production

## Completed hardening

- [x] Created dedicated deployment-hardening branch from the current deployment candidate.
- [x] Made the Staff migration capable of creating `dbo.Staffs` on a completely fresh database.
- [x] Added the generated `AddBookingPaymentsAndCustomerComplaints` migration to the Visual Studio project.
- [x] Made `TermsAndConditions` and `CustomerAgreements` reproducible through EF migrations.
- [x] Seeded the approved Terms & Conditions version 1.0 only when that version is missing.
- [x] Preserved legal/acceptance data during migration rollback.
- [x] Removed the known/default `AACode` administrator fallback.
- [x] Added support for `AA_ADMIN_ACCESS_CODE` as a deployment environment secret.
- [x] Restricted administrator registration after the first administrator exists.
- [x] Replaced predictable OTP generation with a cryptographically secure RNG.
- [x] Added OTP resend throttling using persisted database history.
- [x] Reduced personal information written to SMTP failure logs.
- [x] Removed public SMTP/database diagnostic endpoints.
- [x] Added a safe `/health` endpoint for Azure App Service Health Check.
- [x] Converted admin/staff logout to POST + anti-forgery validation.
- [x] Added anti-forgery validation to booking creation.
- [x] Removed the dead `firebase-config.js` page include.
- [x] Prevented customer/admin/staff passwords and the admin authorization code from being reflected into HTML after validation errors.
- [x] Added hardened Release configuration for debug/errors/cookies/basic security headers.
- [x] Suppressed the ASP.NET version header.
- [x] Added Windows GitHub Actions Release CI.
- [x] CI verifies a Release build with NuGet restore + MSBuild.
- [x] CI creates a completely fresh SQL LocalDB database and runs all EF migrations from zero.
- [x] CI verifies the critical schema and seed data, including Terms v1.0, packages and add-ons.
- [x] CI deletes the throwaway database after the migration test.
- [x] CI validates the transformed Release publish configuration.

## Approved Terms seed

- Version: `1.0`
- Effective date: `03 October 2026`
- Active: yes
- Existing production/development rows are not overwritten when version 1.0 already exists.
- Future Terms changes must be inserted as new immutable versions rather than editing v1.0 in place.

## Remaining work for the assessed deployment

This application is being deployed as a school-project demonstration, not as a live commercial service.

### Simulated payments

The simulated payment workflow is intentional and may remain enabled for the assessment.

- The booking wizard simulates deposit/full-payment choices and writes the resulting payment state to the database.
- Card/CVV/billing values entered in the booking wizard are not sent or stored.
- The remaining-balance page now keeps its demo card fields browser-only and posts only the booking ID plus a simulation-confirmation flag.
- The old standalone banking route redirects to the active booking flow.
- No real payment gateway, merchant account or PCI certification is required for this assessed deployment.

### Mapping/geocoding

The current OpenStreetMap/Nominatim implementation may be used for the low-volume assessment/demo deployment. A commercial provider would only be necessary if the project later became a real public service with meaningful traffic.

### Login abuse protection

OTP requests are already throttled. Additional enterprise-grade login rate limiting/lockout is recommended for a real public service but is not a blocker for the assessed deployment.

### Time handling

The application uses `DateTime.Now` / `DateTime.Today` extensively. The Azure Windows App Service must therefore be configured with:

`WEBSITE_TIME_ZONE=South Africa Standard Time`

A future commercial system should preferably store persisted timestamps in UTC.

### Privacy/legal operational work

The existing Terms & Conditions and agreement tracking remain part of the demo. Formal privacy/compliance operations would be required before any real commercial use, but they are not a blocker for this school-project deployment.

## Azure configuration still to create

- [ ] Azure Resource Group
- [ ] Windows App Service Plan
- [ ] Production App Service
- [ ] Staging deployment slot
- [ ] Azure SQL Server
- [ ] Production Azure SQL database
- [ ] Staging Azure SQL database
- [ ] Azure Key Vault
- [ ] App Service managed identity
- [ ] Application Insights
- [ ] Production custom domain
- [ ] TLS certificate
- [ ] HTTPS Only
- [ ] TLS 1.2+
- [ ] Always On
- [ ] Health Check path: `/health`
- [ ] `WEBSITE_TIME_ZONE=South Africa Standard Time`
- [ ] Production and staging `AndiswaDB` connection strings configured separately and marked as slot settings
- [ ] `AA_ADMIN_ACCESS_CODE` configured as a production secret
- [ ] SMTP settings/secrets configured
- [ ] Azure SQL backup/restore validation completed

## Required pre-production test

A staging deployment is acceptable only after all of the following pass against Azure:

1. Registration and Terms popup.
2. Terms v1.0 acceptance is recorded.
3. Customer login/logout.
4. Forgot-password OTP and throttling.
5. Account contact-change OTP.
6. Booking submission and anti-forgery protection.
7. Address search and map pinning for every supported city.
8. Customer booking view/cancellation.
9. Customer complaints.
10. Admin authentication/booking approval.
11. Staff registration/login/task assignment.
12. Staff complaints.
13. Financial overview.
14. Email delivery.
15. `/health` returns HTTP 200 while app + database are healthy.
16. A routine app redeploy does not delete or reset production/staging database data.
17. Backup restore is tested before accepting real customer data.

## Release process

Do not deploy production directly from feature/fix branches.

Recommended flow:

```
feature/fix branch
        ↓
chore/deployment-hardening
        ↓
CI + clean database test + staging test
        ↓
main/release branch
        ↓
Azure staging slot
        ↓
manual acceptance
        ↓
production slot swap
```
