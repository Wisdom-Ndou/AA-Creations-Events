using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Linq;

namespace WebApplication1.Services
{
    public class OtpDeliveryService
    {
        private readonly string senderEmail;
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string appPassword;

        public string LastError { get; private set; }
        public string LastDiagnostic { get; private set; }

        public OtpDeliveryService()
        {
            senderEmail = GetSetting("AA_EMAIL_SENDER", "EmailSender");
            smtpHost = GetSetting("AA_EMAIL_SMTP_HOST", "EmailSmtpHost");

            int port;
            if (!int.TryParse(GetSetting("AA_EMAIL_SMTP_PORT", "EmailSmtpPort"), out port))
            {
                port = 587;
            }

            smtpPort = port;
            appPassword = GetSetting("AA_EMAIL_APP_PASSWORD", "EmailAppPassword");
        }

        private static string GetSetting(string environmentVariable, string appSettingKey)
        {
            string value = Environment.GetEnvironmentVariable(
                environmentVariable,
                EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable(
                    environmentVariable,
                    EnvironmentVariableTarget.User);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable(
                    environmentVariable,
                    EnvironmentVariableTarget.Machine);
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            string configValue = ConfigurationManager.AppSettings[appSettingKey];
            return string.IsNullOrWhiteSpace(configValue)
                ? null
                : configValue.Trim();
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(senderEmail) &&
                       !string.IsNullOrWhiteSpace(appPassword) &&
                       !string.IsNullOrWhiteSpace(smtpHost);
            }
        }

        public string SenderDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(senderEmail))
                {
                    return "(not configured)";
                }

                int at = senderEmail.IndexOf('@');
                if (at <= 1)
                {
                    return "***";
                }

                return senderEmail.Substring(0, 1) +
                       new string('*', Math.Max(3, at - 1)) +
                       senderEmail.Substring(at);
            }
        }

        public string SmtpHost { get { return smtpHost; } }

        public int SmtpPort { get { return smtpPort; } }

        public bool HasPassword { get { return !string.IsNullOrWhiteSpace(appPassword); } }

        private bool SendEmail(string recipientEmail, string subject, string body)
        {
            LastError = null;
            LastDiagnostic = null;

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                LastError = "The recipient email address is missing.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(appPassword))
            {
                LastError =
                    "Email delivery is not configured. Set AA_EMAIL_SENDER and AA_EMAIL_APP_PASSWORD " +
                    "as local environment variables, or set EmailSender and EmailAppPassword in the local Web.config.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                LastError = "The SMTP host is not configured.";
                return false;
            }

            try
            {
                // Explicit TLS 1.2 support helps older .NET Framework/IIS Express environments.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail, "AA Creations & Events");
                    message.To.Add(new MailAddress(recipientEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        string credentialPassword = appPassword;

                        // Google displays app passwords in groups. Ignore whitespace
                        // when Gmail is the configured SMTP provider.
                        if (!string.IsNullOrWhiteSpace(smtpHost) &&
                            smtpHost.IndexOf("gmail", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            credentialPassword = new string(
                                appPassword.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
                        }

                        smtp.Credentials = new NetworkCredential(senderEmail, credentialPassword);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Timeout = 20000;
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (SmtpException ex)
            {
                Trace.TraceError(
                    "AA Creations SMTP delivery failed. StatusCode={0}; ErrorType={1}; Message={2}",
                    ex.StatusCode,
                    ex.GetType().FullName,
                    ex.Message);

                LastDiagnostic = BuildDiagnostic(ex);

                if (ex.InnerException is SocketException)
                {
                    LastError =
                        "The application could not open a network connection to Gmail SMTP. " +
                        "Check whether smtp.gmail.com on port " + smtpPort +
                        " is reachable from this computer/network.";
                }
                else if (ex.InnerException != null &&
                         ex.InnerException.GetType().Name.IndexOf("Authentication", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    LastError =
                        "The secure connection to Gmail failed during TLS/SSL negotiation. " +
                        "Check Windows/.NET TLS support and any antivirus or firewall that inspects encrypted mail traffic.";
                }
                else if (ex.StatusCode == SmtpStatusCode.GeneralFailure)
                {
                    LastError =
                        "Gmail SMTP could not complete the connection. Use the Email SMTP Test endpoint to see the underlying network/TLS error.";
                }
                else
                {
                    LastError =
                        "Gmail rejected the SMTP request. Verify that the sender account uses a valid Google App Password and that 2-Step Verification is enabled.";
                }

                return false;
            }
            catch (FormatException ex)
            {
                Trace.TraceError(
                    "AA Creations email address format error. Recipient={0}; Subject={1}; Error={2}",
                    recipientEmail,
                    subject,
                    ex);

                LastError = "The sender or recipient email address is not valid.";
                return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "AA Creations email delivery failed. Recipient={0}; Subject={1}; Error={2}",
                    recipientEmail,
                    subject,
                    ex);

                LastError = "The verification email could not be sent because the mail service returned an unexpected error.";
                return false;
            }
        }

        private static string BuildDiagnostic(Exception ex)
        {
            var parts = new System.Collections.Generic.List<string>();
            Exception current = ex;
            int depth = 0;

            while (current != null && depth < 4)
            {
                parts.Add(current.GetType().Name + ": " + current.Message);
                current = current.InnerException;
                depth++;
            }

            return string.Join(" -> ", parts);
        }

        public bool SendDiagnosticEmail(string email)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Email Test",
                "<h2>Email test successful</h2><p>Your AA Creations & Events SMTP configuration is working.</p>");
        }

        public bool SendOtpByEmail(string email, string otp)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Verification Code",
                $@"<h2>AA Creations & Events</h2>
<p>Your verification code is:</p>
<h1>{otp}</h1>
<p>This code will expire in 5 minutes.</p>
<p>If you did not request this code, you can ignore this email.</p>");
        }

        public bool SendRegistrationEmail(string email, string firstName)
        {
            return SendEmail(
                email,
                "Welcome to AA Creations & Events",
                $@"<h2>Welcome to AA Creations & Events!</h2>
<p>Hi {firstName},</p>
<p>Your account has been successfully created.</p>
<p>You can now sign in and start making bookings for your events.</p>
<p>Thank you for choosing AA Creations & Events.</p>");
        }

        public bool SendAdminRegistrationEmail(string email, string firstName)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Admin Account Created",
                $@"<h2>Admin Account Created</h2>
<p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>
<p>Your AA Creations & Events administrator account has been successfully created.</p>
<p>You can now sign in from the Admin option on the login page using the email address registered for this account and the administrator authorization code.</p>
<p>If you did not expect this account to be created, please contact AA Creations & Events.</p>");
        }

        public bool SendStaffRegistrationEmail(
            string email,
            string firstName,
            string team,
            string city)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Staff Account Created",
                $@"<h2>Staff Account Created</h2>
<p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>
<p>Your AA Creations & Events staff account has been successfully created.</p>
<p><strong>Team:</strong> {System.Net.WebUtility.HtmlEncode(team)}</p>
<p><strong>Assigned City:</strong> {System.Net.WebUtility.HtmlEncode(city)}</p>
<p>You can now sign in through the Staff sign-in page using the email address registered for this account.</p>
<p>Welcome to the AA Creations & Events team.</p>");
        }

        public bool SendCustomerComplaintConfirmationEmail(
            string email,
            string firstName,
            int complaintId,
            int bookingId,
            string category,
            string subject,
            DateTime submittedAt)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Complaint Received",
                $@"<h2>Complaint Received</h2>
<p>Hi {firstName},</p>
<p>We have received your complaint successfully and linked it to your booking.</p>
<p><strong>Complaint Reference:</strong> #CP-{complaintId}</p>
<p><strong>Booking Reference:</strong> #BK-{bookingId}</p>
<p><strong>Category:</strong> {System.Net.WebUtility.HtmlEncode(category)}</p>
<p><strong>Subject:</strong> {System.Net.WebUtility.HtmlEncode(subject)}</p>
<p><strong>Submitted:</strong> {submittedAt:dd MMMM yyyy 'at' HH:mm}</p>
<p>Our admin team will review the complaint and any response will appear in your Complaint History.</p>
<p>Thank you for contacting AA Creations & Events.</p>");
        }

        public bool SendBookingCancellationEmail(
            string email,
            string firstName,
            int bookingId,
            string occasion,
            DateTime eventDate,
            string cancellationReason,
            decimal amountPaid,
            decimal cancellationCharge,
            decimal refundAmount)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Booking Cancelled",
                $@"<h2>Booking Cancelled</h2>
<p>Hi {firstName},</p>
<p>Your booking has been cancelled successfully.</p>
<p><strong>Booking Reference:</strong> #{bookingId}</p>
<p><strong>Occasion:</strong> {occasion}</p>
<p><strong>Event Date:</strong> {eventDate:dd MMMM yyyy}</p>
<p><strong>Cancellation Reason:</strong> {System.Net.WebUtility.HtmlEncode(cancellationReason)}</p>
<hr />
<p><strong>Amount Paid:</strong> R {amountPaid:N2}</p>
<p><strong>Cancellation Charge:</strong> R {cancellationCharge:N2}</p>
<p><strong>Refund Amount:</strong> R {refundAmount:N2}</p>
<p>The refund amount shown above is the amount due back to you based on the cancellation policy.</p>
<p>If you have any questions about your cancellation or refund, please contact AA Creations & Events.</p>");
        }

        public bool SendBookingConfirmationEmail(
            string email,
            string firstName,
            int bookingId,
            string occasion,
            DateTime eventDate,
            string eventTime,
            string city,
            decimal totalPrice)
        {
            return SendEmail(
                email,
                "AA Creations & Events - Booking Received",
                $@"<h2>Booking Received</h2>
<p>Hi {firstName},</p>
<p>Your booking has been received successfully and is awaiting review.</p>
<p><strong>Booking Reference:</strong> #{bookingId}</p>
<p><strong>Occasion:</strong> {occasion}</p>
<p><strong>Event Date:</strong> {eventDate:dd MMMM yyyy}</p>
<p><strong>Event Time:</strong> {eventTime}</p>
<p><strong>City:</strong> {city}</p>
<p><strong>Total:</strong> R {totalPrice:N2}</p>
<p>We will keep you updated as your booking progresses.</p>");
        }
    }
}
