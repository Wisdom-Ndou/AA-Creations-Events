using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Xml.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.Services;
using static WebApplication1.Models.Bankingdetailsviewmodel;

namespace WebApplication1.Controllers
{
    public class CustController : Controller
    {
        private readonly DatabaseContext db = new DatabaseContext();

        private static string GetAdminAccessCode()
        {
            var configured =
                ConfigurationManager.AppSettings["AdminAccessCode"];

            // Keep local development compatible with the existing project
            // default while using Web.config whenever it is supplied.
            return string.IsNullOrWhiteSpace(configured)
                ? "AACode"
                : configured.Trim();
        }

        private static bool IsValidAdminAccessCode(string suppliedCode)
        {
            if (string.IsNullOrWhiteSpace(suppliedCode))
                return false;

            return string.Equals(
                suppliedCode.Trim(),
                GetAdminAccessCode(),
                StringComparison.Ordinal);
        }

        [HttpPost]
        public ActionResult Bankingdetails()
        {
            var model = new BankingDetailsViewModel(); // or fetch/populate as needed
            return View(model);
        }
        // GET: Cust
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Customer obj)
        {
            return View(obj);
        }

        [HttpGet]
        public ActionResult CustomerDashboard()
        {
            if (Session["CustomerId"] == null ||
                Session["CustomerAuthenticated"] == null ||
                !(bool)Session["CustomerAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];
            var customer = db.Customers.FirstOrDefault(c => c.Cust_ID == customerId);
            if (customer == null)
            {
                ClearRoleSessions();
                return RedirectToAction("Login", "Cust");
            }

            DateTime today = DateTime.Today;
            var bookings = db.Bookings
                .Where(b => b.CustomerId == customerId)
                .Include("Package")
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            ViewBag.Customer = customer;
            ViewBag.TotalBookings = bookings.Count;
            ViewBag.UpcomingBookings = bookings.Count(b =>
                b.EventDate >= today &&
                !string.Equals(b.Status, "Declined", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));
            ViewBag.BalanceOutstanding = bookings
                .Where(b =>
                    !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(b.Status, "Declined", StringComparison.OrdinalIgnoreCase))
                .Sum(b => Math.Max(0m, b.TotalPrice - b.AmountPaid));
            ViewBag.OpenComplaints = db.CustomerComplaints.Count(x =>
                x.CustomerId == customerId &&
                x.Status != "Resolved" &&
                x.Status != "Closed");
            ViewBag.RecentBookings = bookings.Take(4).ToList();

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
    string email,
    string password,
    string role,
    string adminAccessCode)
        {
            // ==========================================
            // CHECK REQUIRED FIELDS
            // ==========================================

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Please enter your email address and password."
                );

                return View();
            }

            email = email.Trim();

            // ==========================================
            // CUSTOMER LOGIN
            // ==========================================

            if (role == "customer")
            {
                var customer = db.Customers
                    .FirstOrDefault(c => c.Cust_Email == email);

                if (customer == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email address or password."
                    );

                    return View();
                }

                bool passwordValid = false;

                try
                {
                    passwordValid =
                        Crypto.VerifyHashedPassword(
                            customer.Cust_Passw,
                            password
                        );
                }
                catch
                {
                    passwordValid = false;
                }

                if (!passwordValid)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email address or password."
                    );

                    return View();
                }

                ClearRoleSessions();

                // Store authenticated customer information
                Session["CustomerId"] =
                    customer.Cust_ID;

                Session["CustomerEmail"] =
                    customer.Cust_Email;

                Session["CustomerFirstName"] =
                    customer.Cust_FName;

                Session["CustomerAuthenticated"] =
                    true;

                return RedirectToAction(
                    "CustomerDashboard",
                    "Cust"
                );
            }

            // ==========================================
            // ADMIN LOGIN
            // ==========================================

            if (role == "admin")
            {
                if (string.IsNullOrWhiteSpace(adminAccessCode))
                {
                    ModelState.AddModelError(
                        "",
                        "Please enter the admin access code."
                    );

                    return View();
                }

                if (!IsValidAdminAccessCode(adminAccessCode))
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid admin authorization code."
                    );

                    return View();
                }

                // Find the registered admin
                var admin = db.Admins
                    .FirstOrDefault(a => a.admin_Email == email);

                if (admin == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email address or password."
                    );

                    return View();
                }

                // Verify the hashed password
                bool passwordValid = false;

                try
                {
                    passwordValid =
                        Crypto.VerifyHashedPassword(
                            admin.admin_Passw,
                            password
                        );
                }
                catch
                {
                    passwordValid = false;
                }

                if (!passwordValid)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email address or password."
                    );

                    return View();
                }

                // ==========================================
                // ADMIN AUTHENTICATED
                // ==========================================

                ClearRoleSessions();

                Session["AdminId"] =
                    admin.admin_ID;

                Session["AdminEmail"] =
                    admin.admin_Email;

                Session["AdminFirstName"] =
                    admin.admin_FName;

                Session["AdminAuthenticated"] =
                    true;

                return RedirectToAction(
                    "AdminDashboard",
                    "Cust"
                );
            }

            // ==========================================
            // INVALID ROLE
            // ==========================================

            ModelState.AddModelError(
                "",
                "Invalid login role."
            );

            return View();
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.ErrorMessage = "Please enter your email address.";
                return View();
            }

            string normalizedEmail = email.Trim();

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_Email == normalizedEmail);

            // Start a fresh Forgot Password OTP flow.
            Session.Remove("OtpVerified");
            Session.Remove("OtpPurpose");
            Session.Remove("ExpectedOtpPurpose");
            Session.Remove("OtpCustomerId");

            if (customer == null)
            {
                // Do not reveal whether a customer account exists for an email address.
                TempData["ForgotPasswordNotice"] =
                    "If an account exists for that email address, password recovery can continue by email.";

                return RedirectToAction("ForgotPassword", "Cust");
            }

            Session["OtpCustomerId"] = customer.Cust_ID;
            return RedirectToAction(
                "ForgotPasswordMethod",
                "Cust"
            );
        }

        [HttpGet]
        public ActionResult ForgotPasswordMethod()
        {
            if (Session["OtpCustomerId"] == null)
            {
                return RedirectToAction("ForgotPassword", "Cust");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPasswordMethod(string deliveryMethod)
        {
            if (Session["OtpCustomerId"] == null)
            {
                return RedirectToAction("ForgotPassword", "Cust");
            }

            if (deliveryMethod != "Email")
            {
                TempData["OtpError"] = "Password recovery is currently available by email.";
                return RedirectToAction("ForgotPasswordMethod", "Cust");
            }

            int customerId = (int)Session["OtpCustomerId"];

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Remove("OtpCustomerId");

                return RedirectToAction("ForgotPassword", "Cust");
            }

            string otp = OtpHelper.GenerateOtp();

            var otpVerification = new OtpVerification
            {
                CustomerId = customer.Cust_ID,
                OtpHash = OtpHelper.HashOtp(otp),
                DeliveryMethod = deliveryMethod,
                Purpose = "ForgotPassword",
                CreatedAt = DateTime.Now,
                ExpiresAt = OtpHelper.GetExpiryTime(),
                IsUsed = false,
                FailedAttempts = 0
            };

            // Only the newest unused code for this purpose should remain valid.
            var previousOtps = db.OtpVerifications
                .Where(o => o.CustomerId == customer.Cust_ID &&
                            o.Purpose == "ForgotPassword" &&
                            !o.IsUsed)
                .ToList();

            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;
            }

            db.OtpVerifications.Add(otpVerification);
            db.SaveChanges();
            Session["ExpectedOtpPurpose"] = "ForgotPassword";

            var deliveryService = new OtpDeliveryService();

            bool sent = deliveryService.SendOtpByEmail(
                customer.Cust_Email,
                otp
            );

            if (!sent)
            {
                // A code that was never delivered must never remain usable.
                otpVerification.IsUsed = true;
                db.SaveChanges();
                Session.Remove("ExpectedOtpPurpose");

                TempData["OtpError"] =
                    "We could not send the verification code. " +
                    (deliveryService.LastError ?? "Please check the email service configuration.");

                return RedirectToAction(
                    "ForgotPasswordMethod",
                    "Cust"
                );
            }

            return RedirectToAction(
                "VerifyOtp",
                "Cust"
            );
        }

        [HttpGet]
        public ActionResult Customerregister()
        {
            var currentTerms = GetCurrentTerms();
            ViewBag.CurrentTerms = currentTerms;
            ViewBag.TermsVersion = currentTerms == null ? null : currentTerms.Version;
            return View();
        }

        [HttpGet]
        public ActionResult TermsAndConditions()
        {
            ViewBag.CurrentTerms = GetCurrentTerms();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Customerregister(
            Customer obj,
            bool termsAccepted,
            string termsVersion,
            string cookiePreference,
            string confirm)
        {
            var currentTerms = GetCurrentTerms();
            ViewBag.CurrentTerms = currentTerms;
            ViewBag.TermsVersion = currentTerms == null ? null : currentTerms.Version;

            if (currentTerms == null)
            {
                ModelState.AddModelError("", "The Terms & Conditions are currently unavailable. Please try again later.");
                return View(obj);
            }

            if (!termsAccepted)
            {
                ModelState.AddModelError("", "You must agree to the Terms & Conditions before creating an account.");
                return View(obj);
            }

            if (!string.Equals(termsVersion, currentTerms.Version, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "The Terms & Conditions have been updated. Please review and accept the latest version.");
                return View(obj);
            }

            if (!string.Equals(
                    obj == null ? null : obj.Cust_Passw,
                    confirm,
                    StringComparison.Ordinal))
            {
                ModelState.AddModelError("confirm", "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            if (cookiePreference != "necessary" && cookiePreference != "all")
            {
                cookiePreference = "necessary";
            }

            if (string.IsNullOrWhiteSpace(obj.Cust_Passw) ||
                obj.Cust_Passw.Length < 6 ||
                obj.Cust_Passw.Length > 15 ||
                !obj.Cust_Passw.Any(char.IsLetter) ||
                !obj.Cust_Passw.Any(char.IsDigit))
            {
                ModelState.AddModelError("Cust_Passw",
                    "Password must be 6 to 15 characters long and contain at least one letter and one number.");
                return View(obj);
            }

            if (db.Customers.Any(x => x.Cust_Email == obj.Cust_Email))
            {
                ModelState.AddModelError("Cust_Email", "An account with this email address already exists.");
                return View(obj);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    obj.Cust_Passw = Crypto.HashPassword(obj.Cust_Passw);
                    obj.CreatedAt = DateTime.Now;

                    db.Customers.Add(obj);
                    db.SaveChanges();

                    db.CustomerAgreements.Add(new CustomerAgreement
                    {
                        CustomerId = obj.Cust_ID,
                        TermsAccepted = true,
                        TermsVersion = currentTerms.Version,
                        AcceptedAt = DateTime.Now,
                        CookiePreference = cookiePreference
                    });

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            var registrationEmailService = new OtpDeliveryService();
            bool registrationEmailSent = registrationEmailService.SendRegistrationEmail(
                obj.Cust_Email,
                obj.Cust_FName
            );

            TempData["RegistrationSuccess"] = registrationEmailSent
                ? "Your registration was successful. A confirmation email has been sent to you."
                : "Your registration was successful. You can now sign in and start booking.";

            return RedirectToAction("Customerregister", "Cust");
        }

        private TermsAndConditions GetCurrentTerms()
        {
            return db.TermsAndConditions
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.EffectiveDate)
                .ThenByDescending(t => t.Terms_ID)
                .FirstOrDefault();
        }


        // ===============================
        // ADMIN REGISTRATION - POST
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adminregister(
            string firstName,
            string lastName,
            string email,
            string phone,
            string password,
            string confirm,
            bool? termsAccepted,
            string adminAccessCode)
        {
            firstName = (firstName ?? string.Empty).Trim();
            lastName = (lastName ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            phone = (phone ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(firstName) ||
                !firstName.All(char.IsLetter))
            {
                ModelState.AddModelError("firstName", "Please enter a valid first name using letters only.");
            }

            if (string.IsNullOrWhiteSpace(lastName) ||
                !lastName.All(char.IsLetter))
            {
                ModelState.AddModelError("lastName", "Please enter a valid last name using letters only.");
            }

            var emailValidator =
                new System.ComponentModel.DataAnnotations.EmailAddressAttribute();

            if (string.IsNullOrWhiteSpace(email) ||
                !emailValidator.IsValid(email))
            {
                ModelState.AddModelError("email", "Please enter a valid email address.");
            }

            if (phone.Length != 9 ||
                !phone.All(char.IsDigit) ||
                phone.StartsWith("0"))
            {
                ModelState.AddModelError(
                    "phone",
                    "Enter a 9-digit South African mobile number without the leading 0.");
            }

            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8 ||
                password.Count(char.IsUpper) < 2 ||
                !password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ModelState.AddModelError(
                    "password",
                    "Password must be at least 8 characters and include at least 2 uppercase letters and 1 special character.");
            }

            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                ModelState.AddModelError("confirm", "Passwords do not match.");
            }

            if (termsAccepted != true)
            {
                ModelState.AddModelError("termsAccepted", "Please accept the Admin Terms of Use.");
            }

            if (string.IsNullOrWhiteSpace(adminAccessCode))
            {
                ModelState.AddModelError("adminAccessCode", "Please enter the admin authorization code.");
            }
            else if (!IsValidAdminAccessCode(adminAccessCode))
            {
                ModelState.AddModelError("adminAccessCode", "Invalid admin authorization code.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (db.Admins.Any(a => a.admin_Email == email))
            {
                ModelState.AddModelError(
                    "email",
                    "An administrator with this email address already exists.");
                return View();
            }

            var admin = new Admin
            {
                admin_FName = firstName,
                admin_LName = lastName,
                admin_Email = email,
                admin_Passw = Crypto.HashPassword(password),
                admin_Phone = phone
            };

            try
            {
                db.Admins.Add(admin);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Admin registration failed for {0}: {1}",
                    email,
                    ex);

                ModelState.AddModelError(
                    "",
                    "We could not create the admin account. Please try again. If the problem continues, make sure the database migrations are up to date.");
                return View();
            }

            var adminRegistrationEmailService = new OtpDeliveryService();
            bool adminRegistrationEmailSent =
                adminRegistrationEmailService.SendAdminRegistrationEmail(
                    admin.admin_Email,
                    admin.admin_FName);

            TempData["LoginSuccess"] = adminRegistrationEmailSent
                ? "Admin registration was successful. A confirmation email has been sent to " + admin.admin_Email + "."
                : "Admin registration was successful, but the confirmation email could not be sent right now. You can still sign in.";

            return RedirectToAction("Login", "Cust");
        }


        public ActionResult Portfolio(Customer obj)
        {
            return View(obj);
        }

        public ActionResult Booking()
        {
            if (Session["CustomerId"] == null)
            {
                return View("Booking", null);
            }

            var customerId = (int)Session["CustomerId"];

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Cust");
            }

            ViewBag.CustomerFirstName = customer.Cust_FName;
            ViewBag.CustomerLastName = customer.Cust_LName;
            ViewBag.CustomerEmail = customer.Cust_Email;
            ViewBag.CustomerPhone = customer.Cust_Phone;

            return View();
        }

        // NEW: ContactUs page action (GET)
        [HttpGet]
        public ActionResult ContactUs()
        {
            // By default this will return Views/Cust/ContactUs.cshtml
            return View();
        }

        // ==========================================================
        // EVENT LOCATION / ADDRESS LOOKUP
        // ==========================================================

        private static readonly HashSet<string> AllowedEventCities =
       new HashSet<string>(StringComparer.OrdinalIgnoreCase)
       {
        "Durban",
        "Pietermaritzburg",
        "Mandeni",
        "eMandeni"
       };

        

        

        private static string NormalizeEventCity(string city)
        {
            return (city ?? string.Empty).Trim();
        }

        private static bool IsAllowedEventCity(string city)
        {
            return AllowedEventCities.Contains(
                NormalizeEventCity(city));
        }

        private static bool TryGetEventCityServiceArea(
            string city,
            out double centerLatitude,
            out double centerLongitude,
            out double radiusKm)
        {
            centerLatitude = 0;
            centerLongitude = 0;
            radiusKm = 0;

            switch (NormalizeEventCity(city).ToLowerInvariant())
            {
                case "durban":
                    centerLatitude = -29.8587;
                    centerLongitude = 31.0218;
                    radiusKm = 30;
                    return true;

                case "pietermaritzburg":
                    centerLatitude = -29.6006;
                    centerLongitude = 30.3794;
                    radiusKm = 20;
                    return true;

                case "mandeni":
                case "emandeni":
                    centerLatitude = -29.1460;
                    centerLongitude = 31.4070;
                    radiusKm = 15;
                    return true;

                default:
                    return false;
            }
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double CalculateDistanceKm(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            const double earthRadiusKm = 6371.0;

            var lat1 = DegreesToRadians(latitude1);
            var lat2 = DegreesToRadians(latitude2);
            var deltaLat = DegreesToRadians(latitude2 - latitude1);
            var deltaLon = DegreesToRadians(longitude2 - longitude1);

            var a =
                Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var centralAngle =
                2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * centralAngle;
        }

        private static bool IsWithinSelectedCityServiceArea(
            string city,
            double latitude,
            double longitude)
        {
            double centerLatitude;
            double centerLongitude;
            double radiusKm;

            if (!TryGetEventCityServiceArea(
                    city,
                    out centerLatitude,
                    out centerLongitude,
                    out radiusKm))
            {
                return false;
            }

            return CalculateDistanceKm(
                centerLatitude,
                centerLongitude,
                latitude,
                longitude) <= radiusKm;
        }

        private static string GetEventCitySearchViewbox(string city)
        {
            double centerLatitude;
            double centerLongitude;
            double radiusKm;

            if (!TryGetEventCityServiceArea(
                    city,
                    out centerLatitude,
                    out centerLongitude,
                    out radiusKm))
            {
                return string.Empty;
            }

            // Nominatim viewbox format: left,top,right,bottom.
            // Derive it from the same service radius used for pin and booking
            // validation so search cannot suggest places from a much wider area.
            var latitudeDelta = radiusKm / 111.0;
            var longitudeScale =
                111.0 * Math.Cos(DegreesToRadians(centerLatitude));
            var longitudeDelta =
                longitudeScale > 0 ? radiusKm / longitudeScale : latitudeDelta;

            var left = centerLongitude - longitudeDelta;
            var right = centerLongitude + longitudeDelta;
            var top = centerLatitude + latitudeDelta;
            var bottom = centerLatitude - latitudeDelta;

            return string.Join(",",
                left.ToString(System.Globalization.CultureInfo.InvariantCulture),
                top.ToString(System.Globalization.CultureInfo.InvariantCulture),
                right.ToString(System.Globalization.CultureInfo.InvariantCulture),
                bottom.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }


        private static string GetGeocodedCity(JObject feature)
        {
            var geocoding = feature["properties"]?["geocoding"] as JObject;

            if (geocoding == null)
                return string.Empty;

            // geocodejson normally gives us "city". The fallbacks help with
            // places that are classified as a town/village by OpenStreetMap.
            string[] fields = { "city", "town", "village", "locality" };

            foreach (var field in fields)
            {
                var value = geocoding[field]?.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private static string GetGeocodedLabel(JObject feature)
        {
            var geocoding = feature["properties"]?["geocoding"] as JObject;

            var label = geocoding?["label"]?.ToString();

            if (!string.IsNullOrWhiteSpace(label))
                return label.Trim();

            return feature["properties"]?["label"]?.ToString()?.Trim()
                   ?? string.Empty;
        }

        

        private static JObject GetFirstNominatimFeature(string url)
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;

                // Nominatim requires an identifying User-Agent.
                client.Headers["User-Agent"] =
                    "AA-Creations-Events/1.0 (booking address lookup)";

                client.Headers["Accept"] = "application/json";
                client.Headers["Accept-Language"] = "en";

                var json = client.DownloadString(url);
                var root = JObject.Parse(json);
                var features = root["features"] as JArray;

                if (features == null || features.Count == 0)
                    return null;

                return features[0] as JObject;
            }
        }

        private static JArray SearchNominatimFeatures(
            string query,
            string viewbox,
            int limit)
        {
            var url =
                "https://nominatim.openstreetmap.org/search" +
                "?format=geocodejson" +
                "&addressdetails=1" +
                "&countrycodes=za" +
                "&dedupe=1" +
                "&limit=" + limit +
                (string.IsNullOrWhiteSpace(viewbox)
                    ? ""
                    : "&bounded=1&viewbox=" + Uri.EscapeDataString(viewbox)) +
                "&q=" + Uri.EscapeDataString(query);

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers["User-Agent"] =
                    "AA-Creations-Events/1.0 (booking address lookup)";
                client.Headers["Accept"] = "application/json";
                client.Headers["Accept-Language"] = "en";

                var root = JObject.Parse(client.DownloadString(url));
                return root["features"] as JArray ?? new JArray();
            }
        }

        [HttpGet]
        public JsonResult SearchEventAddresses(string address, string city)
        {
            city = NormalizeEventCity(city);

            if (!IsAllowedEventCity(city))
            {
                return Json(new
                {
                    success = false,
                    message = "Please select Durban, Pietermaritzburg or Mandeni first."
                }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(address) || address.Trim().Length < 3)
            {
                return Json(new
                {
                    success = true,
                    results = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var typedAddress = address.Trim();
                var viewbox = GetEventCitySearchViewbox(city);

                // Try the user's exact text both with and without the selected
                // city. Both searches are bounded to the same service area
                // used by map pins and final booking validation.
                var queries = new[]
                {
                    typedAddress + ", " + city + ", KwaZulu-Natal, South Africa",
                    typedAddress
                };

                var results = new List<object>();
                var seenCoordinates = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

                foreach (var query in queries)
                {
                    var features = SearchNominatimFeatures(
                        query,
                        viewbox,
                        15);

                    foreach (var token in features)
                    {
                        var feature = token as JObject;
                        var coordinates =
                            feature?["geometry"]?["coordinates"] as JArray;

                        if (feature == null ||
                            coordinates == null ||
                            coordinates.Count < 2)
                        {
                            continue;
                        }

                        double longitude;
                        double latitude;

                        if (!double.TryParse(
                                coordinates[0]?.ToString(),
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out longitude) ||
                            !double.TryParse(
                                coordinates[1]?.ToString(),
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out latitude))
                        {
                            continue;
                        }

                        // This is the exact same rule used by map pin validation
                        // and CreateBooking. Never show a suggestion the user
                        // will immediately be told they cannot select.
                        if (!IsWithinSelectedCityServiceArea(
                                city,
                                latitude,
                                longitude))
                        {
                            continue;
                        }

                        var coordinateKey =
                            Math.Round(latitude, 6)
                                .ToString(System.Globalization.CultureInfo.InvariantCulture) +
                            "," +
                            Math.Round(longitude, 6)
                                .ToString(System.Globalization.CultureInfo.InvariantCulture);

                        if (!seenCoordinates.Add(coordinateKey))
                            continue;

                        results.Add(new
                        {
                            address = GetGeocodedLabel(feature),
                            latitude = latitude,
                            longitude = longitude,
                            resolvedCity = GetGeocodedCity(feature),
                            inSelectedCity = true
                        });

                        if (results.Count >= 12)
                            break;
                    }

                    if (results.Count >= 12)
                        break;
                }

                return Json(new
                {
                    success = true,
                    results = results
                }, JsonRequestBehavior.AllowGet);
            }
            catch (WebException)
            {
                return Json(new
                {
                    success = false,
                    message = "The address service is temporarily unavailable. Please try again or pin the location on the map."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "We could not search for that address right now. Please try again."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ReverseEventLocation(
            double latitude,
            double longitude,
            string city)
        {
            city = NormalizeEventCity(city);

            if (!IsAllowedEventCity(city))
            {
                return Json(new
                {
                    success = false,
                    message = "Please select a valid city/town first."
                }, JsonRequestBehavior.AllowGet);
            }

            if (latitude < -90 || latitude > 90 ||
                longitude < -180 || longitude > 180)
            {
                return Json(new
                {
                    success = false,
                    message = "The selected map coordinates are invalid."
                }, JsonRequestBehavior.AllowGet);
            }

            var inSelectedCity = IsWithinSelectedCityServiceArea(
                city,
                latitude,
                longitude);

            if (!inSelectedCity)
            {
                return Json(new
                {
                    success = true,
                    address = "",
                    resolvedCity = "",
                    latitude = latitude,
                    longitude = longitude,
                    inSelectedCity = false,
                    geocodeAvailable = false
                }, JsonRequestBehavior.AllowGet);
            }

            // The pin itself is already valid at this point. Reverse
            // geocoding is optional enrichment only: if the external address
            // service is unavailable, keep the customer's manual address and
            // still allow the valid pin to be confirmed.
            try
            {
                var url =
                    "https://nominatim.openstreetmap.org/reverse" +
                    "?format=geocodejson" +
                    "&addressdetails=1" +
                    "&zoom=18" +
                    "&lat=" +
                    latitude.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "&lon=" +
                    longitude.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);

                var feature = GetFirstNominatimFeature(url);

                return Json(new
                {
                    success = true,
                    address = feature == null
                        ? ""
                        : GetGeocodedLabel(feature),
                    resolvedCity = feature == null
                        ? ""
                        : GetGeocodedCity(feature),
                    latitude = latitude,
                    longitude = longitude,
                    inSelectedCity = true,
                    geocodeAvailable = feature != null
                }, JsonRequestBehavior.AllowGet);
            }
            catch (WebException)
            {
                return Json(new
                {
                    success = true,
                    address = "",
                    resolvedCity = "",
                    latitude = latitude,
                    longitude = longitude,
                    inSelectedCity = true,
                    geocodeAvailable = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = true,
                    address = "",
                    resolvedCity = "",
                    latitude = latitude,
                    longitude = longitude,
                    inSelectedCity = true,
                    geocodeAvailable = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool ValidateSubmittedEventLocation(
      string city,
      decimal? latitude,
      decimal? longitude)
        {
            city = NormalizeEventCity(city);

            // A supported service city must still be selected.
            if (!IsAllowedEventCity(city))
            {
                return false;
            }

            // The customer must still confirm a map location.
            if (!latitude.HasValue || !longitude.HasValue)
            {
                return false;
            }

            double lat = (double)latitude.Value;
            double lon = (double)longitude.Value;

            // Only check that the coordinates themselves are valid.
            if (lat < -90 || lat > 90 ||
                lon < -180 || lon > 180)
            {
                return false;
            }

            return IsWithinSelectedCityServiceArea(
                city,
                lat,
                lon);
        }

                [HttpPost]
        public JsonResult CreateBooking(CreateBookingRequest request)
        {
            if (Session["CustomerId"] == null)
            {
                return Json(new
                {
                    success = false,
                    requiresLogin = true,
                    message = "Please sign in before making a booking."
                });
            }

            if (request == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No booking information was received."
                });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "The booking information is invalid."
                });
            }

            try
            {
                var allowedCities = new[] { "Durban", "Pietermaritzburg", "Mandeni" };
                var requestedCity = (request.City ?? "").Trim();

                if (!allowedCities.Any(city =>
                    city.Equals(requestedCity, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select Durban, Pietermaritzburg or Mandeni."
                    });
                }

                request.City = allowedCities.First(city =>
                    city.Equals(requestedCity, StringComparison.OrdinalIgnoreCase));

                request.Address = (request.Address ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(request.Address))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter the event address."
                    });
                }

                // The map is optional. Manual address entry can be submitted
                // without coordinates. If coordinates are supplied, however,
                // they must be a complete pair and must pass the same city
                // service-area rule as the interactive map.
                bool hasLatitude = request.Latitude.HasValue;
                bool hasLongitude = request.Longitude.HasValue;

                if (hasLatitude != hasLongitude)
                {
                    return Json(new
                    {
                        success = false,
                        message = "The map location is incomplete. Please pin the location again or continue using the manually entered address."
                    });
                }

                if (hasLatitude &&
                    !ValidateSubmittedEventLocation(
                        request.City,
                        request.Latitude,
                        request.Longitude))
                {
                    return Json(new
                    {
                        success = false,
                        message = "The pinned location is outside the selected city service area. Move the pin inside the service area or continue with a manual address instead."
                    });
                }

                bool isCustomOccasion = string.Equals(
                    (request.Occasion ?? "").Trim(),
                    "Other",
                    StringComparison.OrdinalIgnoreCase);

                Package package;
                decimal packagePrice = 0m;

                if (isCustomOccasion)
                {
                    // "Other" intentionally has no preset package in the UI.
                    // A lightweight database placeholder preserves the existing
                    // Booking -> Package relationship while the booking awaits a custom quote.
                    package = db.Packages.FirstOrDefault(p => p.PackageId == "custom");

                    if (package == null)
                    {
                        package = new Package
                        {
                            PackageId = "custom",
                            Name = "Custom Setup",
                            Price = 0m
                        };

                        db.Packages.Add(package);
                        db.SaveChanges();
                    }

                    request.PackageId = "custom";
                }
                else
                {
                    if (!TryGetOccasionPackagePrice(request.Occasion, request.PackageId, out packagePrice))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "The selected package is not available for this occasion."
                        });
                    }

                    package = db.Packages
                        .FirstOrDefault(p => p.PackageId == request.PackageId);

                    if (package == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "The selected package is invalid."
                        });
                    }
                }

                var requestedAddOnIds = request.AddOns ?? new List<string>();

                var selectedAddOns = db.AddOns
                    .Where(a => requestedAddOnIds.Contains(a.AddOnId))
                    .ToList();

                if (selectedAddOns.Count != requestedAddOnIds.Count)
                {
                    return Json(new
                    {
                        success = false,
                        message = "One or more selected add-ons are invalid."
                    });
                }

                // Prototype occasion pricing is authoritative on the server.
                // Custom "Other" bookings are quoted after review, so no payment is collected yet.
                decimal totalPrice = isCustomOccasion
                    ? 0m
                    : packagePrice + selectedAddOns.Sum(a => a.Price);

                if (request.EventDate.Date < DateTime.Today)
                {
                    return Json(new
                    {
                        success = false,
                        message = "The event date cannot be in the past."
                    });
                }

                var currentTerms = GetCurrentTerms();
                if (currentTerms == null || !request.TermsAccepted)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please review and accept the Terms & Conditions and refund policy before confirming the booking."
                    });
                }

                int daysUntilEvent = (request.EventDate.Date - DateTime.Today).Days;
                decimal minimumPayment = isCustomOccasion
                    ? 0m
                    : (daysUntilEvent <= 1
                        ? totalPrice
                        : Math.Round(totalPrice * 0.50m, 2));

                if (isCustomOccasion)
                {
                    request.PaymentAmount = 0m;
                }
                else if (request.PaymentAmount < minimumPayment || request.PaymentAmount > totalPrice)
                {
                    return Json(new
                    {
                        success = false,
                        message = daysUntilEvent <= 1
                            ? "Bookings made for today or tomorrow require full payment upfront."
                            : "Please pay at least the 50% deposit, up to the full booking total.",
                        minimumPayment = minimumPayment,
                        totalPrice = totalPrice
                    });
                }

                decimal amountPaid = isCustomOccasion
                    ? 0m
                    : Math.Round(request.PaymentAmount, 2);

                decimal outstandingBalance = isCustomOccasion
                    ? 0m
                    : Math.Max(0m, totalPrice - amountPaid);

                string paymentStatus = isCustomOccasion
                    ? "Quote Required"
                    : (outstandingBalance == 0m
                        ? "Fully Paid"
                        : (amountPaid == minimumPayment ? "Deposit Paid" : "Partially Paid"));

                var customerId = (int)Session["CustomerId"];
                var customer = db.Customers.FirstOrDefault(x => x.Cust_ID == customerId);

                if (customer == null)
                {
                    return Json(new
                    {
                        success = false,
                        requiresLogin = true,
                        message = "Your customer account could not be found. Please sign in again."
                    });
                }

                var booking = new Booking
                {
                    CustomerId = customerId,
                    FirstName = customer.Cust_FName,
                    LastName = customer.Cust_LName,
                    Email = customer.Cust_Email,
                    Phone = customer.Cust_Phone,
                    Occasion = request.Occasion,
                    EventDate = request.EventDate,
                    EventTime = request.EventTime,
                    Address = request.Address,
                    City = request.City,
                    Notes = request.Notes,
                    PackageId = package.PackageId,
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.Now,
                    Status = "Pending",
                    AmountPaid = amountPaid,
                    BalanceDueDate = isCustomOccasion
                        ? (DateTime?)null
                        : (outstandingBalance > 0m
                            ? (DateTime?)request.EventDate.Date.AddDays(-1)
                            : null),
                    PaymentStatus = paymentStatus,
                    CancellationCharge = 0m,
                    RefundAmount = 0m,
                    TermsVersion = currentTerms.Version,
                    TermsAcceptedAt = DateTime.Now
                };

                foreach (var addOn in selectedAddOns)
                {
                    booking.BookingAddOns.Add(new BookingAddOn
                    {
                        AddOnId = addOn.AddOnId
                    });
                }

                db.Bookings.Add(booking);
                db.SaveChanges();

                var bookingEmailService = new OtpDeliveryService();
                bookingEmailService.SendBookingConfirmationEmail(
                    booking.Email,
                    booking.FirstName,
                    booking.BookingId,
                    booking.Occasion,
                    booking.EventDate,
                    booking.EventTime,
                    booking.City,
                    booking.TotalPrice
                );

                return Json(new
                {
                    success = true,
                    bookingId = booking.BookingId,
                    totalPrice = totalPrice,
                    amountPaid = booking.AmountPaid,
                    balanceOutstanding = booking.BalanceOutstanding,
                    paymentStatus = booking.PaymentStatus,
                    balanceDueDate = booking.BalanceDueDate,
                    customQuote = isCustomOccasion,
                    message = isCustomOccasion
                        ? "Your custom event request has been submitted for a quote."
                        : "Booking created successfully."
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while saving the booking."
                });
            }
        }

        private bool TryGetOccasionPackagePrice(string occasion, string packageId, out decimal price)
        {
            price = 0m;

            string occasionKey = (occasion ?? "").Trim();
            string packageKey = (packageId ?? "").Trim().ToLowerInvariant();

            if (occasionKey.Equals("Birthday", StringComparison.OrdinalIgnoreCase))
            {
                if (packageKey == "basic") { price = 650m; return true; }
                if (packageKey == "standard") { price = 850m; return true; }
                if (packageKey == "premium") { price = 1000m; return true; }
                return false;
            }

            if (occasionKey.Equals("Anniversary", StringComparison.OrdinalIgnoreCase))
            {
                if (packageKey == "basic") { price = 700m; return true; }
                if (packageKey == "standard") { price = 950m; return true; }
                if (packageKey == "premium") { price = 1500m; return true; }
                return false;
            }

            if (occasionKey.Equals("Graduation", StringComparison.OrdinalIgnoreCase))
            {
                if (packageKey == "basic") { price = 650m; return true; }
                if (packageKey == "standard") { price = 900m; return true; }
                if (packageKey == "premium") { price = 1200m; return true; }
                return false;
            }

            if (occasionKey.Equals("Valentine's Day", StringComparison.OrdinalIgnoreCase))
            {
                if (packageKey == "basic") { price = 750m; return true; }
                if (packageKey == "standard") { price = 1050m; return true; }
                return false;
            }

            if (occasionKey.Equals("Baby Shower", StringComparison.OrdinalIgnoreCase))
            {
                if (packageKey == "basic") { price = 800m; return true; }
                if (packageKey == "standard") { price = 1100m; return true; }
                if (packageKey == "premium") { price = 1600m; return true; }
                return false;
            }

            return false;
        }

        public ActionResult ViewBooking()
        {
            var bookings = new List<Booking>();

            if (Session["CustomerId"] != null)
            {
                int customerId = (int)Session["CustomerId"];

                bookings = db.Bookings
                    .Where(b => b.CustomerId == customerId)
                    .Include("Package")
                    .Include("BookingAddOns.AddOn")
                    .OrderByDescending(b => b.EventDate)
                    .ToList();

                var bookingIds = bookings.Select(b => b.BookingId).ToList();
                ViewBag.AssignedBookingIds = db.StaffTasks
                    .Where(t => t.BookingId.HasValue && bookingIds.Contains(t.BookingId.Value))
                    .Select(t => t.BookingId.Value)
                    .Distinct()
                    .ToList();
            }
            else
            {
                ViewBag.AssignedBookingIds = new List<int>();
            }

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CancelCustomerBooking(int bookingId, string cancellationReason)
        {
            if (Session["CustomerId"] == null)
                return Json(new { success = false, requiresLogin = true, message = "Please sign in first." });

            int customerId = (int)Session["CustomerId"];
            var booking = db.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.CustomerId == customerId);

            if (booking == null)
                return Json(new { success = false, message = "Booking could not be found." });

            string current = (booking.Status ?? "Pending").Trim();
            if (current.Equals("Setup Completed", StringComparison.OrdinalIgnoreCase) ||
                current.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                current.Equals("Declined", StringComparison.OrdinalIgnoreCase) ||
                current.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "This booking can no longer be cancelled online." });

            if (string.IsNullOrWhiteSpace(cancellationReason))
            {
                return Json(new
                {
                    success = false,
                    message = "Please provide a reason for cancelling this booking."
                });
            }

            cancellationReason = cancellationReason.Trim();
            if (cancellationReason.Length > 500)
            {
                cancellationReason = cancellationReason.Substring(0, 500);
            }

            int daysBeforeEvent = (booking.EventDate.Date - DateTime.Today).Days;
            decimal cancellationRate = daysBeforeEvent > 7 ? 0.10m : 0.20m;
            booking.CancellationCharge = Math.Round(booking.AmountPaid * cancellationRate, 2);
            booking.RefundAmount = Math.Max(0m, booking.AmountPaid - booking.CancellationCharge);
            booking.PaymentStatus = booking.RefundAmount > 0m ? "Refund Due" : booking.PaymentStatus;
            booking.BalanceDueDate = null;
            booking.Status = "Cancelled";

            var pendingTasks = db.StaffTasks
                .Where(t => t.BookingId == bookingId && t.Status == "Pending")
                .ToList();

            foreach (var task in pendingTasks)
            {
                task.Status = "Cancelled";
                task.CompletionReason = "Booking cancelled by customer.";
                task.CompletedAt = DateTime.Now;
            }

            db.SaveChanges();

            var cancellationEmailService = new OtpDeliveryService();
            bool cancellationEmailSent = cancellationEmailService.SendBookingCancellationEmail(
                booking.Email,
                booking.FirstName,
                booking.BookingId,
                booking.Occasion,
                booking.EventDate,
                cancellationReason,
                booking.AmountPaid,
                booking.CancellationCharge,
                booking.RefundAmount
            );

            return Json(new
            {
                success = true,
                status = booking.Status,
                cancellationCharge = booking.CancellationCharge,
                refundAmount = booking.RefundAmount,
                paymentStatus = booking.PaymentStatus,
                balanceOutstanding = 0m,
                cancellationReason = cancellationReason,
                cancellationEmailSent = cancellationEmailSent,
                message = booking.RefundAmount > 0m
                    ? "Booking cancelled. Your refund amount has been calculated from the amount paid." +
                      (cancellationEmailSent ? " A cancellation email has been sent to you." : "")
                    : "Booking cancelled." +
                      (cancellationEmailSent ? " A cancellation email has been sent to you." : "")
            });
        }

        [HttpGet]
        public ActionResult BalancePayment(int bookingId)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];
            var booking = db.Bookings
                .Include("Package")
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.CustomerId == customerId);

            if (booking == null)
            {
                TempData["PaymentError"] = "Booking could not be found.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            string status = (booking.Status ?? "Pending").Trim();
            if (status.Equals("Declined", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PaymentError"] = "Payments can no longer be added to this booking.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            decimal balance = Math.Max(0m, booking.TotalPrice - booking.AmountPaid);
            if (balance <= 0m)
            {
                booking.PaymentStatus = "Fully Paid";
                booking.BalanceDueDate = null;
                db.SaveChanges();

                TempData["PaymentSuccess"] = "This booking is already fully paid.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            if (string.Equals(booking.PaymentStatus, "Quote Required", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PaymentError"] = "This booking is waiting for a custom quote before payment can be made.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            ViewBag.Booking = booking;
            ViewBag.OutstandingBalance = balance;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BalancePayment(
            int bookingId,
            string cardholderName,
            string cardNumber,
            string expiryDate,
            string cvv,
            string streetAddress,
            string billingCity,
            string postalCode)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];
            var booking = db.Bookings
                .Include("Package")
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.CustomerId == customerId);

            if (booking == null)
            {
                TempData["PaymentError"] = "Booking could not be found.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            string status = (booking.Status ?? "Pending").Trim();
            if (status.Equals("Declined", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PaymentError"] = "Payments can no longer be added to this booking.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            decimal balance = Math.Max(0m, booking.TotalPrice - booking.AmountPaid);
            if (balance <= 0m)
            {
                booking.PaymentStatus = "Fully Paid";
                booking.BalanceDueDate = null;
                db.SaveChanges();

                TempData["PaymentSuccess"] = "This booking is already fully paid.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            if (string.Equals(booking.PaymentStatus, "Quote Required", StringComparison.OrdinalIgnoreCase))
            {
                TempData["PaymentError"] = "This booking is waiting for a custom quote before payment can be made.";
                return RedirectToAction("ViewBooking", "Cust");
            }

            string cardDigits = new string((cardNumber ?? "").Where(char.IsDigit).ToArray());
            string cvvDigits = new string((cvv ?? "").Where(char.IsDigit).ToArray());
            string postalDigits = new string((postalCode ?? "").Where(char.IsDigit).ToArray());

            bool detailsValid =
                !string.IsNullOrWhiteSpace(cardholderName) &&
                cardDigits.Length >= 13 &&
                cardDigits.Length <= 16 &&
                !string.IsNullOrWhiteSpace(expiryDate) &&
                (cvvDigits.Length == 3 || cvvDigits.Length == 4) &&
                !string.IsNullOrWhiteSpace(streetAddress) &&
                !string.IsNullOrWhiteSpace(billingCity) &&
                postalDigits.Length == 4;

            if (!detailsValid)
            {
                ViewBag.Booking = booking;
                ViewBag.OutstandingBalance = balance;
                ViewBag.PaymentError = "Please complete all banking details correctly before paying the remaining balance.";
                return View();
            }

            // This project uses a simulated card-payment screen.
            // Card/billing details are validated for the UI flow only and are never stored.
            booking.AmountPaid = booking.TotalPrice;
            booking.PaymentStatus = "Fully Paid";
            booking.BalanceDueDate = null;

            db.SaveChanges();

            TempData["PaymentSuccess"] =
                "Your remaining balance of R" + balance.ToString("N2") +
                " has been paid. This booking is now fully paid.";

            return RedirectToAction("ViewBooking", "Cust");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PayBookingBalance(int bookingId, decimal amount)
        {
            if (Session["CustomerId"] == null)
            {
                return Json(new { success = false, requiresLogin = true, message = "Please sign in first." });
            }

            int customerId = (int)Session["CustomerId"];
            var booking = db.Bookings.FirstOrDefault(b =>
                b.BookingId == bookingId &&
                b.CustomerId == customerId);

            if (booking == null)
            {
                return Json(new { success = false, message = "Booking could not be found." });
            }

            string status = (booking.Status ?? "Pending").Trim();
            if (status.Equals("Declined", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Payments can no longer be added to this booking." });
            }

            decimal balance = Math.Max(0m, booking.TotalPrice - booking.AmountPaid);
            if (balance <= 0m)
            {
                return Json(new
                {
                    success = true,
                    amountPaid = booking.AmountPaid,
                    balanceOutstanding = 0m,
                    paymentStatus = "Fully Paid",
                    message = "This booking is already fully paid."
                });
            }

            if (amount <= 0m || amount > balance)
            {
                return Json(new
                {
                    success = false,
                    message = "Enter an amount greater than zero and no more than the outstanding balance.",
                    balanceOutstanding = balance
                });
            }

            booking.AmountPaid = Math.Round(booking.AmountPaid + amount, 2);
            balance = Math.Max(0m, booking.TotalPrice - booking.AmountPaid);
            booking.PaymentStatus = balance == 0m ? "Fully Paid" : "Partially Paid";
            booking.BalanceDueDate = balance == 0m ? (DateTime?)null : booking.EventDate.Date.AddDays(-1);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                amountPaid = booking.AmountPaid,
                balanceOutstanding = balance,
                paymentStatus = booking.PaymentStatus,
                balanceDueDate = booking.BalanceDueDate,
                message = balance == 0m
                    ? "Your booking balance has been paid in full."
                    : "Payment recorded. A balance is still outstanding."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CompleteCustomerBooking(int bookingId)
        {
            if (Session["CustomerId"] == null)
            {
                return Json(new { success = false, requiresLogin = true, message = "Please sign in first." });
            }

            int customerId = (int)Session["CustomerId"];

            var booking = db.Bookings.FirstOrDefault(b =>
                b.BookingId == bookingId &&
                b.CustomerId == customerId);

            if (booking == null)
            {
                return Json(new { success = false, message = "Booking could not be found." });
            }

            if (!string.Equals(booking.Status, "Setup Completed", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new
                {
                    success = false,
                    message = "The booking can only be completed after staff have finished the venue setup."
                });
            }

            booking.Status = "Completed";
            db.SaveChanges();

            return Json(new { success = true, status = booking.Status, message = "Booking marked as completed." });
        }

        [HttpGet]
        public ActionResult ManageAccount()
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction("Login", "Cust");
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageAccount(Customer obj)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];
            var customer = db.Customers.FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Cust");
            }

            ModelState.Remove("Cust_Passw");

            string requestedEmail = (obj.Cust_Email ?? "").Trim();
            string requestedPhone = (obj.Cust_Phone ?? "").Trim();
            bool emailChanged = !string.Equals(customer.Cust_Email, requestedEmail, StringComparison.OrdinalIgnoreCase);
            bool phoneChanged = !string.Equals(customer.Cust_Phone ?? "", requestedPhone, StringComparison.Ordinal);

            if (emailChanged && db.Customers.Any(c =>
                c.Cust_Email == requestedEmail &&
                c.Cust_ID != customerId))
            {
                ModelState.AddModelError("Cust_Email", "An account with this email address already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            // Non-sensitive profile details can be saved immediately.
            customer.Cust_FName = obj.Cust_FName;
            customer.Cust_LName = obj.Cust_LName;
            db.SaveChanges();

            Session["CustomerFirstName"] = customer.Cust_FName;

            // Sensitive contact changes are staged until OTP verification succeeds.
            Session.Remove("PendingAccountEmail");
            Session.Remove("PendingAccountPhone");

            if (emailChanged)
            {
                Session["PendingAccountEmail"] = requestedEmail;

                if (phoneChanged)
                {
                    Session["PendingAccountPhone"] = requestedPhone;
                }

                if (!CreateAndSendOtp(customer, "ChangeEmail", requestedEmail))
                {
                    Session.Remove("PendingAccountEmail");
                    Session.Remove("PendingAccountPhone");
                    string deliveryError = TempData["OtpDeliveryError"] as string;
                    TempData["OtpError"] =
                        "We could not send a verification code to the new email address. Your email and phone number were not changed. " +
                        (deliveryError ?? "Please check the email service configuration.");
                    return RedirectToAction("ManageAccount", "Cust");
                }

                TempData["OtpNotice"] = "We sent a verification code to your new email address. Verify it before the email change is saved.";
                return RedirectToAction("VerifyOtp", "Cust");
            }

            if (phoneChanged)
            {
                Session["PendingAccountPhone"] = requestedPhone;

                if (!CreateAndSendOtp(customer, "ChangePhone", customer.Cust_Email))
                {
                    Session.Remove("PendingAccountPhone");
                    string deliveryError = TempData["OtpDeliveryError"] as string;
                    TempData["OtpError"] =
                        "We could not send a verification code to your verified email address. Your phone number was not changed. " +
                        (deliveryError ?? "Please check the email service configuration.");
                    return RedirectToAction("ManageAccount", "Cust");
                }

                TempData["OtpNotice"] = "We sent a verification code to your verified email address. Verify it before the phone number change is saved.";
                return RedirectToAction("VerifyOtp", "Cust");
            }

            TempData["AccountSuccess"] = "Your account details have been updated successfully.";
            return RedirectToAction("ManageAccount");
        }

        private bool CreateAndSendOtp(Customer customer, string purpose, string recipientEmail)
        {
            string otp = OtpHelper.GenerateOtp();

            var previousOtps = db.OtpVerifications
                .Where(o => o.CustomerId == customer.Cust_ID &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .ToList();

            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;
            }

            var verification = new OtpVerification
            {
                CustomerId = customer.Cust_ID,
                OtpHash = OtpHelper.HashOtp(otp),
                DeliveryMethod = "Email",
                Purpose = purpose,
                CreatedAt = DateTime.Now,
                ExpiresAt = OtpHelper.GetExpiryTime(),
                IsUsed = false,
                FailedAttempts = 0
            };

            db.OtpVerifications.Add(verification);
            db.SaveChanges();
            Session["ExpectedOtpPurpose"] = purpose;

            var deliveryService = new OtpDeliveryService();
            bool sent = deliveryService.SendOtpByEmail(recipientEmail, otp);

            if (!sent)
            {
                verification.IsUsed = true;
                db.SaveChanges();
                Session.Remove("ExpectedOtpPurpose");
                TempData["OtpDeliveryError"] = deliveryService.LastError;
                return false;
            }

            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Cust");
        }

        [HttpGet]
        public JsonResult EmailDiagnostics()
        {
            if (Session["CustomerId"] == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Please sign in first."
                }, JsonRequestBehavior.AllowGet);
            }

            var service = new OtpDeliveryService();

            return Json(new
            {
                success = true,
                configured = service.IsConfigured,
                sender = service.SenderDisplay,
                smtpHost = service.SmtpHost,
                smtpPort = service.SmtpPort,
                passwordConfigured = service.HasPassword
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult EmailSmtpTest()
        {
            if (Session["CustomerId"] == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Please sign in first."
                }, JsonRequestBehavior.AllowGet);
            }

            int customerId = (int)Session["CustomerId"];
            var customer = db.Customers.FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Customer account could not be found."
                }, JsonRequestBehavior.AllowGet);
            }

            var service = new OtpDeliveryService();
            bool sent = service.SendDiagnosticEmail(customer.Cust_Email);

            return Json(new
            {
                success = sent,
                message = sent
                    ? "SMTP test email sent successfully."
                    : service.LastError,
                diagnostic = sent ? null : service.LastDiagnostic,
                smtpHost = service.SmtpHost,
                smtpPort = service.SmtpPort,
                sender = service.SenderDisplay
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TestCustomerDatabase()
        {
            var customerCount = db.Customers.Count();

            var databaseName = db.Database.SqlQuery<string>(
                "SELECT DB_NAME()"
            ).FirstOrDefault();

            var serverName = db.Database.SqlQuery<string>(
                "SELECT @@SERVERNAME"
            ).FirstOrDefault();

            return Json(new
            {
                success = true,
                customerCount = customerCount,
                database = databaseName,
                server = serverName
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestOtp(string deliveryMethod, string purpose)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];

            var customer = db.Customers.FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction("Login", "Cust");
            }

            if (deliveryMethod != "Email")
            {
                TempData["OtpError"] = "Password verification is currently available by email.";
                return RedirectToAction("ManageAccount", "Cust");
            }

            if (purpose != "ManageAccount" && purpose != "ChangePassword")
            {
                TempData["OtpError"] = "Invalid OTP request.";
                return RedirectToAction("ManageAccount", "Cust");
            }

            string otp = OtpHelper.GenerateOtp();

            var otpVerification = new OtpVerification
            {
                CustomerId = customer.Cust_ID,
                OtpHash = OtpHelper.HashOtp(otp),
                DeliveryMethod = deliveryMethod,
                Purpose = purpose,
                CreatedAt = DateTime.Now,
                ExpiresAt = OtpHelper.GetExpiryTime(),
                IsUsed = false,
                FailedAttempts = 0
            };

            // Only the newest unused code for this purpose should remain valid.
            var previousOtps = db.OtpVerifications
                .Where(o => o.CustomerId == customer.Cust_ID &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .ToList();

            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;
            }

            db.OtpVerifications.Add(otpVerification);
            db.SaveChanges();
            Session["ExpectedOtpPurpose"] = purpose;

            var deliveryService = new OtpDeliveryService();

            bool sent = deliveryService.SendOtpByEmail(
                customer.Cust_Email,
                otp
            );

            if (!sent)
            {
                // A code that was never delivered must never remain usable.
                otpVerification.IsUsed = true;
                db.SaveChanges();
                Session.Remove("ExpectedOtpPurpose");

                TempData["OtpError"] =
                    "We could not send the verification code. " +
                    (deliveryService.LastError ?? "Please check the email service configuration.");

                return RedirectToAction("ManageAccount", "Cust");
            }

            return RedirectToAction("VerifyOtp", "Cust");
        }

        [HttpGet]
        public ActionResult VerifyOtp()
        {
            bool loggedInOtp =
                Session["CustomerId"] != null;

            bool forgotPasswordOtp =
                Session["OtpCustomerId"] != null;

            if (!loggedInOtp && !forgotPasswordOtp)
            {
                return RedirectToAction("Login", "Cust");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOtp(string otp)
        {
            int customerId;

            // Forgot Password OTP takes priority
            if (Session["OtpCustomerId"] != null)
            {
                customerId = (int)Session["OtpCustomerId"];
            }
            else if (Session["CustomerId"] != null)
            {
                customerId = (int)Session["CustomerId"];
            }
            else
            {
                return RedirectToAction("Login", "Cust");
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                ModelState.AddModelError(
                    "",
                    "Please enter the verification code."
                );

                return View();
            }

            string expectedPurpose = Session["ExpectedOtpPurpose"] as string;

            if (string.IsNullOrWhiteSpace(expectedPurpose))
            {
                ModelState.AddModelError("", "The verification session has expired. Please request a new code.");
                return View();
            }

            var verification = db.OtpVerifications
                .Where(o =>
                    o.CustomerId == customerId &&
                    o.Purpose == expectedPurpose &&
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.Now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (verification == null)
            {
                ModelState.AddModelError(
                    "",
                    "Your verification code has expired or is invalid."
                );

                return View();
            }

            if (verification.FailedAttempts >= 5)
            {
                verification.IsUsed = true;
                db.SaveChanges();
                Session.Remove("ExpectedOtpPurpose");

                ModelState.AddModelError(
                    "",
                    "Too many incorrect attempts. Please request a new code."
                );

                return View();
            }

            bool valid = OtpHelper.VerifyOtp(
                otp,
                verification.OtpHash
            );

            if (!valid)
            {
                verification.FailedAttempts++;

                if (verification.FailedAttempts >= 5)
                {
                    verification.IsUsed = true;
                    Session.Remove("ExpectedOtpPurpose");
                }

                db.SaveChanges();

                ModelState.AddModelError(
                    "",
                    verification.FailedAttempts >= 5
                        ? "Too many incorrect attempts. Please request a new code."
                        : "Incorrect verification code."
                );

                return View();
            }

            verification.IsUsed = true;

            db.SaveChanges();

            Session["OtpVerified"] = true;
            Session["OtpPurpose"] = verification.Purpose;
            Session.Remove("ExpectedOtpPurpose");

            // ACCOUNT EMAIL CHANGE
            if (verification.Purpose == "ChangeEmail")
            {
                string pendingEmail = Session["PendingAccountEmail"] as string;
                var accountCustomer = db.Customers.FirstOrDefault(x => x.Cust_ID == customerId);

                if (accountCustomer == null || string.IsNullOrWhiteSpace(pendingEmail))
                {
                    Session.Remove("OtpVerified");
                    Session.Remove("OtpPurpose");
                    Session.Remove("PendingAccountEmail");
                    Session.Remove("PendingAccountPhone");
                    TempData["OtpError"] = "The pending email change could not be completed. Please try again.";
                    return RedirectToAction("ManageAccount", "Cust");
                }

                if (db.Customers.Any(x => x.Cust_ID != customerId && x.Cust_Email == pendingEmail))
                {
                    Session.Remove("OtpVerified");
                    Session.Remove("OtpPurpose");
                    Session.Remove("PendingAccountEmail");
                    Session.Remove("PendingAccountPhone");
                    TempData["OtpError"] = "That email address is already in use.";
                    return RedirectToAction("ManageAccount", "Cust");
                }

                accountCustomer.Cust_Email = pendingEmail.Trim();
                db.SaveChanges();

                Session["CustomerEmail"] = accountCustomer.Cust_Email;
                Session.Remove("PendingAccountEmail");
                Session.Remove("OtpVerified");
                Session.Remove("OtpPurpose");

                string pendingPhoneAfterEmail = Session["PendingAccountPhone"] as string;
                if (!string.IsNullOrWhiteSpace(pendingPhoneAfterEmail) &&
                    !string.Equals(accountCustomer.Cust_Phone ?? "", pendingPhoneAfterEmail, StringComparison.Ordinal))
                {
                    if (CreateAndSendOtp(accountCustomer, "ChangePhone", accountCustomer.Cust_Email))
                    {
                        TempData["OtpNotice"] = "Your new email address is verified. We sent another code there to verify the pending phone number change.";
                        return RedirectToAction("VerifyOtp", "Cust");
                    }

                    Session.Remove("PendingAccountPhone");
                    TempData["AccountSuccess"] = "Your email address was updated successfully, but the phone verification email could not be sent.";
                    return RedirectToAction("ManageAccount", "Cust");
                }

                TempData["AccountSuccess"] = "Your new email address has been verified and saved.";
                return RedirectToAction("ManageAccount", "Cust");
            }

            // ACCOUNT PHONE CHANGE
            if (verification.Purpose == "ChangePhone")
            {
                string pendingPhone = Session["PendingAccountPhone"] as string;
                var accountCustomer = db.Customers.FirstOrDefault(x => x.Cust_ID == customerId);

                if (accountCustomer == null || string.IsNullOrWhiteSpace(pendingPhone))
                {
                    Session.Remove("OtpVerified");
                    Session.Remove("OtpPurpose");
                    Session.Remove("PendingAccountPhone");
                    TempData["OtpError"] = "The pending phone number change could not be completed. Please try again.";
                    return RedirectToAction("ManageAccount", "Cust");
                }

                accountCustomer.Cust_Phone = pendingPhone.Trim();
                db.SaveChanges();

                Session.Remove("PendingAccountPhone");
                Session.Remove("OtpVerified");
                Session.Remove("OtpPurpose");

                TempData["AccountSuccess"] = "Your phone number has been verified and saved.";
                return RedirectToAction("ManageAccount", "Cust");
            }

            // FORGOT PASSWORD
            if (verification.Purpose == "ForgotPassword")
            {
                return RedirectToAction(
                    "ChangePassword",
                    "Cust"
                );
            }

            // LOGGED-IN CUSTOMER PASSWORD CHANGE
            if (verification.Purpose == "ChangePassword")
            {
                return RedirectToAction(
                    "ChangePassword",
                    "Cust"
                );
            }

            // Other OTP purposes
            return RedirectToAction(
                "ManageAccount",
                "Cust"
            );
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            bool loggedInCustomer =
                Session["CustomerId"] != null;

            bool forgotPasswordCustomer =
                Session["OtpCustomerId"] != null;

            if (!loggedInCustomer && !forgotPasswordCustomer)
            {
                return RedirectToAction("Login", "Cust");
            }

            if (Session["OtpVerified"] == null ||
                Session["OtpVerified"].ToString() != "True" ||
                Session["OtpPurpose"] == null)
            {
                return RedirectToAction(
                    forgotPasswordCustomer
                        ? "ForgotPassword"
                        : "ManageAccount",
                    "Cust"
                );
            }

            string purpose = Session["OtpPurpose"].ToString();

            if (purpose != "ChangePassword" &&
                purpose != "ForgotPassword")
            {
                return RedirectToAction(
                    "ManageAccount",
                    "Cust"
                );
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            bool loggedInCustomer =
                Session["CustomerId"] != null;

            bool forgotPasswordCustomer =
                Session["OtpCustomerId"] != null;

            if (!loggedInCustomer && !forgotPasswordCustomer)
            {
                return RedirectToAction("Login", "Cust");
            }

            if (Session["OtpVerified"] == null ||
                Session["OtpVerified"].ToString() != "True" ||
                Session["OtpPurpose"] == null)
            {
                return RedirectToAction(
                    forgotPasswordCustomer
                        ? "ForgotPassword"
                        : "ManageAccount",
                    "Cust"
                );
            }

            string purpose = Session["OtpPurpose"].ToString();

            if (purpose != "ChangePassword" &&
                purpose != "ForgotPassword")
            {
                return RedirectToAction(
                    "ManageAccount",
                    "Cust"
                );
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Please enter a new password."
                );

                return View(model);
            }

            if (model.NewPassword.Length < 6 ||
                model.NewPassword.Length > 15 ||
                !model.NewPassword.Any(char.IsLetter) ||
                !model.NewPassword.Any(char.IsDigit))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Password must be 6 to 15 characters long and contain at least one letter and one number."
                );

                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(
                    "ConfirmPassword",
                    "The passwords do not match."
                );

                return View(model);
            }

            int customerId;

            if (forgotPasswordCustomer)
            {
                customerId = (int)Session["OtpCustomerId"];
            }
            else
            {
                customerId = (int)Session["CustomerId"];
            }

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction(
                    "Login",
                    "Cust"
                );
            }

            // Hash the new password before storing it.
            customer.Cust_Passw =
                Crypto.HashPassword(model.NewPassword);

            db.Entry(customer).State =
                EntityState.Modified;

            db.SaveChanges();

            // OTP authorization has now been consumed.
            Session.Remove("OtpVerified");
            Session.Remove("OtpPurpose");
            Session.Remove("OtpCustomerId");

            TempData["AccountSuccess"] =
                "Your password has been changed successfully.";

            // If they were logged in, return to Manage Account.
            if (loggedInCustomer)
            {
                return RedirectToAction(
                    "ManageAccount",
                    "Cust"
                );
            }

            // Forgot-password users are not logged in.
            return RedirectToAction(
                "Login",
                "Cust"
            );
        }



        [HttpGet]
        public ActionResult AdminDashboard()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            DateTime today = DateTime.Today;

            // ---------------------------------------------------------
            // TOTAL BOOKINGS
            // ---------------------------------------------------------
            int totalBookings = db.Bookings.Count();

            // ---------------------------------------------------------
            // PENDING BOOKINGS
            // ---------------------------------------------------------
            int pendingBookings = db.Bookings.Count(b =>
                b.Status != null &&
                b.Status.ToLower() == "pending");

            // ---------------------------------------------------------
            // UPCOMING APPROVED EVENTS
            // ---------------------------------------------------------
            int upcomingEvents = db.Bookings.Count(b =>
                b.EventDate >= today &&
                b.Status != null &&
                b.Status.ToLower() == "approved");

            // ---------------------------------------------------------
            // DECLINED BOOKINGS
            // ---------------------------------------------------------
            int cancelledBookings = db.Bookings.Count(b =>
                b.Status != null &&
                b.Status.ToLower() == "declined");

            // ---------------------------------------------------------
            // REGISTERED CUSTOMERS
            // ---------------------------------------------------------
            int registeredCustomers = db.Customers.Count();

            // ---------------------------------------------------------
            // APPROVED BOOKING REVENUE
            // ---------------------------------------------------------
            decimal confirmedRevenue =
                db.Bookings
                    .Where(b =>
                        b.Status != null &&
                        b.Status.ToLower() != "declined" &&
                        b.Status.ToLower() != "cancelled")
                    .Select(b => (decimal?)b.AmountPaid)
                    .Sum() ?? 0m;

            // ---------------------------------------------------------
            // STAFF AVAILABILITY
            // A staff member is busy while they have at least one pending task.
            // ---------------------------------------------------------
            int totalStaff = db.Staffs.Count();
            int busyStaff = db.Staffs.Count(s =>
                db.StaffTasks.Any(t => t.StaffId == s.staff_ID && t.Status == "Pending"));
            int availableStaff = totalStaff - busyStaff;

            // ---------------------------------------------------------
            // RECENT BOOKINGS
            // ---------------------------------------------------------
            var recentBookings = db.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToList();

            // ---------------------------------------------------------
            // RECENT ACTIVITIES
            // ---------------------------------------------------------
            var recentActivities = db.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new DashboardActivity
                {
                    Description =
                        b.FirstName + " " +
                        b.LastName +
                        " submitted a booking for " +
                        b.Occasion +
                        " in " +
                        b.City,

                    ActivityDate = b.CreatedAt
                })
                .ToList();

            // ---------------------------------------------------------
            // EVENT LOAD
            // ---------------------------------------------------------
            DateTime startOfWeek =
                today.AddDays(-(int)today.DayOfWeek);

            DateTime endOfWeek =
                startOfWeek.AddDays(7);

            var weeklyBookings = db.Bookings
                .Where(b =>
                    b.EventDate >= startOfWeek &&
                    b.EventDate < endOfWeek)
                .ToList();

            int weddingLoad = weeklyBookings.Count(b =>
                b.Occasion != null &&
                b.Occasion.ToLower().Contains("wedding"));

            int corporateLoad = weeklyBookings.Count(b =>
                b.Occasion != null &&
                b.Occasion.ToLower().Contains("corporate"));

            int birthdayLoad = weeklyBookings.Count(b =>
                b.Occasion != null &&
                b.Occasion.ToLower().Contains("birthday"));

            // ---------------------------------------------------------
            // VIEW MODEL
            // ---------------------------------------------------------
            var model = new AdminDashboardViewModel
            {
                TotalBookings = totalBookings,

                PendingBookings = pendingBookings,

                UpcomingEvents = upcomingEvents,

                CancelledBookings = cancelledBookings,

                RegisteredCustomers = registeredCustomers,

                ConfirmedRevenue = confirmedRevenue,

                TotalStaff = totalStaff,
                AvailableStaff = availableStaff,
                BusyStaff = busyStaff,

                RecentBookings = recentBookings,

                RecentActivities = recentActivities,

                WeddingLoad = weddingLoad,

                CorporateLoad = corporateLoad,

                BirthdayLoad = birthdayLoad
            };

            // ---------------------------------------------------------
            // ADMIN INFORMATION
            // ---------------------------------------------------------
            ViewBag.AdminFirstName = Session["AdminFirstName"];
            ViewBag.AdminEmail = Session["AdminEmail"];

            return View(model);
        }

       



        // ==========================================
        // UPDATE BOOKING STATUS
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateBookingStatus(int bookingId, string status)
        {
            // Check admin authentication
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return Json(new
                {
                    success = false,
                    message = "You are not authorized to perform this action."
                });
            }


            // Validate status
            if (string.IsNullOrWhiteSpace(status))
            {
                return Json(new
                {
                    success = false,
                    message = "A booking status is required."
                });
            }


            status = status.Trim().ToLower();


            // Only allow these statuses
            if (status != "pending" &&
                status != "approved" &&
                status != "declined")
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid booking status."
                });
            }


            // Find booking
            var booking =
                db.Bookings.FirstOrDefault(b =>
                    b.BookingId == bookingId);


            if (booking == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Booking could not be found."
                });
            }

            string currentStatus = (booking.Status ?? "Pending").Trim();
            if (currentStatus.Equals("Declined", StringComparison.OrdinalIgnoreCase) ||
                currentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                currentStatus.Equals("Setup Completed", StringComparison.OrdinalIgnoreCase) ||
                currentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new
                {
                    success = false,
                    message = "This booking has reached a final workflow stage and its approval status can no longer be changed."
                });
            }


            // Convert to display format
            string newStatus;

            if (status == "approved")
            {
                newStatus = "Approved";
            }
            else if (status == "declined")
            {
                newStatus = "Declined";
            }
            else
            {
                newStatus = "Pending";
            }


            // Update database.
            //
            // A staff task is created only when the booking becomes Approved.
            // This prevents staff from receiving work for bookings that are
            // still awaiting admin review.
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    booking.Status = newStatus;

                    if (newStatus == "Approved")
                    {
                        bool taskAlreadyExists = db.StaffTasks
                            .Any(t => t.BookingId == booking.BookingId);

                        if (!taskAlreadyExists)
                        {
                            // Choose the least-loaded staff member assigned
                            // to the booking's city.
                            string bookingCity = NormalizeCity(booking.City);

                            var assignedStaff = db.Staffs
                                .Where(s => s.staff_City != null)
                                .ToList()
                                .Where(s => NormalizeCity(s.staff_City) == bookingCity)
                                .Select(s => new
                                {
                                    Staff = s,
                                    ActiveTaskCount = db.StaffTasks.Count(t =>
                                        t.StaffId == s.staff_ID &&
                                        t.Status == "Pending")
                                })
                                .OrderBy(x => x.ActiveTaskCount)
                                .ThenBy(x => x.Staff.staff_ID)
                                .Select(x => x.Staff)
                                .FirstOrDefault();

                            if (assignedStaff == null)
                            {
                                transaction.Rollback();

                                return Json(new
                                {
                                    success = false,
                                    message = "The booking cannot be approved because no staff member is registered for " +
                                              booking.City + ". Register a staff member for this city first."
                                });
                            }

                            db.StaffTasks.Add(new StaffTask
                            {
                                StaffId = assignedStaff.staff_ID,
                                BookingId = booking.BookingId,
                                TaskName = "Decorate event for " +
                                           booking.FirstName + " " +
                                           booking.LastName,
                                Description = "Complete the event decoration setup at " +
                                              booking.Address + ", " +
                                              booking.City + " for the " +
                                              booking.Occasion + ".",
                                DueDate = booking.EventDate,
                                Priority = "High",
                                Status = "Pending",
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                    else if (newStatus == "Declined")
                    {
                        // A business-side cancellation/decline receives a full refund
                        // of the amount actually paid.
                        booking.CancellationCharge = 0m;
                        booking.RefundAmount = booking.AmountPaid;
                        if (booking.AmountPaid > 0m)
                        {
                            booking.PaymentStatus = "Refund Due";
                        }

                        // Do not leave an active staff task behind if a booking
                        // is declined.
                        var existingTasks = db.StaffTasks
                            .Where(t => t.BookingId == booking.BookingId &&
                                        t.Status == "Pending")
                            .ToList();

                        foreach (var task in existingTasks)
                        {
                            task.Status = "Unable";
                            task.CompletionReason = "Booking was declined by the administrator.";
                            task.CompletedAt = DateTime.Now;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Trace.TraceError(
                        "UpdateBookingStatus failed for booking {0}: {1}",
                        bookingId,
                        ex);

                    return Json(new
                    {
                        success = false,
                        message = "We could not update this booking right now. Please try again."
                    });
                }
            }

            if (!Request.IsAjaxRequest())
            {
                TempData["AdminSuccess"] = newStatus == "Approved"
                    ? "Booking approved and staff task assigned successfully."
                    : "Booking status updated successfully.";

                return RedirectToAction("AllBookings", "Cust");
            }

            return Json(new
            {
                success = true,
                bookingId = booking.BookingId,
                status = newStatus,
                message = newStatus == "Approved"
                    ? "Booking approved and staff task assigned successfully."
                    : "Booking status updated successfully."
            });

        }

        // ==========================================
        // ALL BOOKINGS
        // ==========================================

        [HttpGet]
        public ActionResult AllBookings()
        {
            // ==========================================
            // CHECK ADMIN AUTHENTICATION
            // ==========================================

            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }


            // ==========================================
            // CURRENT DATE
            // ==========================================

            DateTime today = DateTime.Today;


            // ==========================================
            // GET ALL BOOKINGS FROM DATABASE
            // ==========================================

            var allBookings = db.Bookings
                .OrderByDescending(b => b.EventDate)
                .ToList();


            // ==========================================
            // PAST BOOKINGS
            // ==========================================

            var pastBookings = allBookings
                .Where(b => b.EventDate < today)
                .OrderByDescending(b => b.EventDate)
                .ToList();


            // ==========================================
            // UPCOMING BOOKINGS
            // ==========================================

            var upcomingBookings = allBookings
                .Where(b =>
                    b.EventDate >= today &&
                    b.Status != null &&
                    b.Status.ToLower() == "approved")
                .OrderBy(b => b.EventDate)
                .ToList();


            // ==========================================
            // CANCELLED BOOKINGS
            // ==========================================

            var cancelledBookings = allBookings
                .Where(b =>
                    b.Status != null &&
                    b.Status.ToLower() == "declined")
                .OrderByDescending(b => b.CreatedAt)
                .ToList();


            // ==========================================
            // BUILD VIEW MODEL
            // ==========================================

            var model = new AllBookingsViewModel
            {
                AllBookings = allBookings,

                PastBookings = pastBookings,

                UpcomingBookings = upcomingBookings,

                CancelledBookings = cancelledBookings,

                TotalBookings = allBookings.Count,

                PastBookingCount = pastBookings.Count,

                UpcomingBookingCount = upcomingBookings.Count,

                CancelledBookingCount = cancelledBookings.Count
            };


            // ==========================================
            // ADMIN INFORMATION
            // ==========================================

            ViewBag.AdminFirstName =
                Session["AdminFirstName"];

            ViewBag.AdminEmail =
                Session["AdminEmail"];


            return View(model);
        }


        // ==========================================
        // UPCOMING EVENTS
        // ==========================================

        [HttpGet]
        public ActionResult UpcomingEvents()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            DateTime today = DateTime.Today;

            var bookings = db.Bookings
                .Where(b =>
                    b.EventDate >= today &&
                    b.Status != null &&
                    b.Status.ToLower() == "approved")
                .OrderBy(b => b.EventDate)
                .ToList();

            return View(bookings);
        }


        // ==========================================
        // CANCELLED BOOKINGS
        // ==========================================

        [HttpGet]
        public ActionResult CancelledBookings()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            return View();
        }


        // ==========================================
        // PENDING APPROVALS
        // ==========================================

        [HttpGet]
        public ActionResult PendingApprovals()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            // Use the same persisted booking status that drives the dashboard count.
            // Older/null statuses are treated as pending for compatibility.
            var pendingBookings = db.Bookings
                .Where(b => string.IsNullOrEmpty(b.Status) || b.Status == "Pending")
                .OrderBy(b => b.EventDate)
                .ThenBy(b => b.CreatedAt)
                .ToList();

            return View(pendingBookings);
        }


        // ==========================================
        // BUSINESS ANALYTICS
        // ==========================================

        [HttpGet]
        public ActionResult BusinessAnalytics()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            return View();
        }

        // ===============================
        // ADMIN LOGIN - GET
        // ===============================

        [HttpGet]
        public ActionResult AdminLogin()
        {
            return View();
        }


        // ===============================
        // ADMIN LOGIN - POST
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminLogin(
     string email,
     string password,
     string adminAccessCode)
        {
            // ==========================================
            // CHECK REQUIRED FIELDS
            // ==========================================

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(adminAccessCode))
            {
                ModelState.AddModelError(
                    "",
                    "Please enter your email address, password and admin authorization code."
                );

                return View();
            }

            email = email.Trim();
            adminAccessCode = adminAccessCode.Trim();

            // ==========================================
            // CHECK ADMIN AUTHORIZATION CODE
            // ==========================================

            if (!IsValidAdminAccessCode(adminAccessCode))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid admin authorization code."
                );

                return View();
            }

            // ==========================================
            // FIND ADMIN
            // ==========================================

            var admin = db.Admins
                .FirstOrDefault(a => a.admin_Email == email);

            if (admin == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email address or password."
                );

                return View();
            }

            // ==========================================
            // VERIFY PASSWORD
            // ==========================================

            bool passwordValid = false;

            try
            {
                passwordValid =
                    Crypto.VerifyHashedPassword(
                        admin.admin_Passw,
                        password
                    );
            }
            catch
            {
                passwordValid = false;
            }

            if (!passwordValid)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email address or password."
                );

                return View();
            }

            // ==========================================
            // ADMIN AUTHENTICATED
            // ==========================================

            Session["AdminId"] =
                admin.admin_ID;

            Session["AdminEmail"] =
                admin.admin_Email;

            Session["AdminFirstName"] =
                admin.admin_FName;

            Session["AdminAuthenticated"] =
                true;

            // ==========================================
            // SEND TO ADMIN DASHBOARD
            // ==========================================

            return RedirectToAction(
                "AdminDashboard",
                "Cust"
            );
        }

        [HttpGet]
        public ActionResult AdminLogout()
        {
            // Clear admin session information

            Session.Remove("AdminId");
            Session.Remove("AdminEmail");
            Session.Remove("AdminFirstName");
            Session.Remove("AdminAuthenticated");

            return RedirectToAction(
                "Login",
                "Cust"
            );
        }

        [HttpGet]
        public JsonResult AdminExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Email required" }, JsonRequestBehavior.AllowGet);

            bool exists = db.Admins.Any(a => a.admin_Email == email.Trim());
            return Json(new { success = true, exists }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Ping()
        {
            // Quick routing/controller reachability test
            return Content("CustController: Pong");
        }

        [HttpGet]
        public ActionResult Adminregister()
        {
            return View();
        }

        public ActionResult StaffDashboard()
        {
            if (!IsStaffAuthenticated())
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            DateTime weekEnd = today.AddDays(7);

            var staff = db.Staffs.FirstOrDefault(s => s.staff_ID == staffId);
            if (staff == null)
            {
                ClearRoleSessions();
                return RedirectToAction("StaffLogin", "Cust");
            }

            var allTasks = db.StaffTasks
                .Where(t => t.StaffId == staffId)
                .Include("Booking")
                .OrderBy(t => t.DueDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToList();

            var todayTasks = allTasks
                .Where(t => t.Status == "Pending" && t.DueDate >= today && t.DueDate < tomorrow)
                .ToList();

            var upcomingEvents = allTasks
                .Where(t => t.Booking != null &&
                            t.Booking.EventDate >= today &&
                            t.Booking.EventDate < weekEnd &&
                            t.Booking.Status == "Approved")
                .GroupBy(t => t.BookingId)
                .Select(g => g.First().Booking)
                .OrderBy(b => b.EventDate)
                .Select(b => new StaffEventDashboardItem
                {
                    BookingId = b.BookingId,
                    Occasion = b.Occasion,
                    EventDate = b.EventDate,
                    EventTime = b.EventTime,
                    Address = b.Address,
                    City = b.City,
                    Status = b.Status
                })
                .ToList();

            var model = new StaffDashboardViewModel
            {
                StaffMember = staff,
                TeamCity = staff.staff_City,
                TodayTasks = todayTasks.Count,
                HighPriorityTasks = allTasks.Count(t => t.Status == "Pending" && t.Priority == "High"),
                EventsThisWeek = upcomingEvents.Count,
                OpenComplaints = db.StaffComplaints.Count(x => x.StaffId == staffId && x.Status == "Open"),
                HoursLogged = null,
                TasksDueToday = todayTasks,
                UpcomingEvents = upcomingEvents
            };

            return View(model);
        }

        public ActionResult StaffTasks()
        {
            if (!IsStaffAuthenticated())
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            var tasks = db.StaffTasks
                .Where(t => t.StaffId == staffId)
                .Include("Booking")
                .OrderBy(t => t.Status == "Pending" ? 0 : 1)
                .ThenBy(t => t.DueDate)
                .ToList();

            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStaffTask(int taskId, string status, string completionReason)
        {
            if (!IsStaffAuthenticated())
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            var task = db.StaffTasks
                .FirstOrDefault(t => t.TaskId == taskId && t.StaffId == staffId);

            if (task == null)
            {
                TempData["StaffTaskError"] = "The task could not be found.";
                return RedirectToAction("StaffTasks", "Cust");
            }

            if (!string.Equals(task.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                TempData["StaffTaskError"] = "This task has already reached a final status and cannot be changed.";
                return RedirectToAction("StaffTasks", "Cust");
            }

            if (status == "Completed")
            {
                task.Status = "Completed";
                task.CompletionReason = null;
                task.CompletedAt = DateTime.Now;

                // Completing the decoration task means the venue setup is ready.
                // Keep the booking itself open until the customer confirms arrival/completion.
                if (task.BookingId.HasValue)
                {
                    var relatedBooking = db.Bookings
                        .FirstOrDefault(b => b.BookingId == task.BookingId.Value);

                    if (relatedBooking != null &&
                        !string.Equals(relatedBooking.Status, "Declined", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(relatedBooking.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(relatedBooking.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        relatedBooking.Status = "Setup Completed";
                    }
                }
            }
            else if (status == "Unable")
            {
                if (string.IsNullOrWhiteSpace(completionReason))
                {
                    TempData["StaffTaskError"] = "Please provide a reason when a task cannot be completed.";
                    return RedirectToAction("StaffTasks", "Cust");
                }

                task.Status = "Unable";
                task.CompletionReason = completionReason.Trim();
                task.CompletedAt = DateTime.Now;
            }
            else
            {
                TempData["StaffTaskError"] = "Invalid task status.";
                return RedirectToAction("StaffTasks", "Cust");
            }

            db.SaveChanges();

            TempData["StaffTaskSuccess"] = "Task status updated successfully.";
            return RedirectToAction("StaffTasks", "Cust");
        }

        [HttpGet]
        public ActionResult CustomerComplaints(int? bookingId)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];

            ViewBag.Bookings = db.Bookings
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.EventDate)
                .ToList();

            ViewBag.SelectedBookingId = bookingId;

            var complaints = db.CustomerComplaints
                .Where(x => x.CustomerId == customerId)
                .Include("Booking")
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(complaints);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCustomerComplaint(int bookingId, string category, string subject, string description)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Cust");
            }

            int customerId = (int)Session["CustomerId"];

            var booking = db.Bookings.FirstOrDefault(b =>
                b.BookingId == bookingId &&
                b.CustomerId == customerId);

            if (booking == null)
            {
                TempData["CustomerComplaintError"] = "The selected booking could not be found.";
                return RedirectToAction("CustomerComplaints", "Cust");
            }

            if (string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["CustomerComplaintError"] = "Please complete the complaint category, subject and description.";
                return RedirectToAction("CustomerComplaints", "Cust", new { bookingId = bookingId });
            }

            var complaint = new CustomerComplaint
            {
                CustomerId = customerId,
                BookingId = bookingId,
                Category = category.Trim(),
                Subject = subject.Trim(),
                Description = description.Trim(),
                Status = "Submitted",
                CreatedAt = DateTime.Now
            };

            db.CustomerComplaints.Add(complaint);
            db.SaveChanges();

            var complaintEmailService = new OtpDeliveryService();
            bool complaintEmailSent = complaintEmailService.SendCustomerComplaintConfirmationEmail(
                booking.Email,
                booking.FirstName,
                complaint.ComplaintId,
                booking.BookingId,
                complaint.Category,
                complaint.Subject,
                complaint.CreatedAt
            );

            TempData["CustomerComplaintSuccess"] = complaintEmailSent
                ? "Your complaint has been submitted and a confirmation email has been sent to you."
                : "Your complaint has been submitted successfully, but we could not send the confirmation email right now.";

            return RedirectToAction("CustomerComplaints", "Cust", new { bookingId = bookingId });
        }

        [HttpGet]
        public ActionResult AdminCustomerComplaints()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            var complaints = db.CustomerComplaints
                .Include("Customer")
                .Include("Booking")
                .OrderBy(x => x.Status == "Submitted" ? 0 : x.Status == "Under Review" ? 1 : 2)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return View(complaints);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RespondToCustomerComplaint(int complaintId, string adminResponse, string status)
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            var complaint = db.CustomerComplaints.FirstOrDefault(x => x.ComplaintId == complaintId);
            if (complaint == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(adminResponse))
            {
                TempData["CustomerComplaintAdminError"] = "Please enter a response before updating the complaint.";
                return RedirectToAction("AdminCustomerComplaints", "Cust");
            }

            var allowedStatuses = new[] { "Under Review", "Resolved", "Closed" };
            complaint.Status = allowedStatuses.Contains(status) ? status : "Under Review";
            complaint.AdminResponse = adminResponse.Trim();
            complaint.ResolvedAt = complaint.Status == "Resolved" || complaint.Status == "Closed"
                ? (DateTime?)DateTime.Now
                : null;

            db.SaveChanges();

            TempData["CustomerComplaintAdminSuccess"] = "Customer complaint updated successfully.";
            return RedirectToAction("AdminCustomerComplaints", "Cust");
        }

        [HttpGet]
        public ActionResult AdminStaffComplaints()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            var complaints = db.StaffComplaints
                .Include("Staff")
                .Include("Booking")
                .OrderBy(cmp => cmp.Status == "Open" ? 0 : 1)
                .ThenByDescending(cmp => cmp.CreatedAt)
                .ToList();

            return View(complaints);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RespondToStaffComplaint(int complaintId, string adminResponse, string status)
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            var complaint = db.StaffComplaints.FirstOrDefault(cmp => cmp.ComplaintId == complaintId);
            if (complaint == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(adminResponse))
            {
                TempData["ComplaintAdminError"] = "A response is required.";
                return RedirectToAction("AdminStaffComplaints");
            }

            complaint.AdminResponse = adminResponse.Trim();
            complaint.Status = status == "Resolved" ? "Resolved" : "Open";
            complaint.ResolvedAt = complaint.Status == "Resolved" ? (DateTime?)DateTime.Now : null;
            db.SaveChanges();

            TempData["ComplaintAdminSuccess"] = "Complaint updated successfully.";
            return RedirectToAction("AdminStaffComplaints");
        }

        public ActionResult StaffComplaints()
        {
            if (!IsStaffAuthenticated())
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];


            var complaints = db.StaffComplaints
                .Where(c => c.StaffId == staffId)
                .Include("Booking")
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.StaffTasks = db.StaffTasks
                .Where(t => t.StaffId == staffId && t.BookingId != null)
                .Include("Booking")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return View(complaints);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStaffComplaint(
            string title,
            string description,
            string priority,
            int? bookingId)
        {
            if (!IsStaffAuthenticated())
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["ComplaintError"] = "A title and description are required.";
                return RedirectToAction("StaffComplaints", "Cust");
            }

            if (priority != "Low" && priority != "Medium" && priority != "High")
            {
                priority = "Medium";
            }

            int staffId = (int)Session["StaffId"];

            if (bookingId.HasValue &&
                !db.StaffTasks.Any(t =>
                    t.StaffId == staffId &&
                    t.BookingId == bookingId.Value))
            {
                TempData["ComplaintError"] = "You can only link a complaint to one of your assigned bookings.";
                return RedirectToAction("StaffComplaints", "Cust");
            }

            db.StaffComplaints.Add(new StaffComplaint
            {
                StaffId = staffId,
                BookingId = bookingId,
                Title = title.Trim(),
                Description = description.Trim(),
                Priority = priority,
                Status = "Open",
                CreatedAt = DateTime.Now
            });

            db.SaveChanges();

            TempData["ComplaintSuccess"] = "Complaint submitted successfully.";
            return RedirectToAction("StaffComplaints", "Cust");
        }

        public ActionResult StaffProfile()
        {
            if (!IsStaffAuthenticated())
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];
            var staff = db.Staffs.FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            return View(staff);
        }

        private bool IsStaffAuthenticated()
        {
            return Session["StaffId"] != null &&
                   Session["StaffAuthenticated"] != null &&
                   (bool)Session["StaffAuthenticated"];
        }

        private void ClearRoleSessions()
        {
            Session.Remove("CustomerId");
            Session.Remove("CustomerEmail");
            Session.Remove("CustomerFirstName");
            Session.Remove("CustomerAuthenticated");

            Session.Remove("StaffId");
            Session.Remove("StaffEmail");
            Session.Remove("StaffFirstName");
            Session.Remove("StaffAuthenticated");

            Session.Remove("AdminId");
            Session.Remove("AdminEmail");
            Session.Remove("AdminFirstName");
            Session.Remove("AdminAuthenticated");
        }

        [HttpGet]
        public ActionResult StaffLogin()
        {
            if (IsStaffAuthenticated())
            {
                return RedirectToAction("StaffDashboard", "Cust");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Please enter your email address and password.");
                return View();
            }

            var staff = db.Staffs.FirstOrDefault(s => s.staff_Email == email.Trim());

            if (staff == null)
            {
                ModelState.AddModelError("", "Invalid email address or password.");
                return View();
            }

            bool passwordValid = false;

            try
            {
                passwordValid = Crypto.VerifyHashedPassword(staff.staff_Passw, password);
            }
            catch
            {
                passwordValid = false;
            }

            if (!passwordValid)
            {
                ModelState.AddModelError("", "Invalid email address or password.");
                return View();
            }

            ClearRoleSessions();

            Session["StaffId"] = staff.staff_ID;
            Session["StaffEmail"] = staff.staff_Email;
            Session["StaffFirstName"] = staff.staff_FName;
            Session["StaffAuthenticated"] = true;

            return RedirectToAction("StaffDashboard", "Cust");
        }

        [HttpGet]
        public ActionResult StaffLogout()
        {
            Session.Remove("StaffId");
            Session.Remove("StaffEmail");
            Session.Remove("StaffFirstName");
            Session.Remove("StaffAuthenticated");

            return RedirectToAction("Login", "Cust");
        }

        [HttpGet]
        public ActionResult FinancialOverview()
        {
            if (Session["AdminId"] == null || Session["AdminAuthenticated"] == null || !(bool)Session["AdminAuthenticated"])
                return RedirectToAction("Login", "Cust");

            decimal income = db.Bookings
                .Where(b =>
                    b.Status != "Declined" &&
                    b.Status != "Cancelled")
                .Select(b => (decimal?)b.AmountPaid)
                .Sum() ?? 0m;

            var expenses = db.Expenditures.OrderByDescending(e => e.ExpenseDate).ToList();
            decimal expenditure = expenses.Select(e => e.Amount).DefaultIfEmpty(0m).Sum();

            return View(new FinancialOverviewViewModel
            {
                Income = income,
                TotalExpenditure = expenditure,
                Expenditures = expenses
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddExpenditure(string description, decimal amount, DateTime expenseDate, string category)
        {
            if (Session["AdminId"] == null || Session["AdminAuthenticated"] == null || !(bool)Session["AdminAuthenticated"])
                return RedirectToAction("Login", "Cust");

            if (string.IsNullOrWhiteSpace(description) || amount <= 0)
            {
                TempData["FinanceError"] = "Enter a description and an expenditure amount greater than zero.";
                return RedirectToAction("FinancialOverview");
            }

            db.Expenditures.Add(new Expenditure
            {
                Description = description.Trim(),
                Amount = amount,
                ExpenseDate = expenseDate,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
            TempData["FinanceSuccess"] = "Expenditure recorded.";
            return RedirectToAction("FinancialOverview");
        }

        [HttpGet]
        public ActionResult StaffManagement()
        {
            if (Session["AdminId"] == null || Session["AdminAuthenticated"] == null || !(bool)Session["AdminAuthenticated"])
                return RedirectToAction("Login", "Cust");

            var staff = db.Staffs.OrderBy(s => s.staff_City).ThenBy(s => s.staff_FName).ToList();
            var pendingCounts = db.StaffTasks
                .Where(t => t.Status == "Pending")
                .GroupBy(t => t.StaffId)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.PendingTaskCounts = pendingCounts;
            return View(staff);
        }

        [HttpGet]
        public ActionResult EditStaff(int id)
        {
            if (Session["AdminId"] == null || Session["AdminAuthenticated"] == null || !(bool)Session["AdminAuthenticated"])
                return RedirectToAction("Login", "Cust");

            var staff = db.Staffs.FirstOrDefault(s => s.staff_ID == id);
            if (staff == null) return HttpNotFound();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStaff(Staff model)
        {
            if (Session["AdminId"] == null || Session["AdminAuthenticated"] == null || !(bool)Session["AdminAuthenticated"])
                return RedirectToAction("Login", "Cust");

            var staff = db.Staffs.FirstOrDefault(s => s.staff_ID == model.staff_ID);
            if (staff == null) return HttpNotFound();

            // Password is optional when editing an existing staff account.
            ModelState.Remove("staff_Passw");

            if (string.IsNullOrWhiteSpace(model.staff_FName) || string.IsNullOrWhiteSpace(model.staff_LName) ||
                string.IsNullOrWhiteSpace(model.staff_Email) || string.IsNullOrWhiteSpace(model.staff_Phone) ||
                string.IsNullOrWhiteSpace(model.staff_Type))
            {
                ModelState.AddModelError("", "Please complete all staff details.");
                return View(model);
            }

            string editedEmail = model.staff_Email.Trim();
            if (db.Staffs.Any(s => s.staff_ID != model.staff_ID && s.staff_Email == editedEmail))
            {
                ModelState.AddModelError("staff_Email", "A staff account with this email already exists.");
                return View(model);
            }

            // Team is the source of truth for staff assignment. Derive the city
            // server-side so a tampered form cannot save a mismatched team/city pair.
            if (model.staff_Type == "Team Dbn")
            {
                model.staff_City = "Durban";
            }
            else if (model.staff_Type == "Team Peter")
            {
                model.staff_City = "Pietermaritzburg";
            }
            else if (model.staff_Type == "Team Mdn")
            {
                model.staff_City = "Mandeni";
            }
            else
            {
                ModelState.AddModelError("staff_Type", "Select a valid staff team.");
                return View(model);
            }

            staff.staff_FName = model.staff_FName.Trim();
            staff.staff_LName = model.staff_LName.Trim();
            staff.staff_Email = editedEmail;
            staff.staff_Phone = model.staff_Phone.Trim();
            staff.staff_Type = model.staff_Type;
            staff.staff_City = model.staff_City;

            // Keep the existing password unless the admin deliberately supplies a replacement.
            if (!string.IsNullOrWhiteSpace(model.staff_Passw))
                staff.staff_Passw = Crypto.HashPassword(model.staff_Passw);

            db.SaveChanges();
            TempData["StaffManagementSuccess"] = "Staff details updated.";
            return RedirectToAction("StaffManagement");
        }

        [HttpGet]
        public ActionResult RegisterStaff()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            return View(new Staff());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterStaff(
            [Bind(Include = "staff_FName,staff_LName,staff_Email,staff_Passw,staff_Phone,staff_Type")]
            Staff staff)
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            // City is derived from the selected team below, so it is intentionally
            // not posted by the form and must not fail model validation here.
            ModelState.Remove("staff_City");

            if (!ModelState.IsValid)
            {
                ClearInvalidRegistrationValues(
                    "staff_FName",
                    "staff_LName",
                    "staff_Email",
                    "staff_Passw",
                    "staff_Phone",
                    "staff_Type");

                return View(staff);
            }

            // Team is the source of truth for assignment on registration too.
            if (staff.staff_Type == "Team Dbn")
            {
                staff.staff_City = "Durban";
            }
            else if (staff.staff_Type == "Team Peter")
            {
                staff.staff_City = "Pietermaritzburg";
            }
            else if (staff.staff_Type == "Team Mdn")
            {
                staff.staff_City = "Mandeni";
            }
            else
            {
                ModelState.AddModelError("staff_Type", "Select a valid staff team.");
                ClearInvalidRegistrationValues("staff_Type");
                return View(staff);
            }

            staff.staff_FName = staff.staff_FName.Trim();
            staff.staff_LName = staff.staff_LName.Trim();
            staff.staff_Email = staff.staff_Email.Trim();

            if (db.Staffs.Any(s => s.staff_Email == staff.staff_Email))
            {
                ModelState.AddModelError(
                    "staff_Email",
                    "A staff account with this email already exists."
                );

                ClearInvalidRegistrationValues("staff_Email");
                return View(staff);
            }

            staff.staff_Passw = Crypto.HashPassword(staff.staff_Passw);

            db.Staffs.Add(staff);
            db.SaveChanges();

            var staffRegistrationEmailService = new OtpDeliveryService();
            bool staffRegistrationEmailSent =
                staffRegistrationEmailService.SendStaffRegistrationEmail(
                    staff.staff_Email,
                    staff.staff_FName,
                    staff.staff_Type,
                    staff.staff_City);

            TempData["StaffManagementSuccess"] = staffRegistrationEmailSent
                ? "Staff member registered successfully. A confirmation email has been sent to " + staff.staff_Email + "."
                : "Staff member registered successfully, but the confirmation email could not be sent right now.";

            return RedirectToAction("StaffManagement", "Cust");
        }
        private void ClearInvalidRegistrationValues(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var entry = ModelState[fieldName];

                if (entry == null || entry.Errors.Count == 0)
                    continue;

                ModelState.SetModelValue(
                    fieldName,
                    new ValueProviderResult(
                        string.Empty,
                        string.Empty,
                        System.Globalization.CultureInfo.CurrentCulture));
            }
        }

        private string NormalizeCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return string.Empty;
            }

            string value = city.Trim().ToLowerInvariant();

            if (value == "dbn" || value == "durban" || value.Contains("ethekwini"))
                return "durban";

            if (value == "pmb" || value == "pietermaritzburg" || value.Contains("msunduzi"))
                return "pietermaritzburg";

            if (value == "mandeni" || value.Contains("mandeni local municipality"))
                return "mandeni";

            return value;
        }

    }


    
}

