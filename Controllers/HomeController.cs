using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace rqq_management_portal_asp_net.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return RedirectToAction("Login", "Home");
            return View();
            //this is to load the login page when user is logged out, and load the agent/admin home page when user is logged in
            //will come back to this later
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            ViewBag.Message = "Login page";

            return View();
        }
    }
}