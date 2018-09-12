using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace advisorSystem.lib
{

    public class CommonSQL
    {
        SQLHelper sqlHelper = new SQLHelper();

        public JToken getDepartmentList()
        {
            JArray returnJA = new JArray();
            JObject returnValue = sqlHelper.select("[ntust].[teacher]", new JObject(), "GROUP BY t_department", "t_department");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }
        }

        public JToken getTeacherList()
        {
            JArray returnJA = new JArray();
            JObject returnValue = sqlHelper.select("[ntust].[teacher]",new JObject(), "", "t_id, t_name, t_department, t_group");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }
        }

        public JToken getOutSideTeacherUnit()
        {
            JArray returnJA = new JArray();
            JObject returnValue = sqlHelper.select("[ntust].[extra_teacher]", new JObject(), "GROUP BY t_service_units", "t_service_units");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }
        }

        public JToken getOusSideTeacherList()
        {
            JArray returnJA = new JArray();
            JObject returnValue = sqlHelper.select("[ntust].[extra_teacher]", new JObject(), "", "t_id, t_name, t_service_units");
            if ((bool)returnValue["status"])
            {
                return returnValue["data"];
            }
            else
            {
                return returnJA;
            }
        }

    }

    public class SQLHelper
    {
        public static string connString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public SqlConnection cn;

        public JObject queryResult = new JObject();

        public SQLHelper()
        {
            cn = new SqlConnection(connString);
        }

        public JObject select(string table, JObject dataArr, string condi="", string select="*")
        {
            queryResult = new JObject();
            cn = new SqlConnection(connString);
            JArray data = new JArray();
            String whereCondi = "WHERE ";
            foreach (var x in dataArr)
            {
                if((string)x.Value=="null")
                    whereCondi += x.Key + " is " + x.Value + " AND ";
                else
                    whereCondi += x.Key + "='"+x.Value+"' AND ";
            }
            whereCondi = whereCondi.Length==6?"":whereCondi.Substring(0, whereCondi.Length - 4);


            string qs = "SELECT "+select+" FROM "+ table + " "+ whereCondi + " " + condi + ";";

            System.Diagnostics.Debug.Print(qs);

            using (cn)
            {

                //2.開啟資料庫
                cn.Open();
                //3.引用SqlCommand物件
                SqlCommand command = new SqlCommand(qs, cn);

                using (SqlDataReader dr = command.ExecuteReader())
                {
                    if (!dr.HasRows)
                    {
                        dr.Close();
                        cn.Close();
                        queryResult["status"] = false;
                        queryResult["msg"] = "empty";
                        queryResult["data"] = new JArray();
                        System.Diagnostics.Debug.Print(queryResult.ToString());
                        return queryResult;
                    }
                    while (dr.Read())
                    {
                        data.Add(new JObject());
                        for (int i = 0; i < dr.FieldCount; ++i){
                            data[data.Count-1][dr.GetName(i)]= dr[i].ToString();
                        }
                    }
                    dr.Close();
                }
                cn.Close();
            }
            System.Diagnostics.Debug.Print(data.ToString());
            queryResult["status"] = true;
            queryResult["data"] = data;
            return queryResult;

        }

        
        public JObject insertMulti(String table, JArray dataArr)
        {
            cn = new SqlConnection(connString);

            // 將 JSON 字串變成物件
            //JObject obj = JObject.Parse(@"{""Name"": ""Eric""}");
            //obj["Name"] == "Eric"

            // 將物件變成 JSON 字串
            //Person p = new Person();
            //p.Name = "Eric";
            // JsonConvert.SerializeObject(p); ==> { "Name": "Eric" }

            string queryStr = "INSERT INTO " + table + "(";
            string columnStr = "", valueStr = "";
            foreach (var x in (JObject)dataArr[0])
            {
                columnStr += x.Key + ",";
                valueStr += "@" + x.Key + ",";
            }
            columnStr = columnStr.Substring(0, columnStr.Length - 1);
            valueStr = valueStr.Substring(0, valueStr.Length - 1);

            queryStr += columnStr + ") values (" + valueStr + ")";

            System.Diagnostics.Debug.Print(queryStr);
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand(queryStr, cn);

            foreach (var x in dataArr)
            {
                sqlCmd = new SqlCommand(queryStr, cn);
                foreach (var y in (JObject)x)
                {
                    //sqlCmd.Parameters.AddWithValue("@" + y.Key, y.Value.ToString());
                    sqlCmd.Parameters.AddWithValue("@" + y.Key, Convert.ToString(y.Value));
                }
                int modified = (int)sqlCmd.ExecuteNonQuery();
                try
                {
                    if (modified != 1)
                    {
                        queryResult["status"] = false;
                        queryResult["msg"] = "insert fail";
                        return queryResult;
                    }
                }
                catch (SqlException odbcEx)
                {
                    // Handle more specific SqlException exception here.
                    System.Diagnostics.Debug.Print(odbcEx.ToString());

                    JArray errorMessages = new JArray();

                    queryResult["status"] = false;

                    for (int i = 0; i < odbcEx.Errors.Count; i++)
                    {
                        errorMessages.Add(odbcEx.Errors[i].Message);
                        //errorMessages[i] = odbcEx.Errors[i].Message;
                    }

                    queryResult["msg"] = errorMessages;
                    queryResult["code"] = odbcEx.Number;
                    return queryResult;
                }
                catch (Exception ex)
                {
                    // Handle generic ones here.
                    System.Diagnostics.Debug.Print(ex.ToString());

                    queryResult["status"] = false;
                    queryResult["msg"] = ex.ToString();
                    return queryResult;
                }
            }
            
            queryResult["status"] = true;
            return queryResult;
            
        }

        public JObject insert(String table, JObject dataArr, String condi="")
        {
            cn = new SqlConnection(connString);

            // 將 JSON 字串變成物件
            //JObject obj = JObject.Parse(@"{""Name"": ""Eric""}");
            //obj["Name"] == "Eric"

            // 將物件變成 JSON 字串
            //Person p = new Person();
            //p.Name = "Eric";
            // JsonConvert.SerializeObject(p); ==> { "Name": "Eric" }

            string queryStr = "INSERT INTO " + table + "(";
            string columnStr = "", valueStr = "";
            foreach (var x in dataArr) {
                columnStr += x.Key + ",";
                valueStr += "@" + x.Key + ",";
            }
            columnStr = columnStr.Substring(0, columnStr.Length-1);
            valueStr = valueStr.Substring(0, valueStr.Length - 1);

            queryStr += columnStr+") values ("+ valueStr+")"+ condi;

            System.Diagnostics.Debug.Print(queryStr);
            System.Diagnostics.Debug.Print(dataArr.ToString());

            cn.Open();
            SqlCommand sqlCmd = new SqlCommand(queryStr, cn);
            foreach (var x in dataArr)
            {
                sqlCmd.Parameters.AddWithValue("@"+ x.Key, x.Value.ToString());
            }


            try
            {
                int modified = (int)sqlCmd.ExecuteNonQuery();

                //int modified = (int)sqlCmd.ExecuteScalar();

                if (cn.State == System.Data.ConnectionState.Open)
                    cn.Close();

                queryResult["status"] = true;
                return queryResult;
            }
            catch (SqlException odbcEx)
            {
                // Handle more specific SqlException exception here.
                System.Diagnostics.Debug.Print(odbcEx.ToString());

                JArray errorMessages = new JArray();

                queryResult["status"] = false;

                for (int i = 0; i < odbcEx.Errors.Count; i++)
                {
                    errorMessages.Add(odbcEx.Errors[i].Message);
                    //errorMessages[i] = odbcEx.Errors[i].Message;
                }

                queryResult["msg"] = errorMessages;
                queryResult["code"] = odbcEx.Number;
                return queryResult;
            }
            catch (Exception ex)
            {
                // Handle generic ones here.
                System.Diagnostics.Debug.Print(ex.ToString());

                queryResult["status"] = false;
                queryResult["msg"] = ex.ToString();
                return queryResult;
            }

        }
        public JObject insert(String query)
        {
            cn = new SqlConnection(connString);

            cn.Open();
            SqlCommand sqlCmd = new SqlCommand(query, cn);
            try
            {
                int modified = (int)sqlCmd.ExecuteNonQuery();

                //int modified = (int)sqlCmd.ExecuteScalar();

                if (cn.State == System.Data.ConnectionState.Open)
                    cn.Close();

                queryResult["status"] = true;
                return queryResult;
            }
            catch (SqlException odbcEx)
            {
                // Handle more specific SqlException exception here.
                System.Diagnostics.Debug.Print(odbcEx.ToString());

                JArray errorMessages = new JArray();

                queryResult["status"] = false;

                for (int i = 0; i < odbcEx.Errors.Count; i++)
                {
                    errorMessages.Add(odbcEx.Errors[i].Message);
                    //errorMessages[i] = odbcEx.Errors[i].Message;
                }

                queryResult["msg"] = errorMessages;
                queryResult["code"] = odbcEx.Number;
                return queryResult;
            }
            catch (Exception ex)
            {
                // Handle generic ones here.
                System.Diagnostics.Debug.Print(ex.ToString());

                queryResult["status"] = false;
                queryResult["msg"] = ex.ToString();
                return queryResult;
            }

        }

        public JObject update(String queryStr)
        {
            cn = new SqlConnection(connString);
            System.Diagnostics.Debug.Print("update: "+queryStr);
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand(queryStr, cn);
            try
            {
                int modified = (int)sqlCmd.ExecuteNonQuery();

                //int modified = (int)sqlCmd.ExecuteScalar();

                if (cn.State == System.Data.ConnectionState.Open)
                    cn.Close();

                //cn.Close();
                queryResult["status"] = true;
                queryResult["msg_update"] = "update sucess";
                return queryResult;
            }
            catch (SqlException odbcEx)
            {
                // Handle more specific SqlException exception here.
                System.Diagnostics.Debug.Print(odbcEx.ToString());

                JArray errorMessages = new JArray();

                queryResult["status"] = false;

                for (int i = 0; i < odbcEx.Errors.Count; i++)
                {
                    errorMessages.Add(odbcEx.Errors[i].Message);
                    //errorMessages[i] = odbcEx.Errors[i].Message;
                }

                queryResult["msg"] = errorMessages;
                queryResult["code"] = odbcEx.Number;
                return queryResult;
            }
            catch (Exception ex)
            {
                // Handle generic ones here.
                System.Diagnostics.Debug.Print(ex.ToString());

                queryResult["status"] = false;
                queryResult["msg"] = ex.ToString();
                return queryResult;
            }
        }

        public JObject delete(String table, JObject dataArr)
        {

            cn = new SqlConnection(connString);
            string queryStr = "DELETE FROM " + table + "";
            string columnCondi = " WHERE";
            foreach (var x in dataArr)
            {
                columnCondi += " " + x.Key + " = @" + x.Key + " AND";
            }
            columnCondi = columnCondi.Substring(0, columnCondi.Length - 3);
            queryStr += columnCondi;

            System.Diagnostics.Debug.Print("------------");
            System.Diagnostics.Debug.Print(queryStr);

            cn.Open();
            SqlCommand sqlCmd = new SqlCommand(queryStr, cn);

            foreach (var x in dataArr)
            {
                sqlCmd.Parameters.AddWithValue("@" + x.Key, x.Value.ToString());
                System.Diagnostics.Debug.Print(x.Value.ToString());
            }

            System.Diagnostics.Debug.Print("------------");

            try
            {
                int modified = (int)sqlCmd.ExecuteNonQuery();

                //int modified = (int)sqlCmd.ExecuteScalar();

                if (cn.State == System.Data.ConnectionState.Open)
                    cn.Close();

                //cn.Close();
                queryResult["status"] = true;
                return queryResult;
            }
            catch (SqlException odbcEx)
            {
                // Handle more specific SqlException exception here.
                System.Diagnostics.Debug.Print(odbcEx.ToString());

                JArray errorMessages = new JArray();

                queryResult["status"] = false;

                for (int i = 0; i < odbcEx.Errors.Count; i++)
                {
                    errorMessages.Add(odbcEx.Errors[i].Message);
                    //errorMessages[i] = odbcEx.Errors[i].Message;
                }

                queryResult["msg"] = errorMessages;
                queryResult["code"] = odbcEx.Number;
                return queryResult;
            }
            catch (Exception ex)
            {
                // Handle generic ones here.
                System.Diagnostics.Debug.Print(ex.ToString());

                queryResult["status"] = false;
                queryResult["msg"] = ex.ToString();
                return queryResult;
            }

        }
        public JObject select(String queryStr)
        {
            cn = new SqlConnection(connString);
            JArray data = new JArray();
            queryResult["status"] = true;
            System.Diagnostics.Debug.Print(queryStr);
            try
            {
                using (cn)
                {

                    //2.開啟資料庫
                    cn.Open();
                    //3.引用SqlCommand物件
                    SqlCommand command = new SqlCommand(queryStr, cn);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            data.Add(new JObject());
                            for (int i = 0; i < dr.FieldCount; ++i)
                            {
                                data[data.Count - 1][dr.GetName(i)] = dr[i].ToString();
                            }
                        }
                        dr.Close();
                    }
                    cn.Close();
                }

                queryResult["data"] = data;
                return queryResult;
            }
            catch(Exception ex)
            {
                queryResult["status"] = false;
                queryResult["msg"] = ex.Message;
                return queryResult;
            }
            
        }
        public JObject query(String queryStr, JObject dataArray)
        {
            JArray data = new JArray();
            cn = new SqlConnection(connString);
            queryResult = new JObject();
            try
            {
                using (cn)
                {
                    //2.開啟資料庫
                    cn.Open();
                    //3.引用SqlCommand物件
                    SqlCommand command = new SqlCommand(queryStr, cn);
                    foreach (var x in dataArray)
                    {
                        command.Parameters.AddWithValue("@"+x.Key, x.Value.ToString());
                    }
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        if (!dr.HasRows)
                        {
                            dr.Close();
                            cn.Close();
                            queryResult["status"] = true;
                            queryResult["msg"] = "empty";
                            queryResult["data"] = data;
                            System.Diagnostics.Debug.Print(queryResult.ToString());
                            return queryResult;
                        }
                        while (dr.Read())
                        {
                            data.Add(new JObject());
                            for (int i = 0; i < dr.FieldCount; ++i)
                            {
                                data[data.Count - 1][dr.GetName(i)] = dr[i].ToString();
                            }
                        }
                        dr.Close();
                    }
                    cn.Close();

                }

                System.Diagnostics.Debug.Print(data.ToString());
                queryResult["status"] = true;
                queryResult["data"] = data;
                return queryResult;
            }
            catch (Exception ex)
            {
                queryResult["status"] = false;
                queryResult["msg"] = ex.Message;
                return queryResult;
            }
        }
        
    }
}
 