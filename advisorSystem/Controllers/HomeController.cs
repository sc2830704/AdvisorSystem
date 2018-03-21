using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace advisorSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Student()
        {
            ViewBag.Message = "Link your mssql.";

            return View();
        }

        public ActionResult Teacher()
        {
            ViewBag.Message = "Link your mssql.";

            return View();
        }

        public ActionResult Admin()
        {
            ViewBag.Message = "Link your mssql.";

            return View();
        }
    }
}