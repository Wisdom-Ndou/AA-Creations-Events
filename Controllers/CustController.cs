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
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.Services;
using static WebApplication1.Models.Bankingdetailsviewmodel;

namespace WebApplication1.Controllers
{
    public class CustController : Controller
    {
        private readonly DatabaseContext db = new DatabaseContext();

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
                    "Index",
                    "Cust"
                );
            }

            // ==========================================
            // ADMIN LOGIN
            // ==========================================

            if (role == "admin")
            {
                // Admin access code is NOT stored in the database.
                // It is stored in Web.config.
                string correctAccessCode =
                    ConfigurationManager.AppSettings["AdminAccessCode"];

                if (string.IsNullOrWhiteSpace(adminAccessCode))
                {
                    ModelState.AddModelError(
                        "",
                        "Please enter the admin access code."
                    );

                    return View();
                }

                if (string.IsNullOrWhiteSpace(correctAccessCode) ||
                    !string.Equals(
                        adminAccessCode.Trim(),
                        correctAccessCode.Trim(),
                        StringComparison.Ordinal))
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

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_Email == email);

            if (customer == null)
            {
                ViewBag.ErrorMessage =
                    "No account was found with that email address.";

                return View();
            }

            // Start a fresh Forgot Password OTP flow.
            Session.Remove("OtpVerified");
            Session.Remove("OtpPurpose");
            Session.Remove("TestOtp");

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

            if (deliveryMethod != "Email" && deliveryMethod != "Phone")
            {
                TempData["OtpError"] = "Please select a valid verification method.";
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

            db.OtpVerifications.Add(otpVerification);
            db.SaveChanges();

            var deliveryService = new OtpDeliveryService();

            bool sent;

            if (deliveryMethod == "Email")
            {
                sent = deliveryService.SendOtpByEmail(
                    customer.Cust_Email,
                    otp
                );
            }
            else
            {
                sent = deliveryService.SendOtpByPhone(
                    customer.Cust_Phone,
                    otp
                );
            }

            if (!sent)
            {
                TempData["OtpError"] =
                    "We could not send the verification code.";

                return RedirectToAction(
                    "ForgotPasswordMethod",
                    "Cust"
                );
            }

            // TEMPORARY TESTING ONLY
            Session["TestOtp"] = otp;

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
            string cookiePreference)
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
            string configuredAccessCode = ConfigurationManager.AppSettings["AdminAccessCode"];

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                password != confirm ||
                string.IsNullOrWhiteSpace(adminAccessCode) ||
                string.IsNullOrWhiteSpace(configuredAccessCode) ||
                !string.Equals(adminAccessCode.Trim(), configuredAccessCode.Trim(), StringComparison.Ordinal))
            {
                ModelState.AddModelError("", "Please provide valid admin registration details and authorization code.");
                return View();
            }

            if (db.Admins.Any(a => a.admin_Email == email.Trim()))
            {
                ModelState.AddModelError("", "An administrator with this email address already exists.");
                return View();
            }

            var admin = new Admin
            {
                admin_FName = firstName.Trim(),
                admin_LName = lastName.Trim(),
                admin_Email = email.Trim(),
                admin_Passw = Crypto.HashPassword(password),
                admin_Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim()
            };

            db.Admins.Add(admin);
            db.SaveChanges();

            TempData["AdminRegistrationSuccess"] =
                "Admin registration was successful. You can now sign in.";

            // Redirect to the customer Login page so the success message shows on Login.cshtml
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

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "The booking information is invalid."
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

            try
            {
                var allowedCities = new[] { "Durban", "Pietermaritzburg", "Mthatha" };
                var requestedCity = (request.City ?? "").Trim();

                if (!allowedCities.Any(city =>
                    city.Equals(requestedCity, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select Durban, Pietermaritzburg or Mthatha."
                    });
                }

                request.City = allowedCities.First(city =>
                    city.Equals(requestedCity, StringComparison.OrdinalIgnoreCase));

                // Find the package in the database
                var package = db.Packages
                    .FirstOrDefault(p => p.PackageId == request.PackageId);

                if (package == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "The selected package is invalid."
                    });
                }

                // Get the submitted add-on IDs
                var requestedAddOnIds = request.AddOns ?? new List<string>();

                // Look up the actual add-ons and their prices from the database
                var selectedAddOns = db.AddOns
                    .Where(a => requestedAddOnIds.Contains(a.AddOnId))
                    .ToList();

                // Make sure every submitted add-on actually exists
                if (selectedAddOns.Count != requestedAddOnIds.Count)
                {
                    return Json(new
                    {
                        success = false,
                        message = "One or more selected add-ons are invalid."
                    });
                }

                // SERVER-AUTHORITATIVE PRICE CALCULATION
                decimal totalPrice = package.Price;

                totalPrice += selectedAddOns.Sum(a => a.Price);

                // Create the Booking entity
                var customerId = (int)Session["CustomerId"];

                var booking = new Booking
                {
                    CustomerId = customerId,

                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Occasion = request.Occasion,
                    EventDate = request.EventDate,
                    EventTime = request.EventTime,
                    Address = request.Address,
                    City = request.City,
                    Notes = request.Notes,

                    // Use the validated package ID
                    PackageId = package.PackageId,

                    // Use the SERVER-CALCULATED price
                    TotalPrice = totalPrice,

                    CreatedAt = DateTime.Now,
                    Status = "Pending"
                };

                // Add selected add-ons to the booking
                foreach (var addOn in selectedAddOns)
                {
                    booking.BookingAddOns.Add(new BookingAddOn
                    {
                        AddOnId = addOn.AddOnId
                    });
                }

                // Save the booking first. Staff work is created only after
                // an administrator approves the booking.
                db.Bookings.Add(booking);
                db.SaveChanges();

                // Send a receipt/booking-received email after the booking is safely stored.
                // A delivery failure does not roll back the booking.
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
                    message = "Booking created successfully."
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
            }

            return View(bookings);
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

        public JsonResult TestDatabase()
        {
            int bookingCount = db.Bookings.Count();

            return Json(new
            {
                success = true,
                bookingCount = bookingCount,
                message = "Database connection is working."
            }, JsonRequestBehavior.AllowGet);
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

            var customer = db.Customers
                .FirstOrDefault(c => c.Cust_ID == customerId);

            if (customer == null)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction("Login", "Cust");
            }

            // Password is not being changed on the Manage Account page.
            // Therefore, remove password validation from this request.
            ModelState.Remove("Cust_Passw");

            // Check whether another customer already uses this email.
            if (db.Customers.Any(c =>
                c.Cust_Email == obj.Cust_Email &&
                c.Cust_ID != customerId))
            {
                ModelState.AddModelError(
                    "Cust_Email",
                    "An account with this email address already exists."
                );

                return View(customer);
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Update only the information that the user is allowed
            // to change on the Manage Account page.
            customer.Cust_FName = obj.Cust_FName;
            customer.Cust_LName = obj.Cust_LName;
            customer.Cust_Email = obj.Cust_Email;
            customer.Cust_Phone = obj.Cust_Phone;

            // Tell Entity Framework that this existing customer was modified.
            db.Entry(customer).State = EntityState.Modified;

            // Permanently save the changes to the database.
            db.SaveChanges();

            // Keep the session information up to date.
            Session["CustomerEmail"] = customer.Cust_Email;
            Session["CustomerFirstName"] = customer.Cust_FName;

            TempData["AccountSuccess"] =
                "Your account details have been updated successfully.";

            return RedirectToAction("ManageAccount");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Cust");
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

            if (deliveryMethod != "Email" && deliveryMethod != "Phone")
            {
                TempData["OtpError"] = "Invalid OTP delivery method.";
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

            db.OtpVerifications.Add(otpVerification);
            db.SaveChanges();

            var deliveryService = new OtpDeliveryService();

            bool sent;

            if (deliveryMethod == "Email")
            {
                sent = deliveryService.SendOtpByEmail(
                    customer.Cust_Email,
                    otp
                );
            }
            else
            {
                sent = deliveryService.SendOtpByPhone(
                    customer.Cust_Phone,
                    otp
                );
            }

            if (!sent)
            {
                TempData["OtpError"] =
                    "We could not send the verification code.";

                return RedirectToAction("ManageAccount", "Cust");
            }

            /*
             * TEMPORARY TESTING ONLY
             *
             * Remove this once actual email/SMS delivery is connected.
             */
            // TEMPORARY TESTING ONLY
            // Store the plaintext OTP in session so it remains visible
            // if the user enters the wrong code.
            Session["TestOtp"] = otp;

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

            ViewBag.TestOtp = Session["TestOtp"];

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

            ViewBag.TestOtp = Session["TestOtp"];

            if (string.IsNullOrWhiteSpace(otp))
            {
                ModelState.AddModelError(
                    "",
                    "Please enter the verification code."
                );

                return View();
            }

            var verification = db.OtpVerifications
                .Where(o =>
                    o.CustomerId == customerId &&
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

                db.SaveChanges();

                ModelState.AddModelError(
                    "",
                    "Incorrect verification code."
                );

                return View();
            }

            verification.IsUsed = true;

            db.SaveChanges();

            // Remove the test OTP now that it has been successfully used.
            Session.Remove("TestOtp");

            Session["OtpVerified"] = true;
            Session["OtpPurpose"] = verification.Purpose;

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
            Session.Remove("TestOtp");

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
                        b.Status.ToLower() == "approved")
                    .Select(b => (decimal?)b.TotalPrice)
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

                    // Return the error as JSON so the caller can diagnose
                    // why the staff task was not created.
                    return Json(new
                    {
                        success = false,
                        message = "An error occurred while creating the staff task: " + ex.Message
                    });
                }
            }

            if (!Request.IsAjaxRequest())
            {
                TempData["AdminSuccess"] = newStatus == "Approved"
                    ? "Booking approved and staff task assigned successfully."
                    : "Booking status updated successfully.";

                return RedirectToAction("AdminDashboard", "Cust");
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

            string correctAccessCode = "AACode";

            correctAccessCode = correctAccessCode?.Trim();

            if (string.IsNullOrWhiteSpace(correctAccessCode))
            {
                ModelState.AddModelError(
                    "",
                    "Admin authorization code is not configured."
                );

                return View();
            }

            if (!string.Equals(
                    adminAccessCode,
                    correctAccessCode,
                    StringComparison.Ordinal))
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
                TempData["ComplaintAdminSuccess"] = "A response is required.";
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
                .Where(b => b.Status == "Approved" || b.Status == "Setup Completed" || b.Status == "Completed")
                .Select(b => (decimal?)b.TotalPrice)
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

            if (string.IsNullOrWhiteSpace(model.staff_FName) || string.IsNullOrWhiteSpace(model.staff_LName) ||
                string.IsNullOrWhiteSpace(model.staff_Email) || string.IsNullOrWhiteSpace(model.staff_Phone) ||
                string.IsNullOrWhiteSpace(model.staff_Type) || string.IsNullOrWhiteSpace(model.staff_City))
            {
                ModelState.AddModelError("", "Please complete all staff details.");
                return View(model);
            }

            staff.staff_FName = model.staff_FName.Trim();
            staff.staff_LName = model.staff_LName.Trim();
            staff.staff_Email = model.staff_Email.Trim();
            staff.staff_Phone = model.staff_Phone.Trim();
            staff.staff_Type = model.staff_Type;
            staff.staff_City = model.staff_City;

            // Keep the existing password unless the admin deliberately supplies a replacement.
            if (!string.IsNullOrWhiteSpace(model.staff_Passw))
                staff.staff_Passw = model.staff_Passw;

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
            [Bind(Include = "staff_FName,staff_LName,staff_Email,staff_Passw,staff_Phone,staff_Type,staff_City")]
            Staff staff)
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            if (!ModelState.IsValid)
            {
                return View(staff);
            }

            string canonicalCity = NormalizeCity(staff.staff_City);
            if (canonicalCity == "durban")
            {
                staff.staff_City = "Durban";
                staff.staff_Type = "Team Dbn";
            }
            else if (canonicalCity == "pietermaritzburg")
            {
                staff.staff_City = "Pietermaritzburg";
                staff.staff_Type = "Team Peter";
            }
            else if (canonicalCity == "mthatha")
            {
                staff.staff_City = "Mthatha";
                staff.staff_Type = "Team Mdn";
            }
            else
            {
                ModelState.AddModelError("staff_City", "Select Durban, Pietermaritzburg or Mthatha.");
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

                return View(staff);
            }

            staff.staff_Passw = Crypto.HashPassword(staff.staff_Passw);

            db.Staffs.Add(staff);
            db.SaveChanges();

            return RedirectToAction("AdminDashboard", "Cust");
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

            if (value == "mthatha" || value == "umtata")
                return "mthatha";

            return value;
        }

    }


    
}

