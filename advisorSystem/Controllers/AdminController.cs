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
        private AdminHelper adminHelper = new AdminHelper();
        private string userId;
        private JObject AdminInfo;
        private String uname;
        public AdminController()
            
        {
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Link your mssql.";
            ViewBag.Depart = "電子";
            ViewBag.User = "我";
            return View();
        }
        public String GetStudentSemester()
        {
            JArray arr = new JArray();
            arr.Add("M104");
            arr.Add("M105");
            return arr.ToString(Formatting.None);
        }
        public String GetStudentList()
        {
            JArray arr = new JArray();
            JObject obj = new JObject();
            JArray hisory = new JArray();
            JObject hisoryItem = new JObject();
            JArray phd = new JArray();
            JArray m105 = new JArray();
            JArray m104 = new JArray();
            JObject s1 = new JObject();
            s1.Add("status", 1);
            s1.Add("sid", "D10402158");
            phd.Add(s1);

            obj.Add("tname", "jsleu");
            obj.Add("phd", phd);
            s1["sid"] = "M10402158";
            m105.Add(s1);
            obj.Add("M105", m105);
            s1["sid"] = "M10402858";
            m104.Add(s1);
            obj.Add("M104", m104);
            obj.Add("pt_m", m105);
            hisoryItem.Add("hsc_s_id", "M104XXXXX");
            hisoryItem.Add("hsc_endtime", "2018-03-01");
            hisoryItem.Add("in_or_out", "轉出");
            hisory.Add(hisoryItem);
            obj.Add("history", hisory);
            arr.Add(obj);
            return arr.ToString(Formatting.None);
        }
        public String GetStudentInfo()
        {
            JArray arr = new JArray();
            JObject obj = new JObject();
            JArray apply = new JArray();
            JObject change = new JObject();
            JArray old = new JArray();
            JArray news = new JArray();
            JObject a = new JObject();
            a.Add("tg_id", 10);a.Add("status", 1);a.Add("tname", "jsleu");
            JObject b = new JObject();
            b.Add("sc_id", 2); b.Add("status", 1); b.Add("tname", "jaja");
            JObject c = new JObject();
            c.Add("sa_id", 1); c.Add("status", 1); c.Add("tname", "呂"); c.Add("tid", "jsleu");
            
            old.Add(a);
            news.Add(b);
            apply.Add(c);
            change.Add("new", news);
            change.Add("old", old);
            obj.Add("sid", "M10502001");
            obj.Add("sname", "小名");
            obj.Add("tname", "呂");
            obj.Add("apply", apply);
            obj.Add("change", change);
            arr.Add(obj);
            return obj.ToString(Formatting.None);
            
        }
        public String GetStudnetApply(String sid)
        {
            JObject studentApply = adminHelper.GetStudnetApply(sid);
            
            return studentApply["data"].ToString(Formatting.None);
        }
        public String UpdateStudentChange(String sc_id, String org_tg_id, String s_id, String t_id, String thesis_state, String sc_allapproval, int accept)
        {

            ////update teacher accpet according to allapprove
            //JObject update_change = adminHelper.UpdateChange(sc_id, s_id, t_id, thesis_state, sc_allapproval, accept);

            //System.Diagnostics.Debug.Print((String)update_change["status"]);

            ///*check if all original teacher agree for change advisor*/
            //// if they all agree then add new pair 
            //if (sc_allapproval.Equals("0") && adminHelper.CheckOrgChange(sc_id) == 0)
            //{
            //    // CheckAllApply get 0 means all student_apply is accepted
            //    // then update student apply allapprove to 1
            //    adminHelper.UpdateStudentChangeApproval(sc_id);

            //}
            //else if (sc_allapproval.Equals("1") && teacherHelper.CheckNewChange(sc_id) == 0)
            //{
            //    //remove original pair
            //    adminHelper.removePair(s_id);
            //    //add new paired
            //    adminHelper.AddChangePair(sc_id, s_id);
            //    //update student change history
            //    adminHelper.UpdateStudentChangeHistory(org_tg_id, state: 1); //state=1 means success
            //}
            ////todo - remove student_change - another query to check is all change checked

            //return update_change["status"].ToString(Formatting.None);
            return null;
        }


    }
}