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
    [Authorize(Roles = "teacher")]
    public class TeacherController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private string userId;
        private JObject teacherInfo;
        private String tid;
        TeacherHelper teacherHelper = new TeacherHelper();

        public TeacherController()
        {
        }

        private void getRoleInfo()
        {
            userId = User.Identity.GetUserId();
            teacherInfo = teacherHelper.getTeacherInfo(userId);
            tid = (string)teacherInfo["t_id"];
        }

        [Authorize(Roles = "teacher")]
        public ActionResult Index()
        {
            getRoleInfo();
            System.Diagnostics.Debug.Print("user id " + tid);
            //get teacher student
            JObject studnetlist = teacherHelper.GetStudent();
            ViewBag.tname = teacherInfo["t_name"];

            if ((bool)studnetlist["status"])
            {
                ViewBag.teacherStudent = studnetlist["data"].ToString(Formatting.None);
                return View();
            }
            else
            {
                String msg = studnetlist["msg"].ToString();
                return Content(msg);
            }  
        }
        [HttpGet]
        [Authorize(Roles = "teacher")]
        public String GetStudent()
        {
            getRoleInfo();
            JObject studnetlist = teacherHelper.GetStudent();
            if ((bool)studnetlist["status"])
                return studnetlist["data"].ToString(Formatting.None);
            else
                return studnetlist["msg"].ToString();
        }
        [HttpGet]
        [Authorize(Roles = "teacher")]
        public String GetStudentApply()
        {
            getRoleInfo();
            JObject apply = teacherHelper.GetApply();

            if ((bool)apply["status"])
                return apply["data"].ToString(Formatting.None);
            else
                return apply["msg"].ToString();
        }
        [Authorize(Roles = "teacher")]
        public String GetStudentChange()
        {
            getRoleInfo();
            JObject change = teacherHelper.GetChange();
            if ((bool)change["status"])
                return change["data"].ToString(Formatting.None);
            else
                return change["msg"].ToString();
        }
        [Authorize(Roles = "teacher")]
        public String UpdateStudentApply(String tg_id, String s_id, int accept)
        {
            getRoleInfo();
            JObject update_apply = teacherHelper.UpdateApply(tg_id, accept);

            if (accept == 1)
            {
                if (teacherHelper.CheckAllApply(tg_id) == 0)
                {
                    //add new pair
                    teacherHelper.AddApplyPair(tg_id, s_id);
                    //update student apply history
                    teacherHelper.UpdateStudentApplyHistory(tg_id, state: 1); //state=1 means success
                                                                              //update student apply status
                    teacherHelper.UpdateStudentApplyStatus(s_id, state: 0); 
                }
            }
            else if(accept == 2)
            {
                teacherHelper.UpdateStudentApplyHistory(tg_id, 2); //state=2 means reject
                //update student apply status
                teacherHelper.UpdateStudentApplyStatus(s_id, 0); 
            }

            return update_apply["status"].ToString(Formatting.None);
        }
        public String UpdateStudentChange(String sc_id, String org_tg_id, String tg_id, String s_id, String t_id, String thesis_state, String sc_allapproval, int accept)
        {
            getRoleInfo();

            //update teacher accpet according to allapprove
            JObject update_change = teacherHelper.UpdateChange(org_tg_id, tg_id, s_id, t_id, thesis_state, sc_allapproval, accept);
            
            // to do
            if (accept == 1)
            {
                if (sc_allapproval.Equals("0") && teacherHelper.IsAllOrgTeacherApprove(org_tg_id))
                {
                    // then update student apply allapprove to 1
                    teacherHelper.UpdateStudentChangeApproval(tg_id);

                }
                if (sc_allapproval.Equals("1") && teacherHelper.IsAllTeacherApprove(tg_id, org_tg_id))
                {
                    String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //update original pair since it's expired
                    teacherHelper.UpdateOrgPair(org_tg_id, time);
                    //add new paired
                    teacherHelper.AddChangePair(tg_id, s_id, time);
                    //update student change history
                    teacherHelper.UpdateStudentChangeHistory(org_tg_id, 1, time); //state=1 means success
                                                                                  //update student apply status
                    teacherHelper.UpdateStudentApplyStatus(s_id, state: 0); //state=0 means success
                }
            }
            else if (accept == 2)
            {
                String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //update student change history
                teacherHelper.UpdateStudentChangeHistory(org_tg_id, 3, time); //state=3 means reject
                //update student apply status
                teacherHelper.UpdateStudentApplyStatus(s_id, state: 0);
            }
            
            return update_change["status"].ToString(Formatting.None);
            
        }
        public String GetApplyHistory()
        {
            getRoleInfo();
            JObject applyHistory = teacherHelper.GetApplyHistory();
            if ((bool)applyHistory["status"])
                return applyHistory["data"].ToString(Formatting.None);
            else
                return applyHistory["msg"].ToString();
        }
        public String GetChangeHistory()
        {
            getRoleInfo();
            JObject changeHistory = teacherHelper.GetChangeHistory();
            if ((bool)changeHistory["status"])
                return changeHistory["data"].ToString(Formatting.None);
            else
                return changeHistory["msg"].ToString();
        }
        public TeacherController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
        
    }
}
 