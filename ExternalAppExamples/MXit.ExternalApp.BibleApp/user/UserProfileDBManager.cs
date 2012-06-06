using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    class UserProfileDBManager
    {

        //private static Object thisLock = new Object();
        private static Dictionary<String, long> existing_code_list = new Dictionary<string, long>();//used to get profile id given user code. 
        //fill existing code list. this will be maintained in memory so that we dont have to query the db for this everytime. 
        static UserProfileDBManager()
        {
            string sqlQuery = "SELECT bibleusercode,id FROM userprofile";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                String bibleusercode = "";
                while (rdr.Read())
                {
                    bibleusercode = (rdr[0]).ToString().ToUpper();
                    existing_code_list.Add(bibleusercode, long.Parse((rdr[1]).ToString()));
                }
            }
            finally
            {
                conn.Close();
            }
        }

        /**
         * returns new ID
         * 
        **/
        public static long addUser(
            String user_id,
            UserInfo user_info,
            String user_name,
            String user_code
            )
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                int g =-1;
                if (user_info.Gender.ToString().ToUpper() == "MALE")
                {
                    g = 1;
                }
                else
                {
                    g = 0;
                }

                String display_name = user_info.DisplayName;
    
                Console.WriteLine("Registering the following user: \r\n" +
                    "user_id=>" + user_id +
                    "\r\nuser_name=>" + user_name +
                    "\r\nUser_Info Object: DisplayName => " + display_name +
                    "\r\nUser_Info Object: DateOfBirth => " + user_info.DateOfBirth +
                    "\r\nUser_Info Object: RegisteredCountry => " + user_info.RegisteredCountry +
                    "\r\nUser_Info Object: CurrentCountry => " + user_info.CurrentCountry +
                    "\r\nUser_Info Object: CurrentRegion => " + user_info.CurrentRegion +
                    "\r\nUser_Info Object: CurrentCity => " + user_info.CurrentCity +
                     "\r\nUser_Info Object: UserCode=> " + user_code+
                    "\r\n" +DateTime.Now);
                DateTime datetime =  DateTime.Now;
                
                string sqlQuery =
                    "INSERT INTO userprofile VALUES(NULL,@user_id,'" + user_name + "',@display_name"+
                    ",'"+user_info.DateOfBirth+"','"+user_info.RegisteredCountry+"','"+user_info.CurrentCountry
                    + "','" + user_info.CurrentRegion + "','" + user_info.CurrentCity + "','" + g + "','" + datetime.ToString("yyyy-MM-dd HH:mm:ss") +
                    "','" + UserProfile.DEFAULT_TRANSLATION + "','" + user_code + "',0,1)";

                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                cmd.Parameters.Add("@user_id", MySql.Data.MySqlClient.MySqlDbType.VarChar);
                cmd.Parameters.Add("@display_name", MySql.Data.MySqlClient.MySqlDbType.VarChar);
                cmd.Parameters["@user_id"].Value = user_id;
                cmd.Parameters["@display_name"].Value = display_name;
                int output = cmd.ExecuteNonQuery();
                long id = cmd.LastInsertedId;
                existing_code_list.Add(user_code, id);
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if(conn != null)
                    conn.Close();
            }
            return -1;
        }

        /**
         * returns session ID
         * 
        **/
        public static long logSessionStart(
            long id
            )
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                DateTime datetime = DateTime.Now;
                string sqlQuery =
                    "INSERT INTO usersession VALUES(NULL,'" + id + "','" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "',NULL)";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return -1;
        }

        public static void logSessionEnd(long id, long session_id)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                DateTime datetime = DateTime.Now;
                string sqlQuery =
                    "UPDATE usersession SET datetime_end = '" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE id = '" + id +
                        "' AND session_id ='" + session_id  + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static long getUserID(String user_id)
        {
            string sqlQuery = "SELECT id FROM userprofile WHERE user_id = '" + user_id +"'";
            MySqlConnection conn = DBManager.getConnection();
            MySqlCommand cmd = null;
            MySqlDataReader rdr= null;
            try
            {
                conn.Open();
                cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    return long.Parse((rdr[0]).ToString());                                       
                }

                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                    conn.Close();
            }
        }

        //TODO should rather perhaps load all required info because taking info from UserInfo might not be the best since it might change per request. 
        public static UserProfileCustomData loadCustomUserProfileData(String user_id)
        {
            string sqlQuery = "SELECT id, user_name, default_translation, bibleusercode, theme, subscribed_to_daily_verse FROM userprofile WHERE user_id = '" + user_id + "'";
            MySqlConnection conn = DBManager.getConnection();
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                Boolean is_subscribed = false;
                if (rdr.Read())
                {
                    if ((rdr[5]).ToString().Equals("0"))
                        is_subscribed = false;
                    else
                        is_subscribed = true;
                    
                    return new UserProfileCustomData(
                        long.Parse((rdr[0]).ToString()),
                        (rdr[1]).ToString(),
                        (rdr[2]).ToString(),
                        (rdr[3]).ToString(),
                        Int32.Parse((rdr[4]).ToString()),
                        is_subscribed);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                    conn.Close();
            }
        }

        public static string getDefaultTranslation(long id)
        {
            string sqlQuery = "SELECT default_translation FROM userprofile WHERE id = '" + id + "'";
            MySqlConnection conn = DBManager.getConnection();
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    return (rdr[0]).ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                    conn.Close();
            }
        }

        public static List<long> getListOfUsersWithNullUserCodes()
        {
            string sqlQuery = "SELECT id FROM userprofile WHERE bibleusercode is NULL";
            MySqlConnection conn = DBManager.getConnection();
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            List<long> ids = new List<long>();
            try
            {
                conn.Open();
                cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    ids.Add(long.Parse((rdr[0]).ToString()));
                }

                return ids;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                    conn.Close();
            }
        }

        /**
         * returns new ID
         * 
        **/
        public static void updateDefaultTranslation(
            long id,
            string default_translation
            )
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery =
                    "UPDATE userprofile SET default_translation = '" + default_translation +"' WHERE id = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static void updateColourTheme(
            long id,
            int colour_theme)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery =
                    "UPDATE userprofile SET theme = '" + colour_theme + "' WHERE id = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static void updateDailyVerseSubscrtipion(
            long id,
            Boolean is_subscribed)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery = "";
                if(is_subscribed)
                    sqlQuery =
                        "UPDATE userprofile SET subscribed_to_daily_verse = " + 1 + " WHERE id = '" + id + "'";
                else
                    sqlQuery =
                        "UPDATE userprofile SET subscribed_to_daily_verse = " + 0 + " WHERE id = '" + id + "'";

                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        
        public static long getIDFromUserCode(String usercode)
        {
            if (existing_code_list.ContainsKey(usercode.ToUpper()))
            {
                return existing_code_list[usercode.ToUpper()];
            }
            return -1;
        }

    }
}
