using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace WebApplication1.Services
{
    public class OtpDeliveryService
    {
        private readonly string senderEmail;
        private readonly string smtpHost;
        private readonly int smtpPort;

        public OtpDeliveryService()
        {
            senderEmail = ConfigurationManager.AppSettings["EmailSender"];
            smtpHost = ConfigurationManager.AppSettings["EmailSmtpHost"];

            int port;
            if (!int.TryParse(ConfigurationManager.AppSettings["EmailSmtpPort"], out port))
            {
                port = 587;
            }

            smtpPort = port;
        }

        private bool SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                string appPassword = ConfigurationManager.AppSettings["EmailAppPassword"];

                if (string.IsNullOrWhiteSpace(recipientEmail) ||
                    string.IsNullOrWhiteSpace(senderEmail) ||
                    string.IsNullOrWhiteSpace(smtpHost) ||
                    string.IsNullOrWhiteSpace(appPassword))
                {
                    return false;
                }

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail, "AA Creations & Events");
                    message.To.Add(recipientEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Email delivery must not undo a successful registration/booking,
                // but failures must be diagnosable rather than silently swallowed.
                Trace.TraceError(
                    "AA Creations email delivery failed. Recipient={0}; Subject={1}; Error={2}",
                    recipientEmail,
                    subject,
                    ex);
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
