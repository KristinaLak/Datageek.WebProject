using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace DataGeek.BusinessLogic.Common
{
    public static class SQL
    {
        public static String connection_string = System.Configuration.ConfigurationManager.ConnectionStrings["dashboardlocal"].ConnectionString; // for normal dev
        public static String provider_name = System.Configuration.ConfigurationManager.ConnectionStrings["dashboardlocal"].ProviderName;
        //public static String connection_string = System.Configuration.ConfigurationManager.ConnectionStrings["dashboardlocalalt"].ConnectionString; // for alt dev
        //public static String provider_name = System.Configuration.ConfigurationManager.ConnectionStrings["dashboardlocalalt"].ProviderName;

        // Connecting
        public static bool Connect(MySqlConnection con)
        {
            try
            {
                con.Open();
                return true;
            }
            catch { }
            return false;
        }
        public static void Disconnect(MySqlConnection con)
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }

        // Select
        public static String SelectString(String queryString, String returnField, String[] paramNames, Object[] paramVals)
        {
            DataTable dt_qry = SelectDataTable(queryString, paramNames, paramVals);
            if (dt_qry.Rows.Count > 0 && dt_qry.Rows[0][returnField] != DBNull.Value)
                return dt_qry.Rows[0][returnField].ToString();

            return String.Empty;
        }
        public static String SelectString(String queryString, String returnField, String paramName, Object paramVal)
        {
            return SelectString(queryString, returnField, new String[] { paramName }, new Object[] { paramVal });
        }
        public static String SelectString(String queryString, String returnField, Dictionary<String, String> paramDictionary)
        {
            String[] pn = paramDictionary.Keys.ToArray();
            Object[] pv = paramDictionary.Values.ToArray();

            return SelectString(queryString, returnField, pn, pv);
        }
        public static DataTable SelectDataTable(String queryString, String[] paramNames, Object[] paramVals)
        {
            if (String.IsNullOrEmpty(queryString))
                throw new Exception("MySQL Error: Query string cannot be null or empty.");

            DataTable dt_qry = new DataTable();
            using (MySqlConnection mysql_con = new MySqlConnection(connection_string))
            {
                if (Connect(mysql_con))
                {
                    try
                    {
                        MySqlCommand sm = new MySqlCommand(queryString, mysql_con);

                        if (paramNames != null && paramVals != null)
                        {
                            if (paramNames.Length != paramVals.Length)
                                throw new Exception("MySQL Error: Parameter arrays differ in length.");

                            for (int i = 0; i < paramNames.Length; i++)
                                sm.Parameters.AddWithValue(paramNames[i], paramVals[i]);
                        }

                        MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                        sa.Fill(dt_qry);
                    }
                    catch// (Exception r)
                    {
                        //StackTrace s = new StackTrace();
                        //Util.Debug("(" + DateTime.Now.Date + ") Select error called from method " + s.GetFrame(1).GetMethod().Name + ": " 
                        //    + Environment.NewLine + r.Message + " " + r.InnerException);
                        throw;
                    }
                    finally
                    {
                        Disconnect(mysql_con);
                    }
                }
            }
            return dt_qry;
        }
        public static DataTable SelectDataTable(String queryString, String paramName, Object paramVal)
        {
            return SelectDataTable(queryString, new String[] { paramName }, new Object[] { paramVal });
        }
        public static DataTable SelectDataTable(String queryString, Dictionary<String, String> paramDictionary)
        {
            String[] pn = paramDictionary.Keys.ToArray();
            Object[] pv = paramDictionary.Values.ToArray();

            return SelectDataTable(queryString, pn, pv);
        }

        // Update
        public static void Update(String queryString, String[] paramNames, Object[] paramVals)
        {
            NonSelectQuery(queryString, paramNames, paramVals, "Update");
        }
        public static void Update(String queryString, String paramName, Object paramVal)
        {
            Update(queryString, new String[] { paramName }, new Object[] { paramVal });
        }
        public static void Update(String queryString, Dictionary<String, String> paramDictionary)
        {
            String[] pn = paramDictionary.Keys.ToArray();
            Object[] pv = paramDictionary.Values.ToArray();

            Update(queryString, pn, pv);
        }

        // Insert
        public static long Insert(String queryString, String[] paramNames, Object[] paramVals)
        {
            return NonSelectQuery(queryString, paramNames, paramVals, "Insert");
        }
        public static long Insert(String queryString, String paramName, Object paramVal)
        {
            return Insert(queryString, new String[] { paramName }, new Object[] { paramVal });
        }
        public static long Insert(String queryString, Dictionary<String, String> paramDictionary)
        {
            String[] pn = paramDictionary.Keys.ToArray();
            Object[] pv = paramDictionary.Values.ToArray();

            return Insert(queryString, pn, pv);
        }

        // Delete
        public static void Delete(String queryString, String[] paramNames, Object[] paramVals)
        {
            NonSelectQuery(queryString, paramNames, paramVals, "Delete");
        }
        public static void Delete(String queryString, String paramName, Object paramVal)
        {
            Delete(queryString, new String[] { paramName }, new Object[] { paramVal });
        }
        public static void Delete(String queryString, Dictionary<String, String> paramDictionary)
        {
            String[] pn = paramDictionary.Keys.ToArray();
            Object[] pv = paramDictionary.Values.ToArray();

            Delete(queryString, pn, pv);
        }

        private static long NonSelectQuery(String queryString, String[] paramNames, Object[] paramVals, String queryType)
        {
            long last_insert_id = -1;
            if (String.IsNullOrEmpty(queryString))
                throw new Exception("MySQL Error: Query string cannot be null or empty.");

            using (MySqlConnection mysql_con = new MySqlConnection(connection_string))
            {
                if (Connect(mysql_con))
                {
                    try
                    {
                        MySqlCommand c = mysql_con.CreateCommand();
                        c.CommandText = queryString;
                        if (paramNames != null && paramVals != null)
                        {
                            if (paramNames.Length != paramVals.Length)
                                throw new Exception("MySQL Error: Parameter arrays differ in length.");

                            for (int i = 0; i < paramNames.Length; i++)
                                c.Parameters.AddWithValue(paramNames[i], paramVals[i]);
                        }
                        c.ExecuteNonQuery();
                        last_insert_id = c.LastInsertedId;
                    }
                    catch// (Exception r)
                    {
                        //StackTrace s = new StackTrace();
                        //Util.Debug("(" + DateTime.Now.Date + ") Non select error called from method " + s.GetFrame(1).GetMethod().Name + ": "
                        //    + Environment.NewLine + r.Message + " " + r.InnerException);
                        throw;
                    }
                    finally
                    {
                        Disconnect(mysql_con);
                    }
                }
            }
            return last_insert_id;
        }

        // Show original query
        public static String ShowQuery(String queryString, String[] paramNames, Object[] paramVals)
        {
            if (paramNames != null && paramVals != null)
            {
                String[] paramValsString = paramVals.Select(o => o == null ? (String)null : o.ToString()).ToArray();
                Dictionary<String, String> paramDictionary = paramNames.Zip(paramValsString, (first, second) => new { first, second }).ToDictionary(val => val.first, val => val.second);

                foreach (KeyValuePair<String, String> param in paramDictionary.OrderByDescending(x => x.Key.Length))
                {
                    String pn = param.Key;
                    String pv = param.Value;

                    String p;
                    if (pv == null)
                        p = "NULL";
                    else
                        p = "'" + pv + "'";

                    queryString = queryString.Replace(pn, p);
                }
            }

            Util.Debug(queryString);

            return queryString;
        }
        public static String ShowQuery(String queryString, String paramName, Object paramVal)
        {
            return ShowQuery(queryString, new String[] { paramName }, new Object[] { paramVal });
        }
        public static String ShowQuery(String queryString, Dictionary<String, String> paramDictionary)
        {
            String[] pn = paramDictionary.Keys.ToArray();
            Object[] pv = paramDictionary.Values.ToArray();

            return ShowQuery(queryString, pn, pv);
        }
    }
}