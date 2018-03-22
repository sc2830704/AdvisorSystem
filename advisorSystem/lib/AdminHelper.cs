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

        private string st_id;
        JObject st_info = new JObject();
        StudentSQL studentSQL = new StudentSQL();

        public AdminHelper()
        {
            sqlHelper = new SQLHelper();
        }

        public JObject getAdminInfo(string userId)
        {
            JObject condi = new JObject();
            condi["st_u_id"] = userId;
            JObject returnValue = sqlHelper.select("[ntust].[staff]", condi);

            if ((bool)returnValue["status"])
            {
                foreach (JToken jt in returnValue["data"])
                {

                    st_info = (JObject)jt;
                    st_id = (string)st_info["st_id"];
                }
                return st_info;

            }
            else
            {
                //String msg = returnValue["msg"].ToString();

                return st_info;
            }
        }

        public JToken getStudentSemester()
        {
            JArray returnJA = new JArray();
            JObject condi = new JObject();
            JObject returnValue = new JObject();

            string st_department = (string)st_info["st_department"];

            //condi["s_department"] = st_department;


            returnValue = sqlHelper.select("(SELECT SUBSTRING(s.s_id, 1, 4) as sid_short from [ntust].[student] s" +
                                                                " JOIN [ntust].[pair] p on s.s_id=p.p_s_id AND p.p_end_date IS NULL" +
                                                                " JOIN [ntust].[teacher_group] tg on tg.tg_id=p.p_tg_id" +
                                                                " JOIN [ntust].[teacher] t on tg.t_id=t.t_id" +
                                                                " WHERE t.t_department=" + st_department + ") as ss", condi
                                    , "GROUP BY ss.sid_short", "ss.sid_short");
            if ((bool)returnValue["status"])
            {
                foreach (JToken jt in returnValue["data"])
                {
                    returnJA.Add(((JObject)jt)["sid_short"]);
                }
                return returnJA;
            }
            else
            {
                return returnJA;
            }

        }

        public JToken getStudentList()
        {
            JToken semesterList = getStudentSemester();
            JToken returnJT = new JArray();
            JObject condi = new JObject();
            JObject returnValue = new JObject();
            JObject studentJO;
            //JObject returnValue2 = new JObject();

            /*student_state_event sse_id
            sse_event
            sse_datetime*/
            string st_department = (string)st_info["st_department"];
            string s_id;
            condi["t_department"] = st_department;

            returnValue = sqlHelper.select("[ntust].[teacher]", condi
                                    , select: "t_id, t_name tname");
            if (!(bool)returnValue["status"])
            {
                return returnJT;

            }
            returnJT = returnValue["data"];
            foreach (JObject jt in returnJT)
            {
                foreach (string semester in semesterList)
                {
                    jt[semester] = new JArray();
                }
                jt["phd"] = new JArray();
                jt["pt_m"] = new JArray();
                jt["history"] = new JArray();

                condi = new JObject();
                condi["t.t_id"] = jt["t_id"];
                returnValue = sqlHelper.select("[ntust].[teacher] t" +
                            " LEFT JOIN ntust.teacher_group tg on tg.t_id=t.t_id" +
                            " JOIN [ntust].[pair] p on tg.tg_id=p.p_tg_id AND p.p_end_date IS NULL" +
                            " JOIN [ntust].[student] s on s.s_id=p.p_s_id" +
                            " LEFT JOIN (SELECT max(sse_event) now_status, sse_s_id FROM [ntust].[student_state_event] GROUP BY sse_s_id) sse on sse.sse_s_id=s.s_id", condi
                                    , select: "sse.now_status, s.s_id");
                if ((bool)returnValue["status"])
                {
                    foreach (JObject jo in returnValue["data"])
                    {
                        studentJO = new JObject();
                        s_id = (string)jo["s_id"];
                        studentJO["sid"] = s_id;
                        studentJO["status"] = jo["now_status"];
                        if (s_id.Substring(0, 1) == "D")
                        {
                            ((JArray)jt["phd"]).Add(studentJO);
                            continue;
                        }
                        if (s_id.Substring(6, 1) == "9")
                        {
                            ((JArray)jt["pt_m"]).Add(studentJO);
                            continue;
                        }
                        ((JArray)jt[s_id.Substring(0, 4)]).Add(studentJO);
                    }
                }

                returnValue = sqlHelper.select("[ntust].[teacher] t" +
                            " LEFT JOIN ntust.teacher_group tg on tg.t_id=t.t_id" +
                            " JOIN [ntust].[pair] p on tg.tg_id=p.p_tg_id", condi
                                    , select: "p.p_end_date, p.p_end_date , p.p_s_id");
                if ((bool)returnValue["status"])
                {
                    foreach (JObject jo in returnValue["data"])
                    {

                    }
                }

            }
            return returnJT;


        }

        public JToken getStudentInfo(string s_id)
        {
            JToken returnJT = new JArray();
            JObject condi = new JObject();
            JObject returnValue = new JObject();

            //string st_department = (string)st_info["st_department"];

            //condi["s_department"] = st_department;
            condi["s.s_id"] = s_id;

            returnValue = sqlHelper.select("[ntust].[student] s" +
                                        " LEFT JOIN [ntust].[pair] p on s.s_id=p.p_s_id AND p.p_end_date IS NULL" +
                                        " LEFT JOIN [ntust].[teacher_group] tg on tg.tg_id=p.p_tg_id" +
                                        " LEFT JOIN [ntust].[teacher] t on tg.t_id=t.t_id", condi
                                    , "", "s.s_id sid, s.s_name sname, t.t_name tname");

            if (!(bool)returnValue["status"])
            {
                return returnJT;

            }
            returnJT = returnValue["data"];
            foreach (JObject jt in returnJT)
            {
                jt["apply"] = studentSQL.getApplyResultForAdmin(s_id);
                jt["change"] = studentSQL.getChangeResultForAdmin(s_id);


            }
            return returnJT;

        }




    }
}