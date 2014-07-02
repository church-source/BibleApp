using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace BibleLoader
{
    class DBManager
    {
        public static MySqlConnection getConnection()
        {
            string connStr = "server=localhost;user=root;database=thebible;port=3306;password=;";
            MySqlConnection conn = new MySqlConnection(connStr);
            return conn;
        }

        /*query should only have one result field*/
        public static List<String> getList(string sqlQuery)
        {

            MySqlConnection conn = getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                List<String> list = new List<String>();
                while (rdr.Read())
                {
                    list.Add((String)rdr[0]);
                }
                rdr.Close();
                conn.Close();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
                return null;
            }


        }

        
    }
}
