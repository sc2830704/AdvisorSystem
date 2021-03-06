﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            //都LEFT JOIN表示 系所上未有老師的學生也要選擇
            returnValue = sqlHelper.select("(SELECT SUBSTRING(s.s_id, 1, 4) as sid_short from [ntust].[student] s" +
                                                                " LEFT JOIN [ntust].[pair] p on s.s_id=p.p_s_id AND p.p_end_date IS NULL" +
                                                                " LEFT JOIN [ntust].[teacher_group] tg on tg.tg_id=p.p_tg_id" +
                                                                " LEFT JOIN [ntust].[teacher] t on tg.t_id=t.t_id" +
                                                                " WHERE t.t_department=" + st_department + " OR s.s_department=" + st_department + ") as ss", condi
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
            JArray returnJT = new JArray();
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
            returnJT = (JArray)returnValue["data"];
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
                            " LEFT JOIN [ntust].[student_apply_status] sas on sas.sas_s_id=s.s_id" +
                            " LEFT JOIN (SELECT max(sse_event) now_status, sse_s_id FROM [ntust].[student_state_event] GROUP BY sse_s_id) sse on sse.sse_s_id=s.s_id", condi
                                    , select: "(Case when sse.now_status IS NULL then '1' else sse.now_status End) as now_status , s.s_id, (Case when sas.sas_type IS NULL then '0' else sas.sas_type End) as sas_type");
                if ((bool)returnValue["status"])
                {
                    foreach (JObject jo in returnValue["data"])
                    {
                        studentJO = new JObject();
                        s_id = (string)jo["s_id"];
                        studentJO["sid"] = s_id;

                        studentJO["otherTeacher"] = getOtherTeacher(s_id, (string)jt["t_id"]);
                        
                        studentJO["status"] = jo["now_status"];
                        studentJO["apply_status"] = jo["sas_type"];
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
                            " JOIN ntust.teacher_group tg on tg.t_id=t.t_id" +
                            " JOIN (SELECT p.p_tg_id, p.p_s_id, p.p_end_date,'out' as status FROM [ntust].[pair] p" +
                                        " JOIN [ntust].[pair] p2 on p.p_s_id=p2.p_s_id AND p2.p_pair_date=p.p_end_date WHERE p.p_end_date IS NOT NULL" +//out
                                    " UNION SELECT p2.p_tg_id, p2.p_s_id, p.p_end_date,'in' as status FROM [ntust].[pair] p" +
                                        " JOIN [ntust].[pair] p2 on p.p_s_id=p2.p_s_id AND p2.p_pair_date=p.p_end_date WHERE p.p_end_date IS NOT NULL) out on tg.tg_id=out.p_tg_id" +
                            " JOIN [ntust].[student] s on s.s_id=out.p_s_id", condi
                                    , select: "s.s_name , s.s_id, out.p_end_date as datetime, out.status as in_or_out");
                jt["history"] = (JToken)returnValue["data"];
                //Get max student by teacher
                JObject dataArray = new JObject
                {
                    ["tid"] = jt["t_id"]
                };
                String queryString = "SELECT * from ntust.max_student as ms WHERE ms.ms_t_id=@tid";
                returnValue = sqlHelper.query(queryString, dataArray);
                jt["max_student"] = (JToken)returnValue["data"];
                System.Diagnostics.Debug.Print("=========================================");

            }

            //////////////////
            JObject noTeacherStudent = new JObject();
            foreach (string semester in semesterList)
            {
                noTeacherStudent[semester] = new JArray();
            }
            noTeacherStudent["phd"] = new JArray();
            noTeacherStudent["pt_m"] = new JArray();
            noTeacherStudent["history"] = new JArray();

            condi = new JObject();
            condi["s.s_department"] = st_department;
            condi["p.p_id"] = "null";

            returnValue = sqlHelper.select("[ntust].[student] s" +
                        " LEFT JOIN [ntust].[pair] p on s.s_id=p.p_s_id" +
                        " LEFT JOIN [ntust].[student_apply_status] sas on sas.sas_s_id=s.s_id" +
                        " LEFT JOIN (SELECT max(sse_event) now_status, sse_s_id FROM [ntust].[student_state_event] GROUP BY sse_s_id) sse on sse.sse_s_id=s.s_id", condi
                                , select: "(Case when sse.now_status IS NULL then '1' else sse.now_status End) as now_status, s.s_id, (Case when sas.sas_type IS NULL then '0' else sas.sas_type End) as sas_type");
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
                        ((JArray)noTeacherStudent["phd"]).Add(studentJO);
                        continue;
                    }
                    if (s_id.Substring(6, 1) == "9")
                    {
                        ((JArray)noTeacherStudent["pt_m"]).Add(studentJO);
                        continue;
                    }
                    ((JArray)noTeacherStudent[s_id.Substring(0, 4)]).Add(studentJO);
                }
            }
            noTeacherStudent["tname"] = "無人認養";
            noTeacherStudent["max_student"] = "";
            

            returnJT.Add(noTeacherStudent);

            return returnJT;


        }

        

        

        public JObject getStudentInfo(string s_id)
        {
            JObject returnJO = new JObject();
            JObject condi = new JObject();
            JObject returnValue = new JObject();
            //string st_department = (string)st_info["st_department"];

            //condi["s_department"] = st_department;
            condi["s.s_id"] = s_id;

            returnValue = sqlHelper.select("[ntust].[student] s" +
                                        " LEFT JOIN [ntust].[pair] p on s.s_id=p.p_s_id AND p.p_end_date IS NULL"
                                    , condi
                                    , ""
                                    , "s.s_id sid, s.s_name sname, s.s_group, s.s_department"+
                                        ", (Case when p.p_id IS NULL then 'apply' else 'change' End) as doEvent"+
                                            ", STUFF((SELECT  ', ' + t.t_name " +
                                            "FROM ntust.teacher_group as tg " +
                                            "join ntust.teacher as t on t.t_id = tg.t_id " +
                                            "WHERE p.p_tg_id = tg.tg_id " +
                                            "FOR XML PATH('')),1,1,'') AS tname");

            if (!(bool)returnValue["status"])
            {
                return returnJO;

            }
            returnJO = (JObject)returnValue["data"][0];
            returnJO["apply"] = studentSQL.getApplyResultForAdmin(s_id);
            returnJO["change"] = studentSQL.getChangeResultForAdmin(s_id);
            returnJO["pairTeacher"] = studentSQL.getPairTeacher(s_id);
            returnJO["changeHistory"] = studentSQL.getChangeHistoryByID(s_id);
            return returnJO;

        }

        public JToken getOtherTeacher(string s_id, string t_id)
        {
            JArray returnJT = new JArray();
            JObject condi = new JObject();
            JObject returnValue = new JObject();
            //JObject dataArray = new JObject
            //{
            //    ["t.t_id"]=t_id,
            //    ["s.s_id"] = s_id

            //};
            //String queryString = @"SELECT t.t_name tname, t.t_id tid, tg.t_order FROM [ntust].[student] s JOIN [ntust].[pair] p on s.s_id=p.p_s_id AND p.p_end_date IS NULL " +
            //    "JOIN [ntust].[teacher_group] tg on tg.tg_id=p.p_tg_id " +
            //    "JOIN [ntust].[teacher] t on tg.t_id=t.t_id AND t.t_id != @t.t_id WHERE s.s_id=@s.s_id";
            //sqlHelper.query(queryString, dataArray);
            //if (!(bool)returnValue["status"])
            //{
            //    return returnJT;

            //}
            //return returnValue["data"];
            string st_department = (string)st_info["st_department"];

            condi["s_department"] = st_department;
            condi["s.s_id"] = s_id;

            returnValue = sqlHelper.select("[ntust].[student] s" +
                                        " join [ntust].[pair] p on s.s_id=p.p_s_id and p.p_end_date is null" +
                                        " join [ntust].[teacher_group] tg on tg.tg_id=p.p_tg_id" +
                                        " join [ntust].[teacher] t on tg.t_id=t.t_id and t.t_id != '" + t_id + "'", condi
                                    , "", "t.t_name tname, t.t_id tid, tg.t_order");

            if (!(bool)returnValue["status"])
            {
                return returnJT;

            }
            return returnValue["data"];
        }
        public JObject UpdateApply(String tg_id, String t_id, String adminId, int accept)
        {
            sqlHelper = new SQLHelper();
            String queryString = @"UPDATE sa set sa_state=@state, sa_check_by_type=1, sa_check_by_st_id=@sa_check_by_st_id FROM ntust.student_apply sa WHERE sa_tg_id = @sa_tg_id AND sa_t_id = @sa_t_id";
            JObject dataArray = new JObject();
            dataArray["state"] = accept;
            dataArray["sa_check_by_st_id"] = adminId;
            dataArray["sa_tg_id"] = tg_id;
            dataArray["sa_t_id"] = t_id;
            JObject updateStatus = sqlHelper.query(queryString, dataArray);
            return updateStatus;
        }
        
        public bool IsAllTeacherApprove(String sc_tg_id, String scota_tg_id)
        {
            JObject dataArray = new JObject
            {
                ["sc_tg_id"] = sc_tg_id,
                ["scota_tg_id"] = scota_tg_id
            };
            sqlHelper = new SQLHelper();
            String queryString = @"SELECT SUM(t.count) as 'count' FROM (SELECT COUNT(*) AS count "+
                                "FROM ntust.student_change_origin_teacher_approval scota " +
                                "WHERE scota.scota_tg_id = @scota_tg_id AND scota.scota_state!=1 UNION SELECT COUNT(*) AS count "+
                                "FROM ntust.student_change sc WHERE sc.sc_tg_id = @sc_tg_id AND sc.sc_state!=1 ) as t";
            JArray res = (JArray)sqlHelper.query(queryString, dataArray).GetValue("data");
            return Convert.ToInt32(res[0]["count"])==0?true:false;
        }

        public JObject UpdateChange(String sc_id, String org_tg_id, String s_id, String t_id, String adminId, String thesis_state, String allapprove, int accept)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray;
            String queryString;
            if (sc_id != "null") //new teacher
            {
                dataArray = new JObject
                {
                    ["sc_state"] = accept,
                    ["scota_check_by_st_id"] = adminId,
                    ["sc_id"] = sc_id,
                    ["sc_t_id"] = t_id
                };
                queryString = " UPDATE sc set sc.sc_state = @sc_state , sc.sc_check_by_type = 2, sc_check_by_st_id = @scota_check_by_st_id" +
                           " FROM ntust.student_change sc" +
                           " WHERE sc.sc_id =@sc_id AND sc.sc_t_id = @sc_t_id";
                return sqlHelper.query(queryString, dataArray);
                

            }
            else //original teacher
            {
                dataArray = new JObject
                {
                    ["scota_state"] = accept,
                    ["scota_thesis_state"] = thesis_state,
                    ["scota_check_by_st_id"] = adminId,
                    ["scota_tg_id"] = org_tg_id,
                    ["scota_t_id"] = t_id
                };
                queryString = " UPDATE scota set scota_state = @scota_state,scota_thesis_state=@scota_thesis_state" +
                            ", scota_check_by_type=2, scota_check_by_st_id=@scota_check_by_st_id" +
                           " FROM ntust.student_change_origin_teacher_approval scota" +
                           " WHERE scota.scota_tg_id = @scota_tg_id AND scota.scota_t_id = @scota_t_id";
                return sqlHelper.query(queryString, dataArray);
            }
            

        }
        public int CheckAllApply(String tg_id)
        {
            //找出所有未同意的老師申請的數量
            sqlHelper = new SQLHelper();
            JObject obj = new JObject();
            JObject dataArray = new JObject
            {
                ["sa_tg_id"]= tg_id
            };
            String queryString = " SELECT COUNT(*) AS count FROM " +
                            "ntust.student_apply sa " +
                            "WHERE sa.sa_tg_id =@sa_tg_id AND sa.sa_state != 1 ";
            JObject updateStatus = sqlHelper.query(queryString, dataArray);
            return Convert.ToInt32(updateStatus["data"][0]["count"]);
        }
        public JObject AddApplyPair(String tg_id, String s_id)
        {
            sqlHelper = new SQLHelper();
            //String query = "INSERT INTO ntust.pair (p_tg_id, p_s_id, p_pair_date) VALUES ("+ tg_id +", '"+ s_id +"', '')";
            String table = "ntust.pair";
            JObject obj = new JObject();
            obj.Add("p_tg_id", tg_id);
            obj.Add("p_s_id", s_id);
            obj.Add("p_pair_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            JObject res = sqlHelper.insert(table, obj);
            return res;
        }
        public void UpdateStudentApplyHistory(String tg_id, int state,String time)
        {
            sqlHelper = new SQLHelper();
            //String query = "UPDATE hsa set hsa_end_datetime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ,hsa_state=" + state + "  FROM ntust.history_student_apply hsa WHERE hsa_tg_id = " + tg_id;
            //sqlHelper.update(query);
            String queryString = "UPDATE hsa set hsa_end_datetime='" + time + "' ,hsa_state=" + state + "  FROM ntust.history_student_apply hsa WHERE hsa_tg_id = " + tg_id;
            JObject dataArray = new JObject
            {
                ["hsa_state"]=state,
                ["hsa_tg_id"]=tg_id
            };
            sqlHelper.query(queryString, dataArray);
        }
        public JObject UpdateStudentApplyStatus(String s_id, int state)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["sas_type"] = state,
                ["sas_s_id"] = s_id
            };
            String queryString = "UPDATE sas set sas_type=@sas_type FROM ntust.student_apply_status AS sas WHERE sas_s_id=@sas_s_id";
            //String query = "UPDATE sas set sas_type=" + state + " FROM ntust.student_apply_status AS sas WHERE sas_s_id='" + s_id + "'";
            //JObject applyStatus = sqlHelper.select(query);
            JObject applyStatus = sqlHelper.query(queryString, dataArray);
            return applyStatus;
        }
        public bool IsAllOrgTeacherApprove(String org_tg_id)
        {
            sqlHelper = new SQLHelper();
            //query for apply which not agree 
            JObject dataArray = new JObject
            {
                ["scota_tg_id"] = org_tg_id
            };
            String query = @"SELECT COUNT(*) AS count " +
                        "FROM ntust.student_change_origin_teacher_approval scota " +
                        "WHERE scota.scota_tg_id = @scota_tg_id AND scota.scota_state!=1";
            int notApprrovalCount = Convert.ToInt32(sqlHelper.query(query, dataArray)["data"][0]["count"]);
            return notApprrovalCount == 0 ? true : false;
        }
        public JObject UpdateStudentChangeApproval(String tg_id)
        {

            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["sc_tg_id"] = tg_id
            };
            String queryString = @"UPDATE sc set sc.sc_all_approval = 1 " +
                            "FROM ntust.student_change sc " +
                            "WHERE sc.sc_tg_id = @sc_tg_id";
            return sqlHelper.query(queryString, dataArray);
        }
        public int CheckNewChange(String tg_id)
        {
            sqlHelper = new SQLHelper();
            //query for apply which not agree 
            String query = "SELECT COUNT(*) AS count " +
                        "FROM ntust.student_change sc " +
                        "WHERE sc.sc_tg_id = " + tg_id + " AND sc.sc_state!=1";
            JObject updateStatus = sqlHelper.select(query);
            return Convert.ToInt32(updateStatus["data"][0]["count"]);
        }
        public void UpdateOrgPair(string org_tg_id, String time)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["org_tg_id"] = org_tg_id
            };
            String queryString = @"UPDATE p set p_end_date='" + time + "' FROM ntust.pair p WHERE p.p_tg_id = @org_tg_id";
            sqlHelper.query(queryString, dataArray);
            //JObject data = new JObject();
            //data.Add("p_s_id", s_id);
            //String table = "ntust.pair";
            //sqlHelper.delete(table, data);

        }
        public JObject AddChangePair(String new_tg_id, String s_id, String time)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["new_tg_id"] = new_tg_id,
                ["s_id"] = s_id
            };
            //String queryString = @"INSERT into ntust.pair (p_tg_id, p_s_id ,p_pair_date) " +
            //    "SELECT sc.sc_tg_id, @s_id,'"+now+"' " +
            //    "from ntust.student_change sc " +
            //    "WHERE sc.sc_id = @sc_id ";
            String queryString = @"INSERT into ntust.pair (p_tg_id, p_s_id ,p_pair_date) VALUES(@new_tg_id, @s_id,'"+ time + "')";
            return sqlHelper.query(queryString, dataArray);
            
        }
        public void UpdateStudentChangeHistory(String tg_id, int state, String time)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["hsc_origin_tg_id"] = tg_id,
                ["hsc_state"] = state
            };
            String queryString = @"UPDATE hsc set hsc_end_datetime='" + time + "' ,hsc_state=@hsc_state  FROM ntust.history_student_change hsc WHERE hsc_origin_tg_id = @hsc_origin_tg_id";
            sqlHelper.query(queryString, dataArray);
            
        }
        public JObject addNewExtraTeacher(String t_name, String t_email, String t_phone, String t_telephone, String t_service_units)
        {
            sqlHelper = new SQLHelper();

            JObject callback = new JObject();
            JObject condi = new JObject();
            condi["t_email"] = t_email;
            JObject returnValue = sqlHelper.select("[ntust].[extra_teacher]", condi);
            if ((bool)returnValue["status"])
            {
                callback["status"] = false;
                callback["msg"] = "email is exist.";
                return callback;
            }

            condi = new JObject();
            condi["t_phone"] = t_phone;
            returnValue = sqlHelper.select("[ntust].[extra_teacher]", condi);
            if ((bool)returnValue["status"])
            {
                callback["status"] = false;
                callback["msg"] = "phone is exist.";
                return callback;
            }

            condi = new JObject();
            condi["t_telephone"] = t_telephone;
            returnValue = sqlHelper.select("[ntust].[extra_teacher]", condi);
            if ((bool)returnValue["status"])
            {
                callback["status"] = false;
                callback["msg"] = "telephone is exist.";
                return callback;
            }

            String t_id;
            while (true) {
                t_id = RandomString(8);
                condi = new JObject();
                condi["t_id"] = t_id;
                returnValue = sqlHelper.select("[ntust].[extra_teacher]", condi);
                if (!(bool)returnValue["status"])
                {
                    break;
                }
            }
            

            String table = "ntust.extra_teacher";
            condi = new JObject();
            condi["t_id"] = t_id;
            condi["t_name"] = t_name;
            condi["t_email"] = t_email;
            condi["t_phone"] = t_phone;
            condi["t_telephone"] = t_telephone;
            condi["t_service_units"] = t_service_units;
            condi["t_create_by"] = 2;
            condi["t_create_by_id"] = st_id;
            condi["t_create_datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JObject res = sqlHelper.insert(table, condi);
            return res;
        }
        
        public int GetMaxStudentNum(string tid, string sid)
        {
            sqlHelper = new SQLHelper();
            string semester = sid.Substring(1, 3);
            JObject dataArray = new JObject
            {
                ["t_id"] = tid,
                ["semester"] = semester
            };
            String queryString = @"SELECT ms.ms_max_student_num FROM ntust.max_student as ms WHERE ms.ms_t_id=@t_id AND ms.ms_semester =@semester";
            JObject res = sqlHelper.query(queryString, dataArray);

            return Convert.ToInt32(res["data"][0]["ms_max_student_num"]);
        }
        internal JObject UpdateMaxStudent(string tid, int max_student_num, string semester)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["max_student_num"] = max_student_num,
                ["tid"] = tid,
                ["semester"] = semester
            };
            String queryString = "UPDATE ms set ms.ms_max_student_num=@max_student_num " +
                "FROM ntust.max_student as ms WHERE ms.ms_t_id=@tid AND ms.ms_semester=@semester";
            JObject updateStatus = sqlHelper.query(queryString, dataArray);
            return updateStatus;
        }
        public int GetCurrentStudentNum(string tid, string sid)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["tid"] = tid
            };
            String queryString = "SELECT p.p_s_Id FROM ntust.pair  as p, " +
                "ntust.teacher_group as tg WHERE p.p_end_date IS NULL " +
                "AND tg.tg_id=p.p_tg_id AND tg.t_id=@tid";
            JObject student = sqlHelper.query(queryString, dataArray);
            int count = 0;
            foreach (JObject i in student["data"])
            {
                //System.Diagnostics.Debug.Print();
                String p_sid = (String)i["p_s_Id"];
                //略過外籍生與在職生
                if (p_sid[6] == '8' || p_sid[6] == '9')
                {
                    continue;
                }
                if (p_sid.Substring(0, 3).Equals(sid.Substring(0, 3)))
                {
                    count++;
                }
                //System.Diagnostics.Debug.Print((String)p_sid.Substring(1, 4));

            }


            return count;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public int IsNewTeacher(string sid, string tid)
        {
            //return 1 means this student is applying change to teacher
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["sid"] = sid
            };
            String query = @"SELECT tg.t_id FROM ntust.pair as p, ntust.teacher_group as tg WHERE p.p_tg_id = tg.tg_id AND p.p_s_id = @sid AND p_end_date IS NULL";

            JObject result = sqlHelper.query(query, dataArray);
            foreach (JObject i in result["data"])
            {
                string id = (String)i["t_id"];
                if (id == sid)
                    return 0;
            }
            return 1;
        }
    }
}
 