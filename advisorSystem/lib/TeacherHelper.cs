using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace advisorSystem.lib
{
    public class TeacherHelper
    {
        
        SQLHelper sqlHelper;
        private string t_id;
        JObject t_info = new JObject();

        public TeacherHelper(){
            sqlHelper = new SQLHelper();
        }

        public JObject getTeacherInfo(string userId)
        {
            JObject condi = new JObject();
            condi["t_u_id"] = userId;
            JObject returnValue = sqlHelper.select("[ntust].[teacher]", condi);
            if ((bool)returnValue["status"])
            {
                foreach (JToken jt in returnValue["data"])
                {
                    t_info = (JObject)jt;
                    t_id = (string)t_info["t_id"];
                }
                return t_info;
            }
            else
            {
                //String msg = returnValue["msg"].ToString();
                return t_info;
            }
        }
        

        public JObject GetStudent()
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["t_id"] = t_id
            };
            String queryString = @"SELECT s.s_id, s.s_name, s.s_state,STUFF(( " +
                "SELECT ','+sub_t.t_name FROM ntust.teacher as sub_t join ntust.teacher_group as sub_tg on sub_t.t_id=sub_tg.t_id " +
                "WHERE sub_tg.tg_id=tg.tg_id FOR XML PATH('')),1 ,1 ,'' ) AS whole_teacher  " +
                "FROM ntust.teacher t  " +
                "left join ntust.teacher_group tg on t.t_id=tg.t_id  " +
                "join ntust.pair p on p.p_tg_id=tg.tg_id and p.p_end_date IS NULL " +
                "join ntust.student s on s.s_id = p.p_s_id " +
                "left join ntust.teacher t2 on t2.t_id=tg.t_id WHERE t.t_id=@t_id ;";
            return sqlHelper.query(queryString, dataArray);
        }
        public JObject GetApply()
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["t_id"] = t_id
            };
            String queryString = @"SELECT sa.sa_s_id, sa.sa_tg_id, s.s_name,hsa.hsa_create_datetime, 0 AS allapprove, " +
                                "STUFF((SELECT  ', ' + t.t_name " +
                                "FROM ntust.student_apply as sa_all " +
                                "join ntust.teacher as t on t.t_id = sa_all.sa_t_id " +
                                "WHERE sa_all.sa_tg_id = sa.sa_tg_id " +
                                "FOR XML PATH('')),1,1,''" +
                                ") AS all_teacher "+
                                "from ntust.student_apply as sa " +
                                "join [ntust].[teacher_group] as tg on tg.tg_id = sa.sa_tg_id " +
                                "join ntust.student as s on s.s_id = sa.sa_s_id " +
                                "join ntust.history_student_apply as hsa on hsa.hsa_s_id = sa.sa_s_id AND hsa.hsa_tg_id = sa.sa_tg_id AND hsa.hsa_end_datetime IS NULL " +
                                "WHERE sa.sa_state = 0 AND sa.sa_t_id = @t_id " +
                                "UNION " +
                                "SELECT sa.sa_s_id, sa.sa_tg_id, s.s_name,hsa.hsa_create_datetime,1 AS allapprove, " +
                                "STUFF((SELECT  ', ' + t.t_name " +
                                "FROM ntust.student_apply as sa_all " +
                                "join ntust.teacher as t on t.t_id = sa_all.sa_t_id " +
                                "WHERE sa_all.sa_tg_id = sa.sa_tg_id " +
                                "FOR XML PATH('')),1,1,''" +
                                ") AS all_teacher " +
                                "from ntust.student_apply as sa " +
                                "join [ntust].[teacher_group] as tg on tg.tg_id = sa.sa_tg_id " +
                                "join ntust.student_apply as sa_notapprove on sa_notapprove.sa_tg_id = tg.tg_id and sa_notapprove.sa_state = 0 and sa_notapprove.sa_t_id != @t_id " +
                                "join ntust.student as s on s.s_id = sa.sa_s_id " +
                                "join ntust.history_student_apply as hsa on hsa.hsa_s_id = sa.sa_s_id AND hsa.hsa_tg_id = sa.sa_tg_id AND hsa.hsa_end_datetime IS NULL " +
                                "WHERE(sa.sa_state = 1 OR sa.sa_state = 2) AND sa.sa_t_id = @t_id";

            return sqlHelper.query(queryString, dataArray);

        }
        public JObject GetChange()
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["t_id"] = t_id
            };
            String queryString = "SELECT DISTINCT @t_id AS t_id,sc_t_id, STUFF((SELECT  ',' + org_t.t_id " +
                "FROM ntust.teacher_group as org_tg join ntust.teacher as org_t " +
                "on org_t.t_id = org_tg.t_id " +
                "WHERE hsc.hsc_origin_tg_id = org_tg.tg_id FOR XML PATH('')), 1, 1, '') " +
                "AS org_t_id " +
                ", STUFF ((SELECT  ',' + new_t.t_id " +
                "FROM ntust.teacher_group as new_tg join ntust.teacher as new_t " +
                "on new_t.t_id = new_tg.t_id " +
                "WHERE sc.sc_tg_id = new_tg.tg_id FOR XML PATH('')), 1, 1, '') AS new_t_id," +
                "sc.sc_s_id, s.s_name, hsc.hsc_create_datetime" +
                ", sc.sc_tg_id, hsc.hsc_origin_tg_id," +
                "STUFF((SELECT  ',' + new_t.t_name FROM ntust.teacher_group as new_tg join ntust.teacher as new_t " +
                "on new_t.t_id = new_tg.t_id WHERE sc.sc_tg_id = new_tg.tg_id FOR XML PATH('')), 1, 1, '') " +
                "AS new_teacher," +
                "STUFF ((SELECT  ',' + org_t.t_name FROM ntust.teacher_group as org_tg join ntust.teacher as org_t " +
                "on org_t.t_id = org_tg.t_id " +
                "WHERE hsc.hsc_origin_tg_id = org_tg.tg_id FOR XML PATH('')), 1, 1, '') AS org_teacher," +
                "sc.sc_all_approval, scota.scota_state, sc.sc_state, p.p_id " +
                "FROM ntust.student_change as sc " +
                "join ntust.teacher_group as tg on sc.sc_tg_id = tg.tg_id " +
                "join ntust.student as s on s.s_id = sc.sc_s_id " +
                "join ntust.history_student_change " +
                "as hsc on hsc.hsc_s_id = s.s_id AND hsc.hsc_end_datetime IS NULL AND hsc.hsc_tg_id = sc.sc_tg_id " +
                "join ntust.teacher_group as org_tg on org_tg.tg_id = hsc.hsc_origin_tg_id " +
                "left join ntust.student_change_origin_teacher_approval " +
                "as scota on scota.scota_tg_id = org_tg.tg_id AND scota.scota_t_id = @t_id " +
                "left join ntust.pair as p on p.p_tg_id = sc.sc_tg_id " +
                "WHERE((org_tg.t_id = @t_id AND sc.sc_state = 0 AND hsc.hsc_end_datetime IS NULL) " +
                "OR(sc.sc_t_id = @t_id AND sc.sc_all_approval = 1 AND hsc.hsc_end_datetime IS NULL)) AND p.p_tg_id is NULL";


            //String queryString = "SELECT DISTINCT  sc.sc_id, scota.scota_t_id,sc.sc_t_id, sc.sc_s_id, s.s_name, hsc.hsc_create_datetime, sc.sc_tg_id, hsc.hsc_origin_tg_id, " +
            //                    "sc.sc_tg_id, hsc.hsc_origin_tg_id, " +
            //                    "STUFF ((SELECT  ', ' + new_t.t_name " +
            //                    "FROM ntust.teacher_group as new_tg join ntust.teacher as new_t " +
            //                    "on new_t.t_id = new_tg.t_id " +
            //                    "WHERE sc.sc_tg_id = new_tg.tg_id FOR XML PATH('')), 1, 1, '') AS new_teacher, " + 
            //                    "STUFF " +
            //                    "((SELECT  ', ' + org_t.t_name " +
            //                    "FROM ntust.teacher_group as org_tg " +
            //                    "join ntust.teacher as org_t on org_t.t_id = org_tg.t_id " +
            //                    "WHERE hsc.hsc_origin_tg_id = org_tg.tg_id " +
            //                    "FOR XML PATH('')), 1, 1, '') AS org_teacher, sc.sc_all_approval, scota.scota_state, sc.sc_state, p.p_id " +
            //                    "FROM ntust.student_change as sc " +
            //                    "join ntust.teacher_group as tg on sc.sc_tg_id = tg.tg_id " +
            //                    "join ntust.teacher as t on t.t_id = sc.sc_t_id " +
            //                    "join ntust.student as s on s.s_id = sc.sc_s_id " +
            //                    "join ntust.history_student_change as hsc on hsc.hsc_s_id = s.s_id AND hsc.hsc_end_datetime IS NULL AND hsc.hsc_tg_id = sc.sc_tg_id " +
            //                    "join ntust.teacher_group as org_tg on org_tg.tg_id = hsc.hsc_origin_tg_id " +
            //                    "left join ntust.student_change_origin_teacher_approval as scota on scota.scota_tg_id = org_tg.tg_id AND scota.scota_t_id = @t_id " +
            //                    "left join ntust.pair as p on p.p_tg_id = sc.sc_tg_id " +
            //                    "WHERE((org_tg.t_id = @t_id AND sc.sc_state = 0 AND hsc.hsc_end_datetime IS NULL) OR(t.t_id = @t_id AND sc.sc_all_approval = 1 AND hsc.hsc_end_datetime IS NULL)) AND p.p_tg_id is NULL";
            return sqlHelper.query(queryString, dataArray);
            
        }

        public int NewTeacher(string sid, string tid)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["sid"] = sid
            };
            String query = @"SELECT tg.t_id FROM ntust.pair as p, ntust.teacher_group as tg WHERE p.p_tg_id = tg.tg_id AND p.p_s_id = @sid AND p_end_date IS NULL";
            
            JObject result = sqlHelper.query(query, dataArray);
            foreach(JObject i in result["data"])
            {
                string id = (String)i["t_id"];
                if (id == tid)
                    return 0;
            }
            return 1;
        }

        public void UpdateStudentApplyHistory(String tg_id, int state)
        {
            sqlHelper = new SQLHelper();
            String time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JObject dataArray = new JObject
            {
                ["hsa_state"] = state,
                ["hsa_tg_id"] = tg_id
            };
            String query = @"UPDATE hsa set hsa_end_datetime='"+ time + "' ,hsa_state=@hsa_state  FROM ntust.history_student_apply hsa WHERE hsa_tg_id =@hsa_tg_id ";
            sqlHelper.query(query, dataArray);
        }
        public void UpdateStudentChangeHistory(String tg_id, int state, String time)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["hsa_state"] = state,
                ["hsc_origin_tg_id"] = tg_id
            };
            String query = @"UPDATE hsc set hsc_end_datetime='" + time + "' ,hsc_state=@hsa_state  FROM ntust.history_student_change hsc WHERE hsc_origin_tg_id = @hsc_origin_tg_id";
            sqlHelper.update(query);
        }
        public void UpdateOrgPair(string org_tg_id,String time)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["p_tg_id"] = org_tg_id
            };
            String queryStr = @"UPDATE p set p_end_date='" + time + "'  FROM ntust.pair p WHERE p.p_tg_id = @p_tg_id";
            sqlHelper.query(queryStr, dataArray);
            
        }

        public JObject UpdateApply(String tg_id, int accept)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["sa_state"] = accept,
                ["sa_check_by_st_id"] = t_id,
                ["sa_t_id"] = t_id,
                ["sa_tg_id"] = tg_id
            };
            String queryString = @"UPDATE sa set sa_state=@sa_state, sa_check_by_type=1, sa_check_by_st_id=@sa_check_by_st_id" + 
                            " FROM ntust.student_apply sa" +
                            " WHERE sa_tg_id = @sa_tg_id AND sa_t_id = @sa_t_id";
            JObject updateStatus = sqlHelper.query(queryString, dataArray);
            return updateStatus;
                        
        }
        public int CheckAllApply(String tg_id)
        {
            sqlHelper = new SQLHelper();
            //找出所有未同意的老師申請的數量
            JObject dataArray = new JObject
            {
                ["sa_tg_id"] = tg_id
            };
            //query for apply which not agree 
            String queryString = @" SELECT COUNT(*) AS count FROM " +
                            "ntust.student_apply sa "+
                            "WHERE sa.sa_tg_id = @sa_tg_id AND sa.sa_state != 1 ";
            JObject updateStatus = sqlHelper.query(queryString, dataArray);

            JArray array = (JArray)updateStatus.GetValue("data");
            return Convert.ToInt32(updateStatus["data"][0]["count"]);
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
        public bool IsAllTeacherApprove(String sc_tg_id, String scota_tg_id)
        {
            JObject dataArray = new JObject
            {
                ["sc_tg_id"] = sc_tg_id,
                ["scota_tg_id"] = scota_tg_id
            };
            sqlHelper = new SQLHelper();
            String queryString = @"SELECT SUM(t.count) as 'count' FROM (SELECT COUNT(*) AS count " +
                                "FROM ntust.student_change_origin_teacher_approval scota " +
                                "WHERE scota.scota_tg_id = @scota_tg_id AND scota.scota_state!=1 UNION SELECT COUNT(*) AS count " +
                                "FROM ntust.student_change sc WHERE sc.sc_tg_id = @sc_tg_id AND sc.sc_state!=1 ) as t";
            JArray res = (JArray)sqlHelper.query(queryString, dataArray).GetValue("data");
            return Convert.ToInt32(res[0]["count"]) == 0 ? true : false;
        }
        public int CheckOrgChange(String org_tg_id)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["scota_tg_id"] = org_tg_id
            };
            //query for apply which not agree 
            String queryString = @"SELECT COUNT(*) AS count "+
                        "FROM ntust.student_change_origin_teacher_approval scota "+
                        "WHERE scota.scota_tg_id = @scota_tg_id AND scota.scota_state!=1";
            JObject updateStatus = sqlHelper.query(queryString, dataArray);
            return Convert.ToInt32(updateStatus["data"][0]["count"]);
        }
        public int CheckNewChange(String tg_id)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["sc_tg_id"] = tg_id
            };
            //query for apply which not agree 
            String queryString = @"SELECT COUNT(*) AS count " +
                        "FROM ntust.student_change sc " +
                        "WHERE sc.sc_tg_id = " + tg_id + " AND sc.sc_state!=1";
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
        public JObject AddChangePair(String new_tg_id, String s_id, String time)
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["new_tg_id"] = new_tg_id,
                ["s_id"] = s_id
            };
            String queryString = @"INSERT into ntust.pair (p_tg_id, p_s_id ,p_pair_date) VALUES(@new_tg_id, @s_id,'" + time + "')";
            return sqlHelper.query(queryString, dataArray);
        }
        public JObject UpdateChange( String org_tg_id, String tg_id, String s_id, String t_id, String thesis_state, String allapprove, int accept)
        {
            sqlHelper = new SQLHelper();
            String queryString;
            JObject dataArray;
            if (allapprove.Equals("0")) //更新原本老師的表(scota)
            {
                dataArray = new JObject
                {
                    ["scota_state"] = accept,
                    ["scota_thesis_state"] = thesis_state,
                    ["org_tg_id"] = org_tg_id,
                    ["scota_t_id"] = t_id
                };
                queryString = " UPDATE scota set scota_state = @scota_state,scota_thesis_state=@scota_thesis_state, scota_check_by_type=1 " +
                           " FROM ntust.student_change_origin_teacher_approval scota" +
                           " WHERE scota.scota_tg_id =@org_tg_id AND scota.scota_t_id = '" + t_id + "'";
                //檢查是否原本老師全部同意:如果是就更新allapproval=1
            }
            else //更新 申請新老師的表(sc)
            {
                dataArray = new JObject
                {
                    ["sc_state"] = accept,
                    ["sc_t_id"] = t_id,
                    ["tg_id"] = tg_id
                };
                queryString = "UPDATE sc set sc.sc_state = @sc_state , sc.sc_check_by_type = 1 "+
                              "FROM ntust.student_change sc "+
                              "WHERE sc.sc_t_id=@sc_t_id AND sc.sc_tg_id=@tg_id";
            }
            JObject updateChange = sqlHelper.query(queryString, dataArray);
            return updateChange;
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
        public JObject GetApplyHistory()
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["t_id"] = t_id
            };
            String queryString = "SELECT s.s_id,s.s_name,hsa.hsa_create_datetime, hsa.hsa_end_datetime, hsa.hsa_state, " +
                            "STUFF ((SELECT  ',' + new_t.t_name " +
                            "FROM ntust.teacher_group as new_tg "+
                            "join ntust.teacher as new_t on new_t.t_id = new_tg.t_id "+
                            "WHERE hsa.hsa_tg_id = new_tg.tg_id "+
                            "FOR XML PATH('')),1,1,'') AS Apply_teacher "+
                            "from ntust.history_student_apply hsa "+
                            "JOIN ntust.teacher_group as tg on tg.tg_id = hsa.hsa_tg_id "+
                            "JOIN ntust.student as s on s.s_id = hsa.hsa_s_id "+
                            "WHERE tg.t_id = @t_id AND hsa.hsa_end_datetime IS NOT NULL";
            JObject applyHistory = sqlHelper.query(queryString,  dataArray);
            return applyHistory;
        }
        public JObject GetChangeHistory()
        {
            sqlHelper = new SQLHelper();
            JObject dataArray = new JObject
            {
                ["t_id"] = t_id
            };
            String queryString = "SELECT DISTINCT " +
                "hsc.hsc_s_id, s.s_name, hsc.hsc_create_datetime, hsc.hsc_end_datetime, hsc.hsc_state, " +
                "STUFF((SELECT  ', ' + new_t.t_name FROM ntust.teacher_group as new_tg join ntust.teacher as new_t on new_t.t_id = new_tg.t_id " +
                "WHERE hsc.hsc_tg_id = new_tg.tg_id FOR XML PATH('')),1,1,'') AS new_teacher, " +
                "STUFF((SELECT  ', ' + org_t.t_name FROM ntust.teacher_group as org_tg join ntust.teacher as org_t on org_t.t_id = org_tg.t_id " +
                "WHERE hsc.hsc_origin_tg_id = org_tg.tg_id FOR XML PATH('')),1,1,'') AS org_teacher " +
                "FROM ntust.history_student_change as hsc " +
                "JOIN ntust.teacher_group as tg on(tg.tg_id = hsc.hsc_tg_id) " +
                "JOIN ntust.student as s on hsc.hsc_s_id = s.s_id " +
                "join ntust.teacher_group as org_tg on(org_tg.tg_id = hsc.hsc_origin_tg_id  AND hsc.hsc_end_datetime IS NOT NULL) " +
                "WHERE(tg.t_id = @t_id) OR(org_tg.t_id = @t_id)";
            //String queryString = @"SELECT DISTINCT p.p_s_id, s.s_name, hsc.hsc_create_datetime, hsc.hsc_end_datetime, hsc.hsc_state, t.t_name AS new_teacher," +
            //                " STUFF((SELECT  ', ' + org_t.t_name FROM ntust.teacher_group as org_tg"+
            //                " join ntust.teacher as org_t on org_t.t_id = org_tg.t_id"+
            //                " WHERE hsc.hsc_origin_tg_id = org_tg.tg_id"+
            //                " FOR XML PATH('')),1,1,'') AS org_teacher"+
            //                " FROM ntust.history_student_change as hsc"+
            //                " JOIN ntust.teacher_group as tg on(tg.tg_id = hsc.hsc_tg_id)"+
            //                " JOIN ntust.pair as p on p.p_tg_id = tg.tg_id"+
            //                " JOIN ntust.student as s on p.p_s_id = s.s_id"+
            //                " JOIN ntust.teacher as t on t.t_id = tg.t_id"+
            //                " join ntust.teacher_group as org_tg on(org_tg.tg_id = hsc.hsc_origin_tg_id  AND hsc.hsc_end_datetime IS NOT NULL)"+
            //                "WHERE(tg.t_id = @t_id) OR(org_tg.t_id = @t_id)";
            JObject changeHistory = sqlHelper.query(queryString, dataArray);
            return changeHistory;
        }
        public JObject UpdateStudentApplyStatus(String s_id, int state)
        {
            sqlHelper = new SQLHelper();
            String query = "UPDATE sas set sas_type="+state+ " FROM ntust.student_apply_status AS sas WHERE sas_s_id='" + s_id+"'";
            JObject applyStatus = sqlHelper.select(query);
            return applyStatus;
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
    }
    
}