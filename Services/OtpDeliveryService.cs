using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Services
{
    public class OtpDeliveryService
    {
        public bool SendOtpByEmail(string email, string otp)
        {
            // Email provider will be integrated later.
            // For now, return true so the OTP flow can be tested.
            return true;
        }

        public bool SendOtpByPhone(string phone, string otp)
        {
            // SMS/WhatsApp provider will be integrated later.
            // For now, return true so the OTP flow can be tested.
            return true;
        }
    }
}