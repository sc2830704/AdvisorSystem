using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace advisorSystem.lib
{

    //StudentHelper
    public class StudentSQL
    {
        SQLHelper sqlHelper = new SQLHelper();
        public string s_id = null;
        JObject s_info = new JObject();

        public JObject getStudentInfo(string userId)
        {
            JObject condi = new JObject();
            condi["s_u_id"] = userId;
            JObject returnValue = sqlHelper.select("[ntust].[student]", condi);
            if ((bool)returnValue["status"])
            {
                foreach (JToken jt in returnValue["data"])
                {
                    s_info = (JObject)jt;
                    s_id = (string)s_info["s_id"];
                }
                return s_info;
            }
            else
            {
                //String msg = returnValue["msg"].ToString();
                return s_info;
            }
        }

        public string getStudentId()
        {
            return s_id;
        }

        public bool checkStudentStatusIsNew()
        {
            //check applying 
            JObject condi = new JObject();
            condi["sas_s_id"] = s_id;
            condi["sas_type"] = 1;//apply
            JObject returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);
            if ((bool)returnValue["status"])
            {
                return true;
            }
            //no pair teacher
            condi = new JObject();
            condi["p_s_id"] = s_id;
            returnValue = sqlHelper.select("[ntust].[pair]", condi);
            if (!(bool)returnValue["status"])
            {
                return true;
            }
            return false;
        }

        public JToken getApplyResult(string par_s_id = null)
        {
            JArray returnJA = new JArray();
            JObject condi = new JObject();
            condi["sas_s_id"] = par_s_id == null ? s_id : par_s_id;
            condi["sas_type"] = 1;//apply
            JObject returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);
            if (!(bool)returnValue["status"])
            {
                return returnJA;
            }

            condi = new JObject();
            condi["sa_s_id"] = par_s_id == null ? s_id : par_s_id;
            JToken data = returnValue["data"];
            System.Diagnostics.Debug.Print("aa" + data.ToString());
            JObject dataJO = (JObject)data[0];
            condi["sa_tg_id"] = (int)dataJO["sas_tg_id"];
            returnValue = sqlHelper.select("[ntust].[student_apply] sa"
                                    + " LEFT JOIN [ntust].[teacher] t on sa.sa_t_id=t.t_id AND sa.sa_t_type=1"
                                    + " LEFT JOIN [ntust].[extra_teacher] et on sa.sa_t_id=et.t_id AND sa.sa_t_type=2", condi
                                    , select: "(Case when t.t_name IS NULL then et.t_name else t.t_name End) as t_name, sa.sa_state");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }

        }

        public JToken getApplyResultForAdmin(string par_s_id)
        {
            JArray returnJA = new JArray();
            JObject condi = new JObject();
            condi["sas_s_id"] = par_s_id;
            condi["sas_type"] = 1;//apply
            JObject returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);
            if (!(bool)returnValue["status"])
            {
                return returnJA;
            }

            condi = new JObject();
            condi["sa_s_id"] = par_s_id;
            JToken data = returnValue["data"];
            System.Diagnostics.Debug.Print("aa" + data.ToString());
            JObject dataJO = (JObject)data[0];
            condi["sa_tg_id"] = (int)dataJO["sas_tg_id"];
            returnValue = sqlHelper.select("[ntust].[student_apply] sa"
                                    + " JOIN [ntust].[history_student_apply] hsa on hsa.hsa_state = 0 AND hsa.hsa_tg_id = sa.sa_tg_id"//判別申請的老師們拒絕就不引入
                                    + " LEFT JOIN [ntust].[teacher] t on sa.sa_t_id=t.t_id AND sa.sa_t_type=1"
                                    + " LEFT JOIN [ntust].[extra_teacher] et on sa.sa_t_id=et.t_id AND sa.sa_t_type=2", condi
                                    , select: "(Case when t.t_name IS NULL then et.t_name else t.t_name End) tname, (Case when t.t_id IS NULL then et.t_id else t.t_id End) tid, sa.sa_state status, sa.sa_id, sa.sa_tg_id, sa.sa_t_type t_type");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }

        }

        public JObject getChangeResultForAdmin(string par_s_id)
        {
            JObject returnJO = new JObject();
            returnJO["old"] = new JArray();
            returnJO["new"] = new JArray();
            returnJO["sc_all_approval"] = "0";
            JObject condi = new JObject();
            condi["sas_s_id"] = par_s_id;
            condi["sas_type"] = 2;//change
            JObject returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);
            if (!(bool)returnValue["status"])
            {
                return returnJO;
            }

            JToken data = returnValue["data"];
            JObject dataJO = (JObject)data[0];
            int sc_tg_id = (int)dataJO["sas_tg_id"];

            condi = new JObject();
            condi["p.p_s_id"] = par_s_id;
            condi["p.p_end_date"] = "null";
            returnValue = sqlHelper.select("[ntust].[pair] p"
                                    + " JOIN [ntust].[student_change_origin_teacher_approval] scota on scota.scota_tg_id=p.p_tg_id"
                                    + " JOIN [ntust].[teacher_group] tg on tg.tg_id=scota.scota_tg_id AND tg.t_id=scota.scota_t_id"
                                    + " LEFT JOIN [ntust].[teacher] t on tg.t_id=t.t_id AND tg.t_type=1"
                                    + " LEFT JOIN [ntust].[extra_teacher] et on tg.t_id=et.t_id AND tg.t_type=2", condi
                                    , select: "tg.tg_id as org_tg_id,'" + sc_tg_id + "' as new_tg_id, (Case when t.t_name IS NULL then et.t_name else t.t_name End) tname" +
                                            ", (Case when t.t_id IS NULL then et.t_id else t.t_id End) tid" +
                                            ", scota.scota_state status, scota.scota_thesis_state, scota.scota_state, tg.t_type t_type");
            if ((bool)returnValue["status"])
            {
                returnJO["old"] = returnValue["data"];
            }

            condi = new JObject();
            condi["sc_s_id"] = par_s_id;
            condi["sc_tg_id"] = sc_tg_id;
            //condi["sc_state"] = sc_tg_id;
            returnValue = sqlHelper.select("[ntust].[student_change] sc"
                                    //+ " JOIN ( SELECT MAX(sc_state) as max_sc_state, sc_tg_id FROM ntust.student_change GROUP BY sc_tg_id ) check_any_reject on check_any_reject.sc_tg_id = sc.sc_tg_id AND check_any_reject.max_sc_state NOT IN (2,3)"//判別申請的老師們拒絕就不引入
                                    + " JOIN [ntust].[history_student_change] hsc on hsc.hsc_state = 0 AND hsc.hsc_tg_id = sc.sc_tg_id"//判別申請的老師們拒絕就不引入
                                    + " JOIN [ntust].[pair] p on p.p_end_date is null AND sc.sc_s_id=p.p_s_id"
                                    + " LEFT JOIN [ntust].[teacher] t on sc.sc_t_id=t.t_id AND sc.sc_t_type=1"
                                    + " LEFT JOIN [ntust].[extra_teacher] et on sc.sc_t_id=et.t_id AND sc.sc_t_type=2", condi
                                    , select: "(Case when t.t_name IS NULL then et.t_name else t.t_name End) as tname" +
                                            ", (Case when t.t_id IS NULL then et.t_id else t.t_id End) tid, sc.sc_state status" +
                                            ", sc.sc_id, sc.sc_t_type t_type, sc.sc_all_approval, sc.sc_tg_id new_tg_id, p.p_tg_id org_tg_id");
            if ((bool)returnValue["status"])
            {
                returnJO["new"] = returnValue["data"];
            }
            returnJO["sc_all_approval"] = returnValue["data"][0]["sc_all_approval"];

            return returnJO;
        }

        public JToken getChangeResult(string par_s_id = null)
        {
            JArray returnJA = new JArray();
            JObject condi = new JObject();
            condi["sas_s_id"] = par_s_id == null ? s_id : par_s_id;
            condi["sas_type"] = 2;//change
            JObject returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);
            if (!(bool)returnValue["status"])
            {
                return returnJA;
            }

            condi = new JObject();
            condi["sc_s_id"] = par_s_id == null ? s_id : par_s_id;
            JToken data = returnValue["data"];
            System.Diagnostics.Debug.Print("bb" + data.ToString());
            JObject dataJO = (JObject)data[0];
            condi["sc_tg_id"] = (int)dataJO["sas_tg_id"];
            returnValue = sqlHelper.select("[ntust].[student_change] sc"
                                    + " LEFT JOIN [ntust].[teacher] t on sc.sc_t_id=t.t_id AND sc.sc_t_type=1"
                                    + " LEFT JOIN [ntust].[extra_teacher] et on sc.sc_t_id=et.t_id AND sc.sc_t_type=2", condi
                                    , select: "(Case when t.t_name IS NULL then et.t_name else t.t_name End) as t_name, sc.sc_state");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }

        }

        public JToken getChangeHistory()
        {
            JArray returnJA = new JArray();
            JObject condi = new JObject();
            JObject returnValue = new JObject();

            condi["hsc_s_id"] = s_id;
            returnValue = sqlHelper.select("[ntust].[history_student_change] hsc", condi
                                    , select: "hsc.hsc_create_datetime, hsc.hsc_end_datetime, hsc.hsc_state" +
                                            ", STUFF((SELECT  ', ' + new_t.t_name " +
                                                "FROM ntust.teacher_group as new_tg " +
                                                "join ntust.teacher as new_t on new_t.t_id = new_tg.t_id " +
                                                "WHERE hsc.hsc_tg_id = new_tg.tg_id " +
                                                "FOR XML PATH('')),1,1,'') AS t_name" +
                                            ", STUFF((SELECT  ', ' + ori_t.t_name " +
                                                "FROM ntust.teacher_group as ori_tg " +
                                                "join ntust.teacher as ori_t on ori_t.t_id = ori_tg.t_id " +
                                                "WHERE hsc.hsc_origin_tg_id = ori_tg.tg_id " +
                                                "FOR XML PATH('')),1,1,'') AS ori_t_name");//  t.t_name, ori_t_name
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }

        }

        public JToken getPairTeacher(string para_s_id = null)
        {
            JArray returnJA = new JArray();
            JObject condi = new JObject();
            JObject returnValue = new JObject();

            condi["p_s_id"] = para_s_id == null ? s_id : para_s_id;
            condi["p_end_date"] = "null";
            returnValue = sqlHelper.select("[ntust].[pair] p"
                                    + " JOIN [ntust].[teacher_group] tg on tg.tg_id=p.p_tg_id"
                                    + " LEFT JOIN [ntust].[teacher] t on t.t_id=tg.t_id", condi
                                    , select: "t.t_name, t.t_department");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }

        }

        //Request.Form["main"], Request.Form["sub"]
        public JObject studentApply(string main, JToken sub, string para_s_id = null)
        {
            //JObject insertData = new JObject();
            JArray insertData = new JArray();
            JArray insertData2 = new JArray();
            SQLHelper sqlHelper = new SQLHelper();

            string s_id_value = para_s_id == null ? s_id : para_s_id;

            int tgId = getNewTgId();
            /*insertData["sa_s_id"] = s_id;
            insertData["sa_t_id"] = main;
            insertData["sa_t_type"] = 1;
            insertData["sa_tg_id"] = tgId;
            insertData["sa_state"] = 0;
            insertData["sa_create_by_type"] = 1;
            insertData["sa_create_by_st_id"] = "";
            JObject returnValue = sqlHelper.insert("[ntust].[student_apply]", insertData);*/

            JObject insertObj = new JObject();
            JObject insertObj2 = new JObject();
            insertObj["sa_s_id"] = s_id_value;
            insertObj["sa_t_id"] = main;
            insertObj["sa_t_type"] = 1;
            insertObj["sa_tg_id"] = tgId;
            insertObj["sa_state"] = 0;
            insertObj["sa_create_by_type"] = 1;
            insertObj["sa_create_by_st_id"] = "";
            insertData.Add(insertObj);

            insertObj2["tg_id"] = tgId;
            insertObj2["t_id"] = main;
            insertObj2["t_type"] = 1;
            insertObj2["t_order"] = 1;
            insertData2.Add(insertObj2);

            foreach (var x in sub)
            {
                JObject tmp = (JObject)x;
                insertObj = new JObject();
                insertObj["sa_s_id"] = s_id_value;
                insertObj["sa_t_id"] = tmp["t_id"];
                insertObj["sa_t_type"] = tmp["t_type"];
                insertObj["sa_tg_id"] = tgId;
                insertObj["sa_state"] = 0;
                insertObj["sa_create_by_type"] = 1;
                insertObj["sa_create_by_st_id"] = "";
                insertData.Add(insertObj);

                insertObj2 = new JObject();
                insertObj2["tg_id"] = tgId;
                insertObj2["t_id"] = tmp["t_id"];
                insertObj2["t_type"] = tmp["t_type"];
                insertObj2["t_order"] = 2;
                insertData2.Add(insertObj2);
            }

            JObject returnValue = sqlHelper.insertMulti("[ntust].[student_apply]", insertData);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            returnValue = sqlHelper.insertMulti("[ntust].[teacher_group]", insertData2);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            DateTime myDateTime = DateTime.Now;
            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //檢查有沒有未完成的申請
            String query = "UPDATE ntust.history_student_apply set hsa_state=2, hsa_end_datetime='" + sqlFormattedDate + "' WHERE hsa_s_id = '" + s_id_value + "' AND hsa_state=0";
            returnValue = sqlHelper.update(query);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            insertObj = new JObject();
            insertObj["hsa_s_id"] = s_id_value;
            insertObj["hsa_tg_id"] = tgId;
            insertObj["hsa_create_datetime"] = sqlFormattedDate;
            insertObj["hsa_state"] = 0;
            returnValue = sqlHelper.insert("[ntust].[history_student_apply]", insertObj);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }


            //check student_apply_status exist
            JObject condi = new JObject();
            condi["sas_s_id"] = s_id_value;
            returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);

            if ((bool)returnValue["status"])
            {
                query = "UPDATE ntust.student_apply_status set sas_tg_id=" + tgId + " ,sas_type=1 WHERE sas_s_id = '" + s_id_value + "'";
                returnValue = sqlHelper.update(query);
            }
            else
            {
                JObject insertData3 = new JObject();
                insertData3["sas_s_id"] = s_id_value;
                insertData3["sas_type"] = 1;
                insertData3["sas_tg_id"] = tgId;
                returnValue = sqlHelper.insert("[ntust].[student_apply_status]", insertData3);
            }




            //returnValue["sub"] = sub;
            //returnValue["insertData"] = insertData;

            /*if ((bool)returnValue["status"])
            {
                return new HttpStatusCodeResult(200);
            }
            else
            {
                String msg = returnValue["msg"].ToString();
                switch (msg) {
                    case "a":
                        break;
                }
                return new HttpStatusCodeResult(404);
            }*/
            return returnValue;
        }

        //Request.Form["main"], Request.Form["sub"]
        public JObject studentChange(string main, JToken sub, string para_s_id = null)
        {
            //JObject insertData = new JObject();
            JArray insertData = new JArray();
            JArray insertData2 = new JArray();
            SQLHelper sqlHelper = new SQLHelper();

            string s_id_value = para_s_id == null ? s_id : para_s_id;

            int tgId = getNewTgId();
            int oriTgId = getOriTgId(s_id_value);

            if (oriTgId == 0)
            {
                JObject callback = new JObject();
                callback["status"] = false;
                callback["msg"] = "get ori tg id is fail";
                return callback;
            }
            /*insertData["sa_s_id"] = s_id;
            insertData["sa_t_id"] = main;
            insertData["sa_t_type"] = 1;
            insertData["sa_tg_id"] = tgId;
            insertData["sa_state"] = 0;
            insertData["sa_create_by_type"] = 1;
            insertData["sa_create_by_st_id"] = "";
            JObject returnValue = sqlHelper.insert("[ntust].[student_apply]", insertData);*/

            JObject insertObj = new JObject();
            JObject insertObj2 = new JObject();
            insertObj["sc_s_id"] = s_id_value;
            insertObj["sc_t_id"] = main;
            insertObj["sc_t_type"] = 1;
            insertObj["sc_tg_id"] = tgId;
            insertObj["sc_state"] = 0;
            insertObj["sc_create_by_type"] = 1;
            insertObj["sc_create_by_st_id"] = "";
            insertObj["sc_all_approval"] = 0;
            insertData.Add(insertObj);

            insertObj2["tg_id"] = tgId;
            insertObj2["t_id"] = main;
            insertObj2["t_type"] = 1;
            insertObj2["t_order"] = 1;
            insertData2.Add(insertObj2);

            foreach (var x in sub)
            {
                JObject tmp = (JObject)x;
                insertObj = new JObject();
                insertObj["sc_s_id"] = s_id_value;
                insertObj["sc_t_id"] = tmp["t_id"];
                insertObj["sc_t_type"] = tmp["t_type"];
                insertObj["sc_tg_id"] = tgId;
                insertObj["sc_state"] = 0;
                insertObj["sc_create_by_type"] = 1;
                insertObj["sc_create_by_st_id"] = "";
                insertObj["sc_all_approval"] = 0;
                insertData.Add(insertObj);

                insertObj2 = new JObject();
                insertObj2["tg_id"] = tgId;
                insertObj2["t_id"] = tmp["t_id"];
                insertObj2["t_type"] = tmp["t_type"];
                insertObj2["t_order"] = 2;
                insertData2.Add(insertObj2);
            }

            JObject returnValue = sqlHelper.insertMulti("[ntust].[student_change]", insertData);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            returnValue = sqlHelper.insertMulti("[ntust].[teacher_group]", insertData2);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            //刪除同樣這origin teacher group
            JObject condi = new JObject();
            condi["scota_tg_id"] = oriTgId.ToString();
            returnValue = sqlHelper.delete("ntust.student_change_origin_teacher_approval", condi);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            //新增新的student change 
            String query = "INSERT INTO ntust.student_change_origin_teacher_approval(scota_tg_id, scota_t_id, scota_thesis_state, scota_state, scota_create_by_type)" +
                            " SELECT tg.tg_id, tg.t_id, 0, 0, 1" +
                            " FROM ntust.teacher_group AS tg WHERE tg.tg_id = " + oriTgId.ToString();
            returnValue = sqlHelper.insert(query);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            DateTime myDateTime = DateTime.Now;
            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //檢查有沒有未完成的異動
            query = "UPDATE ntust.history_student_change set hsc_state=2, hsc_end_datetime='" + sqlFormattedDate + "' WHERE hsc_s_id = '" + s_id_value + "' AND hsc_state=0";
            returnValue = sqlHelper.update(query);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            insertObj = new JObject();
            insertObj["hsc_s_id"] = s_id_value;
            insertObj["hsc_origin_tg_id"] = oriTgId;
            insertObj["hsc_tg_id"] = tgId;
            insertObj["hsc_create_datetime"] = sqlFormattedDate;
            insertObj["hsc_state"] = 0;
            returnValue = sqlHelper.insert("[ntust].[history_student_change]", insertObj);
            if (!(bool)returnValue["status"])
            {
                return returnValue;
            }

            //check student_apply_status exist
            condi = new JObject();
            condi["sas_s_id"] = s_id_value;
            returnValue = sqlHelper.select("[ntust].[student_apply_status]", condi);
            if ((bool)returnValue["status"])
            {
                query = "UPDATE ntust.student_apply_status set sas_tg_id=" + tgId + " ,sas_type=2 WHERE sas_s_id = '" + s_id_value + "'";
                returnValue = sqlHelper.update(query);
            }
            else
            {
                JObject insertData3 = new JObject();
                insertData3["sas_s_id"] = s_id_value;
                insertData3["sas_type"] = 2;
                insertData3["sas_tg_id"] = tgId;
                returnValue = sqlHelper.insert("[ntust].[student_apply_status]", insertData3);
            }




            //returnValue["sub"] = sub;
            //returnValue["insertData"] = insertData;

            /*if ((bool)returnValue["status"])
            {
                return new HttpStatusCodeResult(200);
            }
            else
            {
                String msg = returnValue["msg"].ToString();
                switch (msg) {
                    case "a":
                        break;
                }
                return new HttpStatusCodeResult(404);
            }*/
            return returnValue;
        }



        public int getNewTgId()
        {
            SQLHelper sqlHelper = new SQLHelper();
            JObject returnValue = sqlHelper.select("[ntust].[teacher_group]", new JObject(), "ORDER BY tg_id DESC ", "top 1 tg_id as max");
            if ((bool)returnValue["status"])
            {
                JToken data = returnValue["data"];
                JObject dataJO = (JObject)data[0];

                System.Diagnostics.Debug.Print(data.ToString());
                System.Diagnostics.Debug.Print("MAX=" + dataJO["max"].ToString());

                return (int)dataJO["max"] + 1;
                //return dataJO.ToString();
            }
            else
            {
                return 1;
            }
        }


        public int getOriTgId(string para_s_id = null)
        {
            SQLHelper sqlHelper = new SQLHelper();
            JObject condi = new JObject();
            condi["p_s_id"] = para_s_id == null ? s_id : para_s_id;
            JObject returnValue = sqlHelper.select("[ntust].[pair]", condi, "ORDER BY p_tg_id DESC ", "top 1 p_tg_id as max");
            if ((bool)returnValue["status"])
            {
                JToken data = returnValue["data"];
                JObject dataJO = (JObject)data[0];

                System.Diagnostics.Debug.Print(data.ToString());
                System.Diagnostics.Debug.Print("MAX=" + dataJO["max"].ToString());

                return (int)dataJO["max"];
                //return dataJO.ToString();
            }
            else
            {
                return 0;
            }
        }


    }
    
}
 