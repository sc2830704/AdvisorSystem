﻿using System;
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
        StudentSQL studentSQL = new StudentSQL();

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
            getRoleInfo();

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "請選擇", Disabled = false, Selected = true });
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

            JToken teacherList = commonSQL.getTeacherList();
            foreach (JToken jt in teacherList)
            {
                JObject tmpJO = (JObject)jt;
                tmpJO["chinesDepart"] = translateDepartment((int)tmpJO["t_department"]);
            }
            ViewBag.teacherList = teacherList.ToString(Formatting.None);
            //ViewBag.mainTeacherListItem = items;

            JToken outSideTeacherUnit = commonSQL.getOutSideTeacherUnit();
            ViewBag.outSideTeacherUnit = outSideTeacherUnit.ToString(Formatting.None);

            JToken ousSideTeacherList = commonSQL.getOusSideTeacherList();
            ViewBag.ousSideTeacherList = ousSideTeacherList.ToString(Formatting.None);

            ViewBag.adminInfo = adminInfo;
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

        // POST: /admin/StudentApply
        [HttpPost]
        public string StudentApply()
        {
            getRoleInfo();

            JObject condi = new JObject();
            condi["s_u_id"] = userId;
            JObject returnValue = studentSQL.studentApply(Request.Form["main"], JArray.Parse(Request.Form["sub"]), Request.Form["s_id"]);

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }

        // POST: /admin/StudentChange
        [HttpPost]
        public string StudentChange()
        {
            getRoleInfo();

            JObject condi = new JObject();
            condi["s_u_id"] = userId;
            JObject returnValue = studentSQL.studentChange(Request.Form["main"], JArray.Parse(Request.Form["sub"]), Request.Form["s_id"]);

            //returnValue.Add("main", Request.Form["main"]);
            //returnValue.Add("sub", Request.Form["sub"]);
            //returnValue.Add("success", true);

            return returnValue.ToString();
        }
        public bool UpdateMaxStudent(String tid, int max_student_num, String semester)
        {
            JObject returnValue = adminHelper.UpdateMaxStudent(tid, max_student_num, semester);
            if ((bool)returnValue["status"])
                return true;
            else
                return false;
        }
        [HttpPost]
        public string addNewExtraTeacher()
        {
            getRoleInfo();
            
            JObject condi = new JObject();
            condi["s_u_id"] = userId;
            JObject returnValue = adminHelper.addNewExtraTeacher(Request.Form["t_name"], Request.Form["t_email"], Request.Form["t_phone"], Request.Form["t_telephone"], Request.Form["t_service_units"]);
            
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
        public String UpdateStudentApply(String tg_id, String t_id, String s_id, int accept)
        {
            getRoleInfo();

            int max_student = adminHelper.GetMaxStudentNum(t_id, s_id);//取得老師可指導學生上限
            int current_student = adminHelper.GetCurrentStudentNum(t_id, s_id);//取得目前學生
            if (current_student >= max_student & accept==1)
                return "100";
            JObject update_apply = adminHelper.UpdateApply(tg_id, t_id, adminId, accept);

            if (accept == 1)
            {
                //檢查是否可以新增
                //新增
                if (adminHelper.CheckAllApply(tg_id) == 0)
                {
                    String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //add new pair
                    adminHelper.AddApplyPair(tg_id, s_id);
                    //update student apply history
                    adminHelper.UpdateStudentApplyHistory(tg_id, 1, time); //state=1 means success
                    //update student apply status
                    adminHelper.UpdateStudentApplyStatus(s_id, 0); 
                    //新增學生上限
                }
            }
            else if (accept == 2)
            {
                String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //update student apply history
                adminHelper.UpdateStudentApplyHistory(tg_id, 2, time); //state=2 means reject
                //update student apply status
                adminHelper.UpdateStudentApplyStatus(s_id, 0); 

            }
            //check if all teacher agree for application, 
            /* CheckAllApply get 0 means all student apply in accept */
            

            return update_apply["status"].ToString(Formatting.None);
            //if ((bool)change["status"])
            //    return change["status"].ToString(Formatting.None);
            //else
            //    return change["msg"].ToString();
        }
        public String UpdateStudentChange(String sc_id, String org_tg_id, String new_tg_id, String s_id, String t_id, String thesis_state, String sc_allapproval, int accept)
        {
            getRoleInfo();

            int max_student = adminHelper.GetMaxStudentNum(t_id, s_id);//取得老師可指導學生上限
            int current_student = adminHelper.GetCurrentStudentNum(t_id, s_id);//取得目前學生
            int i = adminHelper.IsNewTeacher(s_id, t_id);//如果是新老師則需要補正
            if (current_student >= max_student + i & accept == 1)
                return "100";
            //update teacher accpet according to allapprove
            JObject update_change = adminHelper.UpdateChange(sc_id, org_tg_id,s_id, t_id, adminId, thesis_state, sc_allapproval, accept);
            if (accept == 1)
            {
                /*check if all original teacher agree for change advisor*/
                // update allapproval to 1 if all org teacher approved
                if (sc_allapproval.Equals("0") && adminHelper.IsAllOrgTeacherApprove(org_tg_id))
                {
                    adminHelper.UpdateStudentChangeApproval(new_tg_id);
                }
                if (adminHelper.IsAllTeacherApprove(new_tg_id, org_tg_id))
                {
                    String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //update original pair since it's expired
                    adminHelper.UpdateOrgPair(org_tg_id, time);
                    //add new paired
                    adminHelper.AddChangePair(new_tg_id, s_id, time);
                    //update student change history
                    adminHelper.UpdateStudentChangeHistory(org_tg_id, 1, time); //state=1 means success
                    //update student apply status
                    adminHelper.UpdateStudentApplyStatus(s_id, state: 0); 
                }
            }
            else if (accept == 2)
            {
                String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //update student change history
                adminHelper.UpdateStudentChangeHistory(org_tg_id, 3, time); //state=3 means reject
                //update student apply status
                adminHelper.UpdateStudentApplyStatus(s_id, state: 0);
            }
            return update_change["status"].ToString(Formatting.None);
        }
        #endregion
    }
}