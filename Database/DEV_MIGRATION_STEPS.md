# Development database migration

The `chatgpt/aa-creations-improvements` branch changes the EF6 model for booking payment tracking and customer complaints.

After pulling this branch, generate a normal EF6 migration from the current model so Visual Studio creates the required `.cs`, `.Designer.cs`, and `.resx` metadata together.

In Package Manager Console:

```powershell
Add-Migration AddBookingPaymentsAndCustomerComplaints
Update-Database
```

The generated migration should include:
- Booking.AmountPaid
- Booking.BalanceDueDate
- Booking.PaymentStatus
- Booking.CancellationCharge
- Booking.RefundAmount
- Booking.TermsVersion
- Booking.TermsAcceptedAt
- CustomerComplaints table and its Customer/Booking foreign keys

Before applying it to shared/production data, review the generated migration. Existing bookings pre-date payment tracking, so decide whether they should be backfilled as fully paid before deployment.
