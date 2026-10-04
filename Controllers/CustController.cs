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
     string accessCode)
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
                // ==========================================
                // ADMIN ACCESS CODE
                // ==========================================

                string correctAccessCode =
    ConfigurationManager.AppSettings["AdminAccessCode"];

                if (correctAccessCode == null)
                {
                    ModelState.AddModelError(
                        "",
                        "DEBUG: AdminAccessCode key was NOT found in the running Web.config."
                    );

                    return View();
                }

                if (correctAccessCode.Length == 0)
                {
                    ModelState.AddModelError(
                        "",
                        "DEBUG: AdminAccessCode exists but its value is empty."
                    );

                    return View();
                }

                accessCode = accessCode.Trim();
                correctAccessCode = correctAccessCode.Trim();

                if (!string.Equals(
                    accessCode,
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Customerregister(Customer obj)
        {
            // Password policy validation
            if (string.IsNullOrWhiteSpace(obj.Cust_Passw) ||
                obj.Cust_Passw.Length < 6 ||
                obj.Cust_Passw.Length > 15 ||
                !obj.Cust_Passw.Any(char.IsLetter) ||
                !obj.Cust_Passw.Any(char.IsDigit))
            {
                ModelState.AddModelError(
                    "Cust_Passw",
                    "Password must be 6 to 15 characters long and contain at least one letter and one number."
                );

                return View(obj);
            }

            // Check whether email is already registered
            if (db.Customers.Any(c => c.Cust_Email == obj.Cust_Email))
            {
                ModelState.AddModelError(
                    "Cust_Email",
                    "An account with this email address already exists."
                );

                return View(obj);
            }

            // Hash password
            obj.Cust_Passw = Crypto.HashPassword(obj.Cust_Passw);

            // Ensure CreatedAt is a valid SQL datetime value
            obj.CreatedAt = DateTime.Now;

            // Persist
            db.Customers.Add(obj);
            db.SaveChanges();

            TempData["RegistrationSuccess"] =
                "Your registration was successful. You can now sign in and start booking.";

            // Redirect back to registration (existing behavior) or to Login if preferred
            return RedirectToAction("Customerregister", "Cust");
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
            // existing validation omitted for brevity...

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

                // Save everything as one transaction
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Bookings.Add(booking);

                        db.SaveChanges();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

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
        public JsonResult UpdateBookingStatus(int bookingId, string status)
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


            // Update database
            booking.Status = newStatus;

            db.SaveChanges();


            return Json(new
            {
                success = true,
                bookingId = booking.BookingId,
                status = newStatus,
                message = "Booking status updated successfully."
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
                    b.Status.ToLower() == "confirmed")
                .OrderBy(b => b.EventDate)
                .ToList();


            // ==========================================
            // CANCELLED BOOKINGS
            // ==========================================

            var cancelledBookings = allBookings
                .Where(b =>
                    b.Status != null &&
                    b.Status.ToLower() == "cancelled")
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
            // Check that an authenticated admin is logged in
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            // Get all bookings that are still waiting for admin approval
            var bookings = db.Bookings
                .Where(b =>
                    b.Status != null &&
                    b.Status.ToLower() == "pending")
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(bookings);
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

            // ---------------------------------------------------------
            // BOOKING COUNTS
            // ---------------------------------------------------------

            int totalBookings = db.Bookings.Count();

            int approvedBookings = db.Bookings.Count(b =>
                b.Status != null &&
                b.Status.ToLower() == "approved");

            int pendingBookings = db.Bookings.Count(b =>
                b.Status != null &&
                b.Status.ToLower() == "pending");

            int declinedBookings = db.Bookings.Count(b =>
                b.Status != null &&
                b.Status.ToLower() == "declined");


            // ---------------------------------------------------------
            // APPROVED BOOKING REVENUE
            // ---------------------------------------------------------

            decimal totalRevenue =
                db.Bookings
                    .Where(b =>
                        b.Status != null &&
                        b.Status.ToLower() == "approved")
                    .Select(b => (decimal?)b.TotalPrice)
                    .Sum() ?? 0m;


            // ---------------------------------------------------------
            // EXPENDITURE
            // ---------------------------------------------------------

            decimal totalExpenditure =
                db.Expenses
                    .Select(e => (decimal?)e.Amount)
                    .Sum() ?? 0m;


            // ---------------------------------------------------------
            // NET PROFIT
            // ---------------------------------------------------------

            decimal netProfit =
                totalRevenue - totalExpenditure;


            // ---------------------------------------------------------
            // AVERAGE APPROVED BOOKING VALUE
            // ---------------------------------------------------------

            decimal averageBookingValue = approvedBookings > 0
                ? totalRevenue / approvedBookings
                : 0m;


            // ---------------------------------------------------------
            // REVENUE BY OCCASION
            // ---------------------------------------------------------

            var revenueByOccasion = db.Bookings
                .Where(b =>
                    b.Status != null &&
                    b.Status.ToLower() == "approved")
                .GroupBy(b => b.Occasion)
                .Select(g => new AnalyticsCategory
                {
                    Name = g.Key,
                    Amount = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();


            // ---------------------------------------------------------
            // RECENT EXPENSES
            // ---------------------------------------------------------

            var recentExpenses = db.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(10)
                .ToList();


            // ---------------------------------------------------------
            // CREATE VIEW MODEL
            // ---------------------------------------------------------

            var model = new BusinessAnalyticsViewModel
            {
                TotalRevenue = totalRevenue,

                TotalExpenditure = totalExpenditure,

                NetProfit = netProfit,

                TotalBookings = totalBookings,

                ApprovedBookings = approvedBookings,

                PendingBookings = pendingBookings,

                DeclinedBookings = declinedBookings,

                AverageBookingValue = averageBookingValue,

                RevenueByOccasion = revenueByOccasion,

                RecentExpenses = recentExpenses
            };


            return View(model);
        }


        // ==============================================
        // ADMIN - VIEW STAFF EVENT ISSUES
        // ==============================================

        [HttpGet]
        public ActionResult AdminComplaints()
        {
            // IMPORTANT:
            // Put the SAME admin authentication check here
            // that you already use in AdminDashboard or AllBookings.
            if (Session["AdminId"] == null ||
               Session["AdminAuthenticated"] == null ||
               !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            // ==========================================
            // GET ALL STAFF COMPLAINTS
            // ==========================================

            var complaints = db.StaffComplaints
                .Include("Staff")
                .Include("Booking")
                .OrderBy(c => c.Status == "Resolved")
                .ThenByDescending(c => c.Priority == "High")
                .ThenByDescending(c => c.CreatedAt)
                .ToList();

            return View(complaints);
        }



        // ==============================================
        // ADMIN - RESOLVE STAFF EVENT ISSUE
        // ==============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResolveStaffComplaint(int complaintId)
        {
            // IMPORTANT:
            // Put the SAME admin authentication check here
            // that you use in AdminComplaints.


            // ==========================================
            // FIND COMPLAINT
            // ==========================================

            var complaint = db.StaffComplaints
                .FirstOrDefault(c =>
                    c.ComplaintId == complaintId);

            if (complaint == null)
            {
                TempData["ComplaintError"] =
                    "The reported issue could not be found.";

                return RedirectToAction("AdminComplaints");
            }


            // ==========================================
            // CHECK IF ALREADY RESOLVED
            // ==========================================

            if (complaint.Status == "Resolved")
            {
                TempData["ComplaintError"] =
                    "This issue has already been resolved.";

                return RedirectToAction("AdminComplaints");
            }


            // ==========================================
            // RESOLVE COMPLAINT
            // ==========================================

            complaint.Status = "Resolved";

            complaint.ResolvedAt = DateTime.Now;

            db.SaveChanges();


            TempData["ComplaintSuccess"] =
                "The event issue has been marked as resolved.";

            return RedirectToAction("AdminComplaints");
        }



        // ==========================================
        // STAFF INFORMATION
        // ==========================================

        [HttpGet]
        public ActionResult StaffInformation()
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
        // REGISTER STAFF - GET
        // ==========================================

        [HttpGet]
        public ActionResult RegisterStaff()
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
        // REGISTER STAFF - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterStaff(Staff staff)
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

            db.Staffs.Add(staff);

            db.SaveChanges();

            TempData["StaffSuccess"] =
                "Staff member registered successfully.";

            return RedirectToAction("ViewStaff", "Cust");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffRegister(Staff staff)
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

            // Check whether email already exists
            bool emailExists =
                db.Staffs.Any(s => s.staff_Email == staff.staff_Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "staff_Email",
                    "A staff member with this email already exists."
                );

                return View(staff);
            }

            db.Staffs.Add(staff);

            db.SaveChanges();

            TempData["StaffSuccess"] =
                "Staff member registered successfully.";

            return RedirectToAction("StaffInformation", "Cust");
        }



        [HttpGet]
        public ActionResult StaffTasks()
        {
            // ==========================================
            // MAKE SURE STAFF MEMBER IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            // ==========================================
            // GET LOGGED-IN STAFF ID
            // ==========================================

            int staffId = (int)Session["StaffId"];

            // ==========================================
            // MAKE SURE STAFF MEMBER STILL EXISTS
            // ==========================================

            var staff = db.Staffs.FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();
                return RedirectToAction("StaffLogin", "Cust");
            }

            // ==========================================
            // GET THIS STAFF MEMBER'S TASKS
            // ==========================================

            var tasks = db.StaffTasks
            .Include("Booking")
            .Where(t => t.StaffId == staffId)
            .OrderBy(t => t.Status == "Completed")
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority == "High")
            .ToList();

            // ==========================================
            // SEND STAFF INFORMATION TO VIEW
            // ==========================================

            ViewBag.StaffName = staff.staff_FName + " " + staff.staff_LName;
            ViewBag.StaffType = staff.staff_Type;

            return View(tasks);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStaffTaskStatus(int taskId, string newStatus)
        {
            // ==========================================
            // MAKE SURE STAFF MEMBER IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            // ==========================================
            // VALIDATE STATUS
            // ==========================================

            var allowedStatuses = new[]
            {
        "Pending",
        "In Progress",
        "Completed"
    };

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["TaskError"] = "Invalid task status.";
                return RedirectToAction("StaffTasks");
            }

            // ==========================================
            // FIND TASK
            // IMPORTANT:
            // Task must belong to logged-in staff member
            // ==========================================

            var task = db.StaffTasks.FirstOrDefault(t =>
                t.TaskId == taskId &&
                t.StaffId == staffId);

            if (task == null)
            {
                TempData["TaskError"] =
                    "Task could not be found or is not assigned to you.";

                return RedirectToAction("StaffTasks");
            }

            // ==========================================
            // UPDATE STATUS
            // ==========================================

            task.Status = newStatus;

            db.SaveChanges();

            TempData["TaskSuccess"] =
                "Task status updated successfully.";

            return RedirectToAction("StaffTasks");
        }


        [HttpGet]
        public ActionResult StaffEvents()
        {
            // ==========================================
            // MAKE SURE STAFF MEMBER IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction(
                    "StaffLogin",
                    "Cust"
                );
            }


            // ==========================================
            // DETERMINE STAFF TEAM CITY
            // ==========================================

            string teamCity = "";

            switch (staff.staff_Type)
            {
                case "Team Dbn":
                    teamCity = "Durban";
                    break;

                case "Team Peter":
                    teamCity = "Pietermaritzburg";
                    break;

                case "Team Mdn":
                    teamCity = "Mandeni";
                    break;
            }


            // ==========================================
            // GET APPROVED EVENTS FOR THIS TEAM
            // ==========================================

            var events = db.Bookings
                .Where(b =>
                    b.City == teamCity &&
                    b.Status == "Approved" &&
                    b.EventDate >= DateTime.Today)
                .OrderBy(b => b.EventDate)
                .ToList();


            // ==========================================
            // STAFF INFORMATION FOR VIEW
            // ==========================================

            ViewBag.StaffName =
                staff.staff_FName + " " + staff.staff_LName;

            ViewBag.StaffType = staff.staff_Type;

            ViewBag.TeamCity = teamCity;


            return View(events);
        }



        [HttpGet]
        public ActionResult StaffEventDetails(int id)
        {
            // ==========================================
            // MAKE SURE STAFF MEMBER IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            // ==========================================
            // GET LOGGED-IN STAFF MEMBER
            // ==========================================

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // DETERMINE STAFF TEAM CITY
            // ==========================================

            string teamCity = "";

            switch (staff.staff_Type)
            {
                case "Team Dbn":
                    teamCity = "Durban";
                    break;

                case "Team Peter":
                    teamCity = "Pietermaritzburg";
                    break;

                case "Team Mdn":
                    teamCity = "Mandeni";
                    break;
            }


            // ==========================================
            // GET BOOKING
            //
            // IMPORTANT:
            // The booking must:
            // 1. Match the requested ID
            // 2. Belong to the staff member's city
            // 3. Be approved
            // ==========================================

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.City == teamCity &&
                    b.Status == "Approved");

            if (booking == null)
            {
                TempData["EventError"] =
                    "The event could not be found or you do not have access to it.";

                return RedirectToAction("StaffEvents");
            }


            // ==========================================
            // STAFF INFORMATION FOR VIEW
            // ==========================================

            ViewBag.StaffName =
                staff.staff_FName + " " + staff.staff_LName;

            ViewBag.StaffType = staff.staff_Type;

            ViewBag.TeamCity = teamCity;


            return View(booking);
        }



        [HttpGet]
        public ActionResult AssignStaffTask(int bookingId)
        {
            // ==========================================
            // GET APPROVED BOOKING
            // ==========================================

            var booking = db.Bookings.FirstOrDefault(b =>
                b.BookingId == bookingId &&
                b.Status == "Approved");

            if (booking == null)
            {
                TempData["TaskError"] =
                    "The approved booking could not be found.";

                return RedirectToAction("AllBookings");
            }


            // ==========================================
            // DETERMINE TEAM FROM BOOKING CITY
            // ==========================================

            string staffType = "";

            switch (booking.City)
            {
                case "Durban":
                    staffType = "Team Dbn";
                    break;

                case "Pietermaritzburg":
                    staffType = "Team Peter";
                    break;

                case "Mandeni":
                    staffType = "Team Mdn";
                    break;
            }


            // ==========================================
            // MAKE SURE CITY IS SUPPORTED
            // ==========================================

            if (string.IsNullOrWhiteSpace(staffType))
            {
                TempData["TaskError"] =
                    "No staff team is configured for this booking city.";

                return RedirectToAction("AllBookings");
            }


            // ==========================================
            // GET STAFF FROM CORRECT TEAM
            // ==========================================

            var staffMembers = db.Staffs
                .Where(s => s.staff_Type == staffType)
                .OrderBy(s => s.staff_FName)
                .ThenBy(s => s.staff_LName)
                .ToList();


            // ==========================================
            // CREATE STAFF DROPDOWN
            // ==========================================

            var staffOptions = staffMembers
                .Select(s => new
                {
                    StaffId = s.staff_ID,

                    DisplayName =
                        s.staff_FName + " " +
                        s.staff_LName + " - " +
                        s.staff_Email
                })
                .ToList();


            // ==========================================
            // SEND INFORMATION TO VIEW
            // ==========================================

            // ==========================================
            // GET TASKS ALREADY ASSIGNED TO THIS EVENT
            // ==========================================

            var existingTasks = db.StaffTasks
                .Include("Staff")
                .Where(t => t.BookingId == booking.BookingId)
                .OrderBy(t => t.DueDate)
                .ToList();


            // ==========================================
            // SEND INFORMATION TO VIEW
            // ==========================================

            ViewBag.Booking = booking;
            ViewBag.TeamType = staffType;
            ViewBag.TeamCity = booking.City;

            ViewBag.StaffMembers = new SelectList(
                staffOptions,
                "StaffId",
                "DisplayName"
            );

            ViewBag.ExistingTasks = existingTasks;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignStaffTask(
    int bookingId,
    int staffId,
    string taskName,
    string description,
    DateTime dueDate,
    string priority)
        {
            // ==========================================
            // GET APPROVED BOOKING
            // ==========================================

            var booking = db.Bookings.FirstOrDefault(b =>
                b.BookingId == bookingId &&
                b.Status == "Approved");

            if (booking == null)
            {
                TempData["TaskError"] =
                    "The approved booking could not be found.";

                return RedirectToAction("AllBookings");
            }


            // ==========================================
            // DETERMINE REQUIRED STAFF TEAM
            // ==========================================

            string requiredStaffType = "";

            switch (booking.City)
            {
                case "Durban":
                    requiredStaffType = "Team Dbn";
                    break;

                case "Pietermaritzburg":
                    requiredStaffType = "Team Peter";
                    break;

                case "Mandeni":
                    requiredStaffType = "Team Mdn";
                    break;
            }


            if (string.IsNullOrWhiteSpace(requiredStaffType))
            {
                TempData["TaskError"] =
                    "No staff team is configured for this booking city.";

                return RedirectToAction(
                    "AssignStaffTask",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // VALIDATE STAFF MEMBER
            //
            // Staff must belong to the booking's team.
            // ==========================================

            var staff = db.Staffs.FirstOrDefault(s =>
                s.staff_ID == staffId &&
                s.staff_Type == requiredStaffType);

            if (staff == null)
            {
                TempData["TaskError"] =
                    "The selected staff member does not belong to this event's team.";

                return RedirectToAction(
                    "AssignStaffTask",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // VALIDATE TASK NAME
            // ==========================================

            if (string.IsNullOrWhiteSpace(taskName))
            {
                TempData["TaskError"] =
                    "Please enter a task name.";

                return RedirectToAction(
                    "AssignStaffTask",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // VALIDATE PRIORITY
            // ==========================================

            var allowedPriorities = new[]
            {
        "Low",
        "Medium",
        "High"
    };

            if (!allowedPriorities.Contains(priority))
            {
                TempData["TaskError"] =
                    "Please select a valid priority.";

                return RedirectToAction(
                    "AssignStaffTask",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // CREATE STAFF TASK
            // ==========================================

            var task = new StaffTask
            {
                StaffId = staff.staff_ID,

                BookingId = booking.BookingId,

                TaskName = taskName.Trim(),

                Description = string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim(),

                DueDate = dueDate,

                Priority = priority,

                Status = "Pending",

                CreatedAt = DateTime.Now
            };


            db.StaffTasks.Add(task);

            db.SaveChanges();


            // ==========================================
            // SUCCESS
            // ==========================================

            TempData["TaskSuccess"] =
                "Task successfully assigned to " +
                staff.staff_FName + " " +
                staff.staff_LName + ".";


            return RedirectToAction(
                "AssignStaffTask",
                new { bookingId = bookingId }
            );
        }


     


        // ==============================================
        // STAFF - REPORT EVENT ISSUE PAGE
        // ==============================================

        [HttpGet]
        public ActionResult StaffComplaints(int? bookingId)
        {
            // ==========================================
            // MAKE SURE STAFF IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // GET LOGGED-IN STAFF MEMBER
            // ==========================================

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // DETERMINE STAFF TEAM CITY
            // ==========================================

            string teamCity = "";

            switch (staff.staff_Type)
            {
                case "Team Dbn":
                    teamCity = "Durban";
                    break;

                case "Team Peter":
                    teamCity = "Pietermaritzburg";
                    break;

                case "Team Mdn":
                    teamCity = "Mandeni";
                    break;
            }


            // ==========================================
            // GET APPROVED EVENTS FOR STAFF'S TEAM
            // ==========================================

            var teamEvents = db.Bookings
                .Where(b =>
                    b.City == teamCity &&
                    b.Status == "Approved")
                .OrderBy(b => b.EventDate)
                .ToList();


            // ==========================================
            // CREATE EVENT DROPDOWN OPTIONS
            // ==========================================

            var eventOptions = teamEvents
                .Select(b => new
                {
                    BookingId = b.BookingId,

                    DisplayName =
                        b.Occasion +
                        " - " +
                        b.EventDate.ToString("dd MMM yyyy")
                })
                .ToList();


            // ==========================================
            // GET THIS STAFF MEMBER'S REPORTED ISSUES
            // ==========================================

            var myComplaints = db.StaffComplaints
                .Include("Booking")
                .Where(c => c.StaffId == staffId)
                .OrderBy(c => c.Status == "Resolved")
                .ThenByDescending(c => c.CreatedAt)
                .ToList();


            // ==========================================
            // SEND DATA TO VIEW
            // ==========================================

            ViewBag.EventOptions = new SelectList(
                eventOptions,
                "BookingId",
                "DisplayName",
                bookingId
            );

            ViewBag.StaffName =
                staff.staff_FName + " " +
                staff.staff_LName;

            ViewBag.TeamCity = teamCity;

            ViewBag.SelectedBookingId = bookingId;

            ViewBag.MyComplaints = myComplaints;


            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffComplaints(
    int bookingId,
    string subject,
    string description,
    string priority)
        {
            // ==========================================
            // MAKE SURE STAFF IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // GET LOGGED-IN STAFF MEMBER
            // ==========================================

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // DETERMINE STAFF TEAM CITY
            // ==========================================

            string teamCity = "";

            switch (staff.staff_Type)
            {
                case "Team Dbn":
                    teamCity = "Durban";
                    break;

                case "Team Peter":
                    teamCity = "Pietermaritzburg";
                    break;

                case "Team Mdn":
                    teamCity = "Mandeni";
                    break;
            }


            // ==========================================
            // SECURITY CHECK - VERIFY EVENT
            // ==========================================

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.City == teamCity &&
                    b.Status == "Approved");

            if (booking == null)
            {
                TempData["ComplaintError"] =
                    "The selected event could not be found or does not belong to your team.";

                return RedirectToAction("StaffComplaints");
            }


            // ==========================================
            // VALIDATE SUBJECT
            // ==========================================

            if (string.IsNullOrWhiteSpace(subject))
            {
                TempData["ComplaintError"] =
                    "Please enter an issue subject.";

                return RedirectToAction(
                    "StaffComplaints",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // VALIDATE DESCRIPTION
            // ==========================================

            if (string.IsNullOrWhiteSpace(description))
            {
                TempData["ComplaintError"] =
                    "Please describe the issue.";

                return RedirectToAction(
                    "StaffComplaints",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // VALIDATE PRIORITY
            // ==========================================

            var allowedPriorities = new[]
            {
        "Low",
        "Medium",
        "High"
    };

            if (!allowedPriorities.Contains(priority))
            {
                TempData["ComplaintError"] =
                    "Please select a valid priority.";

                return RedirectToAction(
                    "StaffComplaints",
                    new { bookingId = bookingId }
                );
            }


            // ==========================================
            // CREATE COMPLAINT
            // ==========================================

            var complaint = new StaffComplaint
            {
                StaffId = staffId,
                BookingId = booking.BookingId,

                Subject = subject.Trim(),
                Description = description.Trim(),

                Priority = priority,

                Status = "Open",

                CreatedAt = DateTime.Now,

                ResolvedAt = null
            };


            db.StaffComplaints.Add(complaint);

            db.SaveChanges();


            // ==========================================
            // SUCCESS
            // ==========================================

            TempData["ComplaintSuccess"] =
                "The event issue has been reported successfully.";

            return RedirectToAction(
                "StaffComplaints",
                new { bookingId = bookingId }
            );
        }


        // ==============================================
        // STAFF CLOCK IN / OUT PAGE
        // ==============================================

        [HttpGet]
        public ActionResult StaffClockInOut()
        {
            // ==========================================
            // MAKE SURE STAFF IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // GET LOGGED-IN STAFF MEMBER
            // ==========================================

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // CHECK IF STAFF IS CURRENTLY CLOCKED IN
            // ==========================================

            var activeEntry = db.StaffTimeEntries
                .FirstOrDefault(t =>
                    t.StaffId == staffId &&
                    t.ClockOutTime == null);


            // ==========================================
            // GET TODAY'S TIME ENTRIES
            // ==========================================

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            var todayEntries = db.StaffTimeEntries
                .Where(t =>
                    t.StaffId == staffId &&
                    t.ClockInTime >= today &&
                    t.ClockInTime < tomorrow)
                .OrderByDescending(t => t.ClockInTime)
                .ToList();


            // ==========================================
            // CALCULATE TODAY'S COMPLETED HOURS
            // ==========================================

            double todayHours = todayEntries
                .Where(t => t.HoursWorked.HasValue)
                .Sum(t => t.HoursWorked ?? 0);


            // ==========================================
            // SEND DATA TO VIEW
            // ==========================================

            ViewBag.StaffName =
                staff.staff_FName + " " +
                staff.staff_LName;

            ViewBag.StaffType = staff.staff_Type;

            ViewBag.IsClockedIn = activeEntry != null;

            ViewBag.ActiveEntry = activeEntry;

            ViewBag.TodayEntries = todayEntries;

            ViewBag.TodayHours = todayHours;


            return View();
        }


        // ==============================================
        // STAFF CLOCK IN
        // ==============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffClockIn()
        {
            // ==========================================
            // MAKE SURE STAFF IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }


            int staffId = (int)Session["StaffId"];


            // ==========================================
            // MAKE SURE STAFF STILL EXISTS
            // ==========================================

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // PREVENT MULTIPLE ACTIVE CLOCK-INS
            // ==========================================

            var activeEntry = db.StaffTimeEntries
                .FirstOrDefault(t =>
                    t.StaffId == staffId &&
                    t.ClockOutTime == null);

            if (activeEntry != null)
            {
                TempData["TimeError"] =
                    "You are already clocked in.";

                return RedirectToAction("StaffClockInOut");
            }


            // ==========================================
            // CREATE CLOCK-IN ENTRY
            // ==========================================

            var timeEntry = new StaffTimeEntry
            {
                StaffId = staffId,

                ClockInTime = DateTime.Now,

                ClockOutTime = null,

                HoursWorked = null
            };


            db.StaffTimeEntries.Add(timeEntry);

            db.SaveChanges();


            TempData["TimeSuccess"] =
                "You have successfully clocked in.";


            return RedirectToAction("StaffClockInOut");
        }


        // ==============================================
        // STAFF CLOCK OUT
        // ==============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffClockOut()
        {
            // ==========================================
            // MAKE SURE STAFF IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }


            int staffId = (int)Session["StaffId"];


            // ==========================================
            // FIND ACTIVE CLOCK-IN
            // ==========================================

            var activeEntry = db.StaffTimeEntries
                .Where(t =>
                    t.StaffId == staffId &&
                    t.ClockOutTime == null)
                .OrderByDescending(t => t.ClockInTime)
                .FirstOrDefault();


            if (activeEntry == null)
            {
                TempData["TimeError"] =
                    "You are not currently clocked in.";

                return RedirectToAction("StaffClockInOut");
            }


            // ==========================================
            // CLOCK OUT
            // ==========================================

            DateTime clockOutTime = DateTime.Now;

            activeEntry.ClockOutTime = clockOutTime;


            // ==========================================
            // CALCULATE HOURS WORKED
            // ==========================================

            TimeSpan workedTime =
                clockOutTime - activeEntry.ClockInTime;

            activeEntry.HoursWorked =
                Math.Round(workedTime.TotalHours, 2);


            db.SaveChanges();


            TempData["TimeSuccess"] =
                "You have successfully clocked out.";


            return RedirectToAction("StaffClockInOut");
        }

        // ==========================================
        // VIEW STAFF
        // ==========================================

        [HttpGet]
        public ActionResult ViewStaff()
        {
            if (Session["AdminId"] == null ||
                Session["AdminAuthenticated"] == null ||
                !(bool)Session["AdminAuthenticated"])
            {
                return RedirectToAction("Login", "Cust");
            }

            var staffMembers =
                db.Staffs
                  .OrderBy(s => s.staff_FName)
                  .ThenBy(s => s.staff_LName)
                  .ToList();

            return View(staffMembers);
        }



        [HttpGet]
        public ActionResult StaffLogin()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Please enter your email and password.");
                return View();
            }

            var staff = db.Staffs.FirstOrDefault(s =>
                s.staff_Email == email &&
                s.staff_Passw == password);

            if (staff == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid staff email or password."
                );

                return View();
            }

            // ==========================================
            // STAFF SESSION
            // ==========================================

            Session["StaffId"] = staff.staff_ID;

            Session["StaffFirstName"] = staff.staff_FName;

            Session["StaffLastName"] = staff.staff_LName;

            Session["StaffEmail"] = staff.staff_Email;

            Session["StaffType"] = staff.staff_Type;

            Session["StaffAuthenticated"] = true;


            // ==========================================
            // REDIRECT TO STAFF DASHBOARD
            // ==========================================

            return RedirectToAction(
                "StaffDashboard",
                "Cust"
            );
        }


        

        private string GetCityFromStaffType(string staffType)
        {
            if (string.IsNullOrWhiteSpace(staffType))
            {
                return "";
            }

            switch (staffType.ToLower())
            {
                case "team dbn":
                    return "Durban";

                case "team peter":
                    return "Pietermaritzburg";

                case "team mdn":
                    return "Mandeni";

                default:
                    return "";
            }
        }



        [HttpGet]
        public ActionResult StaffDashboard()
        {
            // Make sure a staff member is logged in
            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            // Get logged-in staff ID
            int staffId = (int)Session["StaffId"];

            // Get staff member
            var staff = db.Staffs.FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();
                return RedirectToAction("StaffLogin", "Cust");
            }

            // ==========================================
            // DETERMINE TEAM CITY
            // ==========================================

            string teamCity = "";

            switch (staff.staff_Type)
            {
                case "Team Dbn":
                    teamCity = "Durban";
                    break;

                case "Team Peter":
                    teamCity = "Pietermaritzburg";
                    break;

                case "Team Mdn":
                    teamCity = "Mandeni";
                    break;
            }

            // ==========================================
            // GET TODAY'S TASKS
            // ==========================================

            var today = DateTime.Today;

            // ==========================================
            // CALCULATE HOURS LOGGED TODAY
            // ==========================================

            DateTime tomorrow = today.AddDays(1);

            var todayTimeEntries = db.StaffTimeEntries
                .Where(t =>
                    t.StaffId == staffId &&
                    t.ClockInTime >= today &&
                    t.ClockInTime < tomorrow)
                .ToList();

            double hoursLogged = todayTimeEntries
                .Where(t => t.HoursWorked.HasValue)
                .Sum(t => t.HoursWorked ?? 0);


            var todayTasks = db.StaffTasks
                .Where(t =>
                    t.StaffId == staffId &&
                    DbFunctions.TruncateTime(t.DueDate) == today)
                .ToList();

            // ==========================================
            // HIGH PRIORITY TASKS
            // ==========================================

            int highPriorityTasks = todayTasks.Count(t =>
                t.Priority == "High");

            // ==========================================
            // EVENTS THIS WEEK
            // ==========================================

            DateTime weekStart = today;
            DateTime weekEnd = today.AddDays(7);

            var eventsThisWeek = db.Bookings
    .Where(b =>
        b.City == teamCity &&
        b.Status == "Approved" &&
        b.EventDate >= weekStart &&
        b.EventDate < weekEnd)
    .ToList();


            // ==========================================
            // OPEN STAFF COMPLAINTS
            // ==========================================

            int openComplaints = db.StaffComplaints
                .Count(c =>
                    c.StaffId == staffId &&
                    c.Status == "Open");


            // ==========================================
            // CREATE DASHBOARD VIEW MODEL
            // ==========================================

            var model = new StaffDashboardViewModel
            {
                StaffMember = staff,

                TeamCity = teamCity,

                TodayTasks = todayTasks.Count,

                HighPriorityTasks = highPriorityTasks,

                HoursLogged = hoursLogged,

                EventsThisWeek = eventsThisWeek.Count,

                OpenComplaints = openComplaints,

                Tasks = todayTasks,

                Events = eventsThisWeek.Select(b => new StaffEventDashboardItem
                {
                    BookingId = b.BookingId,
                    FirstName = b.FirstName,
                    LastName = b.LastName,
                    Occasion = b.Occasion,
                    EventDate = b.EventDate,
                    EventTime = b.EventTime,
                    Address = b.Address,
                    City = b.City,
                    Status = b.Status
                }).ToList()
            };

            return View(model);
        }


        // ==============================================
        // STAFF PROFILE
        // ==============================================

        [HttpGet]
        public ActionResult StaffProfile()
        {
            // ==========================================
            // MAKE SURE STAFF IS LOGGED IN
            // ==========================================

            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // GET LOGGED-IN STAFF MEMBER
            // ==========================================

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();

                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // DETERMINE TEAM CITY
            // ==========================================

            string teamCity = "";

            switch (staff.staff_Type)
            {
                case "Team Dbn":
                    teamCity = "Durban";
                    break;

                case "Team Peter":
                    teamCity = "Pietermaritzburg";
                    break;

                case "Team Mdn":
                    teamCity = "Mandeni";
                    break;
            }


            // ==========================================
            // GET STAFF STATISTICS
            // ==========================================

            int totalTasks = db.StaffTasks
                .Count(t => t.StaffId == staffId);

            int completedTasks = db.StaffTasks
                .Count(t =>
                    t.StaffId == staffId &&
                    t.Status == "Completed");

            int openComplaints = db.StaffComplaints
                .Count(c =>
                    c.StaffId == staffId &&
                    c.Status == "Open");

            double totalHours = db.StaffTimeEntries
                .Where(t =>
                    t.StaffId == staffId &&
                    t.HoursWorked.HasValue)
                .Select(t => t.HoursWorked ?? 0)
                .DefaultIfEmpty(0)
                .Sum();


            // ==========================================
            // SEND INFORMATION TO VIEW
            // ==========================================

            ViewBag.TeamCity = teamCity;

            ViewBag.TotalTasks = totalTasks;

            ViewBag.CompletedTasks = completedTasks;

            ViewBag.OpenComplaints = openComplaints;

            ViewBag.TotalHours = totalHours;


            return View(staff);
        }


        // ==============================================
        // STAFF - EDIT PROFILE PAGE
        // ==============================================

        [HttpGet]
        public ActionResult EditStaffProfile()
        {
            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();
                return RedirectToAction("StaffLogin", "Cust");
            }

            return View(staff);
        }


        // ==============================================
        // STAFF - SAVE PROFILE CHANGES
        // ==============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStaffProfile(
            string firstName,
            string lastName,
            string email,
            string phone)
        {
            if (Session["StaffAuthenticated"] == null ||
                !(bool)Session["StaffAuthenticated"])
            {
                return RedirectToAction("StaffLogin", "Cust");
            }

            int staffId = (int)Session["StaffId"];

            var staff = db.Staffs
                .FirstOrDefault(s => s.staff_ID == staffId);

            if (staff == null)
            {
                Session.Clear();
                return RedirectToAction("StaffLogin", "Cust");
            }


            // ==========================================
            // CLEAN INPUT
            // ==========================================

            firstName = (firstName ?? "").Trim();
            lastName = (lastName ?? "").Trim();
            email = (email ?? "").Trim();
            phone = (phone ?? "").Trim();


            // ==========================================
            // REQUIRED FIELDS
            // ==========================================

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone))
            {
                TempData["ProfileError"] =
                    "Please complete all profile fields.";

                return RedirectToAction("EditStaffProfile");
            }


            // ==========================================
            // VALIDATE EMAIL
            // ==========================================

            try
            {
                var emailAddress =
                    new System.Net.Mail.MailAddress(email);

                if (emailAddress.Address != email)
                {
                    throw new FormatException();
                }
            }
            catch
            {
                TempData["ProfileError"] =
                    "Please enter a valid email address.";

                return RedirectToAction("EditStaffProfile");
            }


            // ==========================================
            // VALIDATE PHONE
            // ==========================================

            if (phone.Length != 10 ||
     !phone.All(char.IsDigit) ||
     !phone.StartsWith("0"))
            {
                TempData["ProfileError"] =
                    "Phone number must contain exactly 10 digits and start with 0.";

                return RedirectToAction("EditStaffProfile");
            }


            // ==========================================
            // CHECK EMAIL IS NOT USED BY ANOTHER STAFF
            // ==========================================

            bool emailExists = db.Staffs.Any(s =>
                s.staff_Email == email &&
                s.staff_ID != staffId);

            if (emailExists)
            {
                TempData["ProfileError"] =
                    "That email address is already being used by another staff member.";

                return RedirectToAction("EditStaffProfile");
            }


            // ==========================================
            // UPDATE PROFILE
            // ==========================================

            staff.staff_FName = firstName;
            staff.staff_LName = lastName;
            staff.staff_Email = email;
            staff.staff_Phone = phone;

            db.SaveChanges();


            // ==========================================
            // UPDATE SESSION VALUES
            // ==========================================

            Session["StaffFirstName"] = staff.staff_FName;
            Session["StaffLastName"] = staff.staff_LName;
            Session["StaffEmail"] = staff.staff_Email;


            TempData["ProfileSuccess"] =
                "Your profile has been updated successfully.";

            return RedirectToAction("StaffProfile");
        }



        [HttpGet]
        public ActionResult StaffLogout()
        {
            Session.Remove("StaffId");
            Session.Remove("StaffFirstName");
            Session.Remove("StaffLastName");
            Session.Remove("StaffEmail");
            Session.Remove("StaffType");
            Session.Remove("StaffAuthenticated");

            return RedirectToAction(
                "StaffLogin",
                "Cust"
            );
        }



        // ===============================
        // ADMIN LOGIN - GET
        // ===============================

        [HttpGet]
        public ActionResult AdminLogin()
        {
            return RedirectToAction("Login", "Cust");
        }


        // ===============================
        // ADMIN LOGIN - POST
        // ===============================

        
        

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


    }
}