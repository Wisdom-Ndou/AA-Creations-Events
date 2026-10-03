using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

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

            if (!int.TryParse(
                ConfigurationManager.AppSettings["EmailSmtpPort"],
                out port))
            {
                port = 587;
            }

            smtpPort = port;
        }

        private bool SendEmail(
            string recipientEmail,
            string subject,
            string body)
        {
            try
            {
                // We will configure this securely in the next step.
                string appPassword =
    ConfigurationManager.AppSettings["EmailAppPassword"];

                if (string.IsNullOrWhiteSpace(senderEmail) ||
                    string.IsNullOrWhiteSpace(appPassword))
                {
                    return false;
                }

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(
                        senderEmail,
                        "AA Creations & Events");

                    message.To.Add(recipientEmail);

                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(
                        smtpHost,
                        smtpPort))
                    {
                        smtp.EnableSsl = true;

                        smtp.UseDefaultCredentials = false;

                        smtp.Credentials =
                            new NetworkCredential(
                                senderEmail,
                                appPassword);

                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ==========================================
        // OTP EMAIL
        // ==========================================

        public bool SendOtpByEmail(
            string email,
            string otp)
        {
            string subject =
                "AA Creations & Events - Verification Code";

            string body = $@"
                <h2>AA Creations & Events</h2>

                <p>Your verification code is:</p>

                <h1>{otp}</h1>

                <p>
                    This code will expire in 5 minutes.
                </p>

                <p>
                    If you did not request this code,
                    you can ignore this email.
                </p>";

            return SendEmail(
                email,
                subject,
                body);
        }

        // ==========================================
        // REGISTRATION EMAIL
        // ==========================================

        public bool SendRegistrationEmail(
            string email,
            string firstName)
        {
            string subject =
                "Welcome to AA Creations & Events";

            string body = $@"
                <h2>Welcome to AA Creations & Events!</h2>

                <p>Hi {firstName},</p>

                <p>
                    Your account has been successfully created.
                </p>

                <p>
                    You can now sign in and start making
                    bookings for your events.
                </p>

                <p>
                    Thank you for choosing
                    AA Creations & Events.
                </p>";

            return SendEmail(
                email,
                subject,
                body);
        }

        // ==========================================
        // BOOKING CONFIRMATION EMAIL
        // ==========================================

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
            string subject =
                "AA Creations & Events - Booking Confirmation";

            string body = $@"
                <h2>Booking Confirmation</h2>

                <p>Hi {firstName},</p>

                <p>
                    Your booking has been received successfully.
                </p>

                <p>
                    <strong>Booking Reference:</strong>
                    #{bookingId}
                </p>

                <p>
                    <strong>Occasion:</strong>
                    {occasion}
                </p>

                <p>
                    <strong>Event Date:</strong>
                    {eventDate:dd MMMM yyyy}
                </p>

                <p>
                    <strong>Event Time:</strong>
                    {eventTime}
                </p>

                <p>
                    <strong>City:</strong>
                    {city}
                </p>

                <p>
                    <strong>Total:</strong>
                    R {totalPrice:N2}
                </p>

                <p>
                    Thank you for choosing
                    AA Creations & Events.
                </p>";

            return SendEmail(
                email,
                subject,
                body);
        }

        // ==========================================
        // PHONE OTP PLACEHOLDER
        // ==========================================

        public bool SendOtpByPhone(
            string phone,
            string otp)
        {
            // SMS/WhatsApp provider will be
            // integrated later.
            return true;
        }
    }
}