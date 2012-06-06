using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class UserRoleManager
    {
        private static UserRoleManager instance = new UserRoleManager();
        private static Dictionary<long, int> user_role_list= new Dictionary<long, int>();

        //fill existing code list. this will be maintained in memory so that we dont have to query the db for this everytime. 
        static UserRoleManager()
        {
            Console.WriteLine("Loading User Names...");
            string sqlQuery = "SELECT user_id,role FROM userroles";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                int user_role = -1;
                long id = -1;
                while (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    user_role = Int32.Parse((rdr[1]).ToString());
                    user_role_list.Add(id, user_role);
                }
            }
            finally
            {
                conn.Close();
            }
        }



        public static UserRoleManager getInstance()
        {
            return instance;
        }

        public bool isUserAdmin(UserProfile up)
        {
            if (user_role_list.ContainsKey(up.id))
            {
                if (user_role_list[up.id] == USER_ROLE_ADMIN || user_role_list[up.id] == USER_ROLE_SUPER_ADMIN)
                    return true;
            }
            return false;
        }

        public bool isUserMod(UserProfile up)
        {
            if (user_role_list.ContainsKey(up.id))
            {
                if (user_role_list[up.id] == USER_ROLE_MODERATOR)
                    return true;
            }
            return false;
        }

        public const int USER_ROLE_SUPER_ADMIN = 0;
        public const int USER_ROLE_ADMIN = 1;
        public const int USER_ROLE_MODERATOR = 2;

    }
}
