using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using static WebApplication1.Models.Bankingdetailsviewmodel;
using System.Data.Entity;

namespace WebApplication1.Controllers
{
    public class CustController : Controller
    {
        private readonly DatabaseContext db = new DatabaseContext();

        [HttpPost]
        // Handles GET requests (page load)
        [HttpGet]
        public ActionResult Bankingdetails()
        {
            var model = new Bankingdetailsviewmodel.BankingDetailsViewModel();
            return View(model);
        }

        // Handles POST requests (form submission)
        [HttpPost]
        public ActionResult Bankingdetails(Bankingdetailsviewmodel.BankingDetailsViewModel model)
        {
            // Handle form submission here (e.g., save payment details)
            return View(model);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Customer obj)
        {
            return View(obj);
        }

        public ActionResult Login(Customer obj)
        {
            return View(obj);
        }

        public ActionResult Customerregister(Customer obj)
        {
            return View(obj);
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
                var booking = new Booking
                {
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
            var bookings = db.Bookings
                .Include("Package")
                .Include("BookingAddOns.AddOn")
                .OrderByDescending(b => b.EventDate)
                .ToList();

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
    }
}