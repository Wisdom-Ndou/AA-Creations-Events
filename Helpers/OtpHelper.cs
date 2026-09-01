using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace WebApplication1.Helpers
{
    public static class OtpHelper
    {
        public static string GenerateOtp()
        {
            Random random = new Random();

            return random.Next(100000, 1000000).ToString();
        }

        public static string HashOtp(string otp)
        {
            return Crypto.HashPassword(otp);
        }

        public static bool VerifyOtp(string otp, string hashedOtp)
        {
            try
            {
                return Crypto.VerifyHashedPassword(hashedOtp, otp);
            }
            catch
            {
                return false;
            }
        }

        public static DateTime GetExpiryTime()
        {
            return DateTime.Now.AddMinutes(5);
        }
    }
}