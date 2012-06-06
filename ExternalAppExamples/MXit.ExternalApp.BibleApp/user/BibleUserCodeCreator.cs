using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    class BibleUserCodeCreator
    {
        private static BibleUserCodeCreator instance = new BibleUserCodeCreator();
        private static Dictionary<String, String> existing_code_list = new Dictionary<string, string>();
        private static Object thisLock = new Object();

        //fill existing code list. this will be maintained in memory so that we dont have to query the db for this everytime. 
        static BibleUserCodeCreator()
        {
            string sqlQuery = "SELECT bibleusercode FROM bibleusercodes";
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
                    existing_code_list.Add(bibleusercode, bibleusercode);
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public static BibleUserCodeCreator getInstance()
        {
            return instance;
        }

        public String generateUniqueANRandomCode(int length)
        {
            String randomCode = "";
            lock (thisLock)
            {
                randomCode = generateANRandomCode(length).ToUpper();
                while (existing_code_list.ContainsKey(randomCode))
                {
                    randomCode = generateANRandomCode(length).ToUpper();
                }
                existing_code_list.Add(randomCode, randomCode);
            }
            addRandomCodeToDB(randomCode);
            return randomCode;
        }

        //throws Exception if code is already in db
        private void addRandomCodeToDB(String randomCode)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                DateTime datetime = DateTime.Now;
                string sqlQuery =
                    "INSERT INTO bibleusercodes VALUES('"+randomCode+"')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private String generateANRandomCode(int length)
        {
            /*var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            String result = new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;*/
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new String(stringChars);

        }

        public void generateANRandomCodesForExistingUsers(int length)
        {
            List<long> ids = UserProfileDBManager.getListOfUsersWithNullUserCodes();
            foreach(var id in ids)
            {
                MySqlConnection conn = DBManager.getConnection();
                try
                {
                    conn.Open();
                    String randomCode = BibleUserCodeCreator.getInstance().generateUniqueANRandomCode(length);
                    string sqlQuery =
                        "UPDATE userprofile SET bibleusercode = '" + randomCode + "' WHERE id = '" + id + "'";
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
        }

        public const int CODE_LENGTH = 6;

    }
}
