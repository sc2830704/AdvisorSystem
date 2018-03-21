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
using System.Collections.Generic;

namespace advisorSystem.Controllers
{
    //[Authorize]
    [Authorize(Roles = "student")]
    public class StudentController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private string userId;
        private string studentId;
        private JObject studentInfo;
        StudentSQL studentSQL = new StudentSQL();
        CommonSQL commonSQL = new CommonSQL();

        public StudentController()
        {
        }

        private void getRoleInfo()
        {
            userId = User.Identity.GetUserId();
            studentInfo = studentSQL.getStudentInfo(userId);
            studentInfo["chinesDepart"] = translateDepartment((int)studentInfo["s_department"]);
            studentId = (string)studentInfo["s_id"];
        }

        private string translateDepartment( int department )
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
            getRoleInfo();

            ViewBag.userId = userId;
            ViewBag.studentId = studentId;
            ViewBag.studentInfo = studentInfo;

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "請選擇", Disabled = true, Selected = true });
            JToken departmentList = commonSQL.getDepartmentList();
            foreach (JToken jt in departmentList)
            {
                JObject tmpJO = (JObject)jt;
                tmpJO["chinesDepart"] = translateDepartment((int)tmpJO["t_department"]);
                items.Add(new SelectListItem { Text = (string)tmpJO["chinesDepart"], Value = (string)tmpJO["t_department"] });
            }
            items.Add(new SelectListItem { Text = "校外", Value = "0" });
            ViewBag.departmentList = departmentList.ToString(Formatting.None);
            ViewBag.departmentListItem = items;

            items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "選擇指導教授*", Disabled = true, Selected = true });
            JToken teacherList = commonSQL.getTeacherList();
            foreach (JToken jt in teacherList)
            {
                JObject tmpJO = (JObject)jt;
                tmpJO["chinesDepart"] = translateDepartment((int)tmpJO["t_department"]);
                if ((int)tmpJO["t_department"]== (int)studentInfo["s_department"]&& (int)tmpJO["t_group"] == (int)studentInfo["s_group"]) {
                    items.Add(new SelectListItem { Text = (string)tmpJO["t_name"], Value = (string)tmpJO["t_id"] });
                }
            }
            ViewBag.teacherList = teacherList.ToString(Formatting.None);
            ViewBag.mainTeacherListItem = items;

            JToken outSideTeacherUnit = commonSQL.getOutSideTeacherUnit();
            ViewBag.outSideTeacherUnit = outSideTeacherUnit.ToString(Formatting.None);
            
            JToken ousSideTeacherList = commonSQL.getOusSideTeacherList();
            ViewBag.ousSideTeacherList = ousSideTeacherList.ToString(Formatting.None);



            bool isNew = studentSQL.checkStudentStatusIsNew();
            if (isNew)
            {
                JToken applyResult = studentSQL.getApplyResult();
                ViewBag.applyResult = applyResult.ToString(Formatting.None);
                return View("new");
            }
            else
            {
                JToken changeResult = studentSQL.getChangeResult();
                ViewBag.changeResult = changeResult.ToString(Formatting.None);

                JToken changeHistory = studentSQL.getChangeHistory();
                ViewBag.changeHistory = changeHistory.ToString(Formatting.None);

                JToken pairTeacher = studentSQL.getPairTeacher();
                foreach (JToken jt in pairTeacher)
                {
                    JObject tmpJO = (JObject)jt;
                    tmpJO["chinesDepart"] = translateDepartment((int)tmpJO["t_department"]);
                }
                ViewBag.pairTeacher = pairTeacher.ToString(Formatting.None);

                return View();
            }
            
        }

        // POST: /Student/StudentApply
        [HttpPost]
        public string StudentApply()
        {
            getRoleInfo();

            JObject condi = new JObject();
            condi["s_u_id"] = userId;
            JObject returnValue = studentSQL.studentApply(Request.Form["main"], JArray.Parse(Request.Form["sub"]));

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }

        // POST: /Student/StudentChange
        [HttpPost]
        public string StudentChange()
        {
            getRoleInfo();

            JObject condi = new JObject();
            condi["s_u_id"] = userId;
            JObject returnValue = studentSQL.studentChange(Request.Form["main"], JArray.Parse(Request.Form["sub"]));

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }
        

        public ActionResult testInsert(string returnUrl)
        {

            JObject bbb = new JObject();
            SQLHelper sqlHelper = new SQLHelper();

            bbb["s_id"] = "123456789";
            bbb["s_name"] = "123456789";
            bbb["s_department"] = 1;
            bbb["s_group"] = 1;
            bbb["s_state"] = 1;
            JObject returnValue = sqlHelper.insert("[ntust].[student]", bbb);
            if ((bool)returnValue["status"])
            {
                return new HttpStatusCodeResult(200);
            }
            else
            {
                String msg = returnValue["msg"].ToString();
                /*switch (msg) {
                    case "a":


                }*/
                return Content(msg);

                //return new HttpStatusCodeResult(404);
            }

            //ViewBag.ReturnUrl = returnUrl;

        }

        public ActionResult testSelect(string returnUrl)
        {

            JObject bbb = new JObject();
            SQLHelper sqlHelper = new SQLHelper();

            bbb["s_id"] = "123456789";
            bbb["s_name"] = "123456789";

            JObject returnValue = sqlHelper.select("[ntust].[student]", bbb);
            if ((bool)returnValue["status"])
            {
                return Content(returnValue["data"].ToString());
            }
            else
            {
                String msg = returnValue["msg"].ToString();
                /*switch (msg) {
                    case "a":


                }*/
                return Content(msg);

            }

        }

        public ActionResult testDelete(string returnUrl)
        {

            JObject bbb = new JObject();
            SQLHelper sqlHelper = new SQLHelper();

            bbb["s_id"] = "123456789";

            JObject returnValue = sqlHelper.delete("[ntust].[student]", bbb);
            if ((bool)returnValue["status"])
            {
                return Content("delete success");
                //return new HttpStatusCodeResult(200);
            }
            else
            {
                String msg = returnValue["msg"].ToString();

                return Content(msg);

                //return new HttpStatusCodeResult(404);
            }

        }

        public StudentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 要求重新導向至外部登入提供者
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // 產生並傳送 Token
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // 若使用者已經有登入資料，請使用此外部登入提供者登入使用者
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // 若使用者沒有帳戶，請提示使用者建立帳戶
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // 從外部登入提供者處取得使用者資訊
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
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
            //main: $("#mainTeacherListItem").val() ,
            //sub: subTeacher
            return Content(Request["main"].ToString());

            //return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}