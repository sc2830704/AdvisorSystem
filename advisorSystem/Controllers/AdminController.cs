using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using advisorSystem.Models;
using advisorSystem.lib;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace advisorSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private string userId;
        private string adminId;
        private JObject adminInfo;
        CommonSQL commonSQL = new CommonSQL();
        AdminHelper adminHelper = new AdminHelper();

        public AdminController()
        {
        }


        private void getRoleInfo()
        {
            userId = User.Identity.GetUserId();
            adminInfo = adminHelper.getAdminInfo(userId);
            adminInfo["chinesDepart"] = translateDepartment((int)adminInfo["st_department"]);
            adminId = (string)adminInfo["st_id"];
        }

        private string translateDepartment(int department)
        {
            string chineseDepartment;
            switch (department)
            {
                case 1:
                    chineseDepartment = "電子系";
                    break;
                case 2:
                    chineseDepartment = "設計系";
                    break;
                default:
                    chineseDepartment = "未知";
                    break;
            }
            return chineseDepartment;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Link your mssql.";
            ViewBag.Depart = "電子";
            ViewBag.User = "我";
            return View();
        }


        // POST: /admin/GetStudentSemester
        [HttpPost]
        public string GetStudentSemester()
        {
            getRoleInfo();
            
            JToken returnValue = adminHelper.getStudentSemester();

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }

        // POST: /admin/GetStudentList
        [HttpPost]
        public string GetStudentList()
        {
            getRoleInfo();
            
            JToken returnValue = adminHelper.getStudentList();

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }


        // POST: /admin/GetStudentList
        [HttpPost]
        public string GetStudentInfo()
        {
            getRoleInfo();

            JObject returnValue = adminHelper.getStudentInfo(Request.Form["sid"]);

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }

        

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #region Helper
        // 新增外部登入時用來當做 XSRF 保護
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");

        }
        #endregion
    }
}