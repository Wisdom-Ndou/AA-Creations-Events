using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using static WebApplication1.Models.Bankingdetailsviewmodel;
using System.Data.Entity;
using System.Web.Helpers;

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
        public ActionResult Login(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Please enter your email address and password."
                );

                return View();
            }

            // We are only implementing customer login for now.
            if (role != "customer")
            {
                ModelState.AddModelError(
                    "",
                    "Admin login is not available yet."
                );

                return View();
            }

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

            // Store the authenticated customer
            // information in the server-side session.
            Session["CustomerId"] = customer.Cust_ID;
            Session["CustomerEmail"] = customer.Cust_Email;
            Session["CustomerFirstName"] = customer.Cust_FName;

            return RedirectToAction("Index", "Cust");
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



            obj.Cust_Passw = Crypto.HashPassword(obj.Cust_Passw);

            db.Customers.Add(obj);
            db.SaveChanges();

            TempData["RegistrationSuccess"] =
                "Your registration was successful. You can now sign in and start booking.";

            return RedirectToAction("Customerregister", "Cust");
        }

        public ActionResult Adminregister(Customer obj)
        {
            return View(obj);
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
    }
}