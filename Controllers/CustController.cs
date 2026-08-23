using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CustController : Controller
    {
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
    }
}