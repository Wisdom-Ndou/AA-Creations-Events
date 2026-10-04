using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace WebApplication1.Services
{
    public class OtpDeliveryService
    {
        private readonly string senderEmail;
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string appPassword;

        public string LastError { get; private set; }

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
            string environmentValue = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue.Trim();
            }

            string configValue = ConfigurationManager.AppSettings[appSettingKey];
            return string.IsNullOrWhiteSpace(configValue)
                ? null
                : configValue.Trim();
        }

        private bool SendEmail(string recipientEmail, string subject, string body)
        {
            LastError = null;

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
                        smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
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
                    "AA Creations SMTP delivery failed. Recipient={0}; Subject={1}; StatusCode={2}; Error={3}",
                    recipientEmail,
                    subject,
                    ex.StatusCode,
                    ex);

                LastError = ex.StatusCode == SmtpStatusCode.GeneralFailure
                    ? "The email server could not be reached or rejected the connection. Check the SMTP host, port and internet connection."
                    : "The email server rejected the message. Check the sender email and app password.";
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
