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
            JObject teacher = new JObject();

            teacher["t.t_id"] = t_id;

            String select = "s.s_id, s.s_name, s.s_state,STUFF(( SELECT ','+sub_t.t_name FROM ntust.teacher as sub_t join ntust.teacher_group as sub_tg on sub_t.t_id=sub_tg.t_id WHERE sub_tg.tg_id=tg.tg_id FOR XML PATH('')),1 ,1 ,'' ) AS whole_teacher ";
            
            return sqlHelper.select2("ntust.teacher t " +
                                                " left join ntust.teacher_group tg on t.t_id=tg.t_id " +
                                                " join ntust.pair p on p.p_tg_id=tg.tg_id " +
                                                " join ntust.student s on s.s_id = p.p_s_id  " +
                                                " left join ntust.teacher t2 on t2.t_id=tg.t_id", teacher, select: select);
            

        }
        public JObject GetApply()
        {
            sqlHelper = new SQLHelper();
            JObject obj = new JObject();

            obj["sa.sa_state"] = 0;
            obj["sa.sa_t_id"] = t_id;

            String select = " sa.sa_s_id, s.s_name, hsa.hsa_create_datetime, sa.sa_tg_id";


            //return sqlHelper.select("ntust.student_apply as sa " +
            //                                    " join [ntust].[teacher_group] as tg on tg.tg_id = sa.sa_tg_id" +
            //                                    " join ntust.student as s on s.s_id = sa.sa_s_id" +
            //                                    " join ntust.history_student_apply as hsa on hsa.hsa_s_id = sa.sa_s_id" 
            //                                    , obj, select: select);
            String queryStr = "SELECT sa.sa_s_id, sa.sa_tg_id, s.s_name,hsa.hsa_create_datetime, 0 AS allapprove " +
                                "from ntust.student_apply as sa " +
                                "join [ntust].[teacher_group] as tg on tg.tg_id = sa.sa_tg_id " +
                                "join ntust.student as s on s.s_id = sa.sa_s_id " +
                                "join ntust.history_student_apply as hsa on hsa.hsa_s_id = sa.sa_s_id " +
                                "WHERE sa.sa_state = 0 AND sa.sa_t_id = '"+ t_id +"' " +
                                "UNION " +
                                "SELECT sa.sa_s_id, sa.sa_tg_id, s.s_name,hsa.hsa_create_datetime,1 AS allapprove " +
                                "from ntust.student_apply as sa " +
                                "join [ntust].[teacher_group] as tg on tg.tg_id = sa.sa_tg_id " +
                                "join ntust.student_apply as sa_notapprove on sa_notapprove.sa_tg_id = tg.tg_id and sa_notapprove.sa_state = 0 and sa_notapprove.sa_t_id != '" + t_id + "' " +
                                "join ntust.student as s on s.s_id = sa.sa_s_id " +
                                "join ntust.history_student_apply as hsa on hsa.hsa_s_id = sa.sa_s_id " +
                                "WHERE(sa.sa_state = 1 OR sa.sa_state = 2) AND sa.sa_t_id = '" + t_id + "'";

            return sqlHelper.select(queryStr);

        }
        public JObject GetChange()
        {
            sqlHelper = new SQLHelper();

            return sqlHelper.select("SELECT DISTINCT  sc.sc_id, sc.sc_t_id, sc.sc_s_id, s.s_name, hsc.hsc_create_datetime,hsc.hsc_origin_tg_id, t.t_name AS new_teacher, " +
                                "STUFF " +
                                "((SELECT  ', ' + org_t.t_name " +
                                "FROM ntust.teacher_group as org_tg " +
                                "join ntust.teacher as org_t on org_t.t_id = org_tg.t_id " +
                                "WHERE hsc.hsc_origin_tg_id = org_tg.tg_id " +
                                "FOR XML PATH('')), 1, 1, '') AS org_teacher, sc.sc_all_approval, scota.scota_state, sc.sc_state, p.p_id " +
                                "   FROM ntust.student_change as sc " +
                                "join ntust.teacher_group as tg on sc.sc_tg_id = tg.tg_id " +
                                "join ntust.teacher as t on t.t_id = sc.sc_t_id " +
                                "join ntust.student as s on s.s_id = sc.sc_s_id " +
                                "join ntust.history_student_change as hsc on hsc.hsc_s_id = s.s_id " +
                                "left join ntust.student_change_origin_teacher_approval as scota on scota.scota_sc_id = sc.sc_id AND scota.scota_t_id = '"+ t_id + "' " +
                                "join ntust.teacher_group as org_tg on org_tg.tg_id = hsc.hsc_origin_tg_id " +
                                "left join ntust.pair as p on p.p_tg_id = sc.sc_tg_id " +
                                "WHERE((org_tg.t_id = '" + t_id + "' AND sc.sc_state = 0) OR(t.t_id = '" + t_id + "' AND sc.sc_all_approval = 1 AND sc.sc_state = 0)) AND p.p_tg_id is NULL");


        }

        public void UpdateStudentApplyHistory(String tg_id, int state)
        {
            sqlHelper = new SQLHelper();
            String query = "UPDATE hsa set hsa_end_datetime='"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ,hsa_state="+state+"  FROM ntust.history_student_apply hsa WHERE hsa_tg_id = " + tg_id;
            sqlHelper.update(query);
        }
        public void UpdateStudentChangeHistory(String tg_id, int state)
        {
            sqlHelper = new SQLHelper();
            String query = "UPDATE hsc set hsc_end_datetime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ,hsc_state=" + state + "  FROM ntust.history_student_change hsc WHERE hsc_origin_tg_id = " + tg_id;
            sqlHelper.update(query);
        }
        public void removePair(string s_id)
        {
            sqlHelper = new SQLHelper();
            //String query = "DELETE FROM ntust.pair WHERE s_id =  "+s_id;
            JObject data = new JObject();
            data.Add("s_id", s_id);
            String table = "ntust.pair";
            sqlHelper.delete(table, data);
            //sqlHelper.delete(query);
        }

        public JObject UpdateApply(String tg_id, int accept)
        {
            sqlHelper = new SQLHelper();
            JObject obj = new JObject();
            String query = " update sa set sa_state="+ accept + ", sa_check_by_type=1, sa_check_by_st_id='"+t_id+"'" + 
                            " FROM ntust.student_apply sa" +
                            " WHERE sa_tg_id = '"+ tg_id + "' AND sa_t_id = '"+ t_id +"'";
            JObject updateStatus = sqlHelper.update(query);
            return updateStatus;
                        
        }
        public int CheckAllApply(String tg_id)
        {
            sqlHelper = new SQLHelper();
            //找出所有未同意的老師申請的數量
            JObject obj = new JObject();
            //query for apply which not agree 
            String query = " SELECT COUNT(*) AS count FROM " +
                            "ntust.student_apply sa "+
                            "WHERE sa.sa_tg_id = "+ tg_id +" AND sa.sa_state != 1 ";
            JObject updateStatus = sqlHelper.select(query);

            JArray array = (JArray)updateStatus.GetValue("data");
            System.Diagnostics.Debug.Print(array.ToString());
            return Convert.ToInt32(updateStatus["data"][0]["count"]);
        }
        public int CheckOrgChange(String sc_id)
        {
            sqlHelper = new SQLHelper();
            //query for apply which not agree 
            String query = "SELECT COUNT(*) AS count "+
                        "FROM ntust.student_change_origin_teacher_approval scota "+
                        "WHERE scota.scota_sc_id = "+sc_id+" AND scota.scota_state!=1";
            JObject updateStatus = sqlHelper.select(query);
            return Convert.ToInt32(updateStatus["data"][0]["count"]);
        }
        public int CheckNewChange(String sc_id)
        {
            sqlHelper = new SQLHelper();
            //query for apply which not agree 
            String query = "SELECT COUNT(*) AS count " +
                        "FROM ntust.student_change_origin_teacher_approval scota " +
                        "WHERE scota.scota_sc_id = " + sc_id + " AND scota.scota_state!=1";
            JObject updateStatus = sqlHelper.select(query);
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
        public JObject AddChangePair(String sc_id, String s_id)
        {
            sqlHelper = new SQLHelper();
            String query = "INSERT into ntust.pair (p_tg_id, p_s_id ,p_pair_date) " +
                           "SELECT sc.sc_tg_id, '" + s_id + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                           "from ntust.student_change sc " +
                           "WHERE sc.sc_id = " + sc_id;
            
            JObject res = sqlHelper.insert(query);

            return res;
        }
        public JObject UpdateChange(String sc_id, String s_id, String t_id, String thesis_state, String allapprove, int accept)
        {
            sqlHelper = new SQLHelper();
            String query;
            if (allapprove.Equals("0")) //更新原本老師的表(scota)
            {
                query = " UPDATE scota set scota_state = "+accept+",scota_thesis_state="+ thesis_state + 
                            ", scota_check_by_type=1, scota_check_by_st_id='"+ t_id + "' " +
                           " FROM ntust.student_change_origin_teacher_approval scota" +
                           " WHERE scota.scota_sc_id ='" + sc_id + "' AND scota.scota_t_id = '" + t_id + "'";
                //檢查是否原本老師全部同意:如果是就更新allapproval=1
            }
            else //更新 申請新老師的表(sc)
            {
                query = " UPDATE sc set sc.sc_state = " + accept + ", sc.sc_check_by_type = 1, sc_check_by_st_id = '" + t_id +"'"+
                           " FROM ntust.student_change sc" +
                           " WHERE sc.sc_id ='" + sc_id + "' AND sc.sc_t_id = '" + t_id + "'";
            }
            JObject updateChange = sqlHelper.update(query);
            return updateChange;
        }
        public JObject UpdateStudentChangeApproval(String sc_id)
        {

            sqlHelper = new SQLHelper();
            String query = "UPDATE sc set sc.sc_all_approval = 1 "+
                            "FROM ntust.student_change sc "+
                            "WHERE sc.sc_id = "+sc_id;
            JObject updateSCA = sqlHelper.update(query);
            return updateSCA;
        }
        public JObject GetApplyHistory()
        {
            sqlHelper = new SQLHelper();
            String query = "SELECT s.s_id,s.s_name,hsa.hsa_create_datetime, hsa.hsa_end_datetime, hsa.hsa_state, " +
                            "STUFF ((SELECT  ', ' + new_t.t_name " +
                            "FROM ntust.teacher_group as new_tg "+
                            "join ntust.teacher as new_t on new_t.t_id = new_tg.t_id "+
                            "WHERE hsa.hsa_tg_id = new_tg.tg_id "+
                            "FOR XML PATH('')),1,1,'') AS Apply_teacher "+
                            "from ntust.history_student_apply hsa "+
                            "JOIN ntust.teacher_group as tg on tg.tg_id = hsa.hsa_tg_id "+
                            "JOIN ntust.student as s on s.s_id = hsa.hsa_s_id "+
                            "WHERE tg.t_id = '"+t_id+"' AND hsa.hsa_end_datetime IS NOT NULL";
            JObject applyHistory = sqlHelper.select(query);
            return applyHistory;
        }
        public JObject GetChangeHistory()
        {
            sqlHelper = new SQLHelper();
            String query = "SELECT DISTINCT p.p_s_id, s.s_name, hsc.hsc_create_datetime, hsc.hsc_end_datetime, hsc.hsc_state, t.t_name AS new_teacher," +
                            " STUFF((SELECT  ', ' + org_t.t_name FROM ntust.teacher_group as org_tg"+
                            " join ntust.teacher as org_t on org_t.t_id = org_tg.t_id"+
                            " WHERE hsc.hsc_origin_tg_id = org_tg.tg_id"+
                            " FOR XML PATH('')),1,1,'') AS org_teacher"+
                            " FROM ntust.history_student_change as hsc"+
                            " JOIN ntust.teacher_group as tg on(tg.tg_id = hsc.hsc_tg_id)"+
                            " JOIN ntust.pair as p on p.p_tg_id = tg.tg_id"+
                            " JOIN ntust.student as s on p.p_s_id = s.s_id"+
                            " JOIN ntust.teacher as t on t.t_id = tg.t_id"+
                            " join ntust.teacher_group as org_tg on(org_tg.tg_id = hsc.hsc_origin_tg_id  AND hsc.hsc_end_datetime IS NOT NULL)"+
                            "WHERE(tg.t_id = '"+t_id+"') OR(org_tg.t_id = '"+t_id+"')";
            JObject changeHistory = sqlHelper.select(query);
            return changeHistory;
        }
    }
    
}