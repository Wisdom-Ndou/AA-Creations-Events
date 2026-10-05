using System;
using System.Security.Cryptography;
using System.Web.Helpers;

namespace WebApplication1.Helpers
{
    public static class OtpHelper
    {
        public static string GenerateOtp()
        {
            const uint otpRange = 900000;
            const uint upperBound = uint.MaxValue - (uint.MaxValue % otpRange);

            uint value;
            var bytes = new byte[4];

            using (var rng = RandomNumberGenerator.Create())
            {
                do
                {
                    rng.GetBytes(bytes);
                    value = BitConverter.ToUInt32(bytes, 0);
                }
                while (value >= upperBound);
            }

            return (100000 + (value % otpRange)).ToString("D6");
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
