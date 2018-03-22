using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace advisorSystem.lib
{
    public class AdminHelper
    {
        
        SQLHelper sqlHelper;
        private string a_id;
        JObject t_info = new JObject();
        public AdminHelper()
        {
            sqlHelper = new SQLHelper();
        }
        public JObject getAdminInfo(string userId)
        {
            JObject condi = new JObject();
            condi["a_u_id"] = userId;
            JObject returnValue = sqlHelper.select("[ntust].[teacher]", condi);
            if ((bool)returnValue["status"])
            {
                foreach (JToken jt in returnValue["data"])
                {
                    t_info = (JObject)jt;
                    a_id = (string)t_info["a_id"];
                }
                return t_info;
            }
            else
            {
                //String msg = returnValue["msg"].ToString();
                return t_info;
            }
        }
        public JObject GetStudentSemester(string userId)
        {
            sqlHelper = new SQLHelper();
            String queryStr = "SELECT DISTINCT SUBSTRING(s.s_id, 0,5) AS sesmeter FROM ntust.student s WHERE s.s_department = 1";
            return sqlHelper.select(queryStr);
        }

        public JObject GetStudnetApply(string sid)
        {
            sqlHelper = new SQLHelper();
            String queryStr = "SELECT t.t_name,sa.sa_state, sa.sa_tg_id FROM ntust.student_apply AS sa "+
                                "JOIN ntust.teacher t on t.t_id = sa.sa_t_id "+
                                "WHERE sa.sa_s_id = '"+ sid +"'";
            return sqlHelper.select(queryStr);
        }
    }
}