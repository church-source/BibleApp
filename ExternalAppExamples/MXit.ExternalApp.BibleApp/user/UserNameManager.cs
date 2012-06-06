using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    class UserNameManager
    {
        private static UserNameManager instance = new UserNameManager();
        private static Dictionary<String, long> user_name_list = new Dictionary<string, long>();
        private static Dictionary<long, String> user_name_by_id_list= new Dictionary<long, String>();
        private static Object thisLock = new Object();

        //fill existing code list. this will be maintained in memory so that we dont have to query the db for this everytime. 
        static UserNameManager()
        {
            Console.WriteLine("Loading User Names...");
            string sqlQuery = "SELECT id,user_name FROM userprofile";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                String user_name = "";
                long id = -1;
                while (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    user_name = (rdr[1]).ToString().ToUpper();
                    user_name_list.Add(user_name.ToUpper(), id);
                    user_name_by_id_list.Add(id, (rdr[1]).ToString());
                }
            }
            finally
            {
                conn.Close();
            }
        }

        //this check is performed up front to avoid overhead of db Hit, but needs to be done again 
        public Boolean isUserNameUnique(String user_name)
        {
            return !user_name_list.ContainsKey(user_name.ToUpper());
        }

        public static UserNameManager getInstance()
        {
            return instance;
        }

        public void removeNameFromCachedList(String user_name)
        {
            lock(thisLock)
            {
                if(user_name_list.ContainsKey(user_name.ToUpper()))
                {
                    user_name_list.Remove(user_name.ToUpper());
                }
            }
        }

        //check this for sql injection attack, I think parametrized query is enough protection
        public void saveUserNameToDBProfile(long id, String userName)
        {
            lock (thisLock)
            {
                MySqlConnection conn = DBManager.getConnection();
                try
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("UPDATE Userprofile" +
                        " SET user_name = @user_name where id = " + id, conn);

                    cmd.Parameters.Add("@user_name", MySql.Data.MySqlClient.MySqlDbType.Text);

                    cmd.Parameters["@user_name"].Value = userName;
                    cmd.ExecuteNonQuery();

                    if (user_name_by_id_list.ContainsKey(id))
                    {
                        removeNameFromCachedList(user_name_by_id_list[id]);
                        user_name_by_id_list.Remove(id);
                    }
                    user_name_list.Add(userName.ToUpper(), id);   
                    user_name_by_id_list.Add(id, userName);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static String getUserName(long user_id)
        {
            if (user_name_by_id_list.ContainsKey(user_id))
            {
                return user_name_by_id_list[user_id];
            }
            return "NO NAME - ERROR OCCURED";
        }

        public static long getUserID(String user_name)
        {
            if (user_name_list.ContainsKey(user_name.ToUpper()))
            {
                return user_name_list[user_name.ToUpper()];
            }
            return -1;
        }
     
        public const int MAX_USER_NAME_LENGTH = 30;

    }
}
