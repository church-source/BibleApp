using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class SuspensionManager
    {
        private static SuspensionManager instance;
        private static Dictionary<long, SuspendedRecord> suspended_user_list = new Dictionary<long, SuspendedRecord>();
        static SuspensionManager()
        {
            string sqlQuery = "SELECT user_id, datetime_end FROM suspendedusers order by datetime_end";
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long user_id = -1;
                DateTime datetime_sus_end = DateTime.MaxValue;
                while (rdr.Read())
                {
                    user_id = long.Parse((rdr[0]).ToString());
                    datetime_sus_end = DateTime.Parse((rdr[1]).ToString());
                    if(suspended_user_list.ContainsKey(user_id))
                        suspended_user_list.Remove(user_id);
                    suspended_user_list.Add(user_id,new SuspendedRecord(user_id,datetime_sus_end));
                }
                
                rdr.Close();
                conn.Close();

            }
            finally
            {
                if(rdr!= null)
                    rdr.Close();
                if(conn!=null)
                    conn.Close();
            }

        }


        //dont lock here, but be sure to call it in during startup. 
        public static SuspensionManager getInstance()
        {
            if(instance == null)
            {
                 instance = new SuspensionManager();
            }
            return instance; 
        }

        public bool isSuspended(long user_id)
        {
            if (!suspended_user_list.ContainsKey(user_id))
                return false;

            DateTime datetime = suspended_user_list[user_id].datetime_end;

            if (DateTime.Now < datetime)
                return true;
            else 
                return false;
        }
    }
}
