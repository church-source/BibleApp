using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;

using System.Threading;

namespace MxitTestApp
{
    public class VerseTagTask
    {
        private long user_id { get; set; }
        private String start_verse { get; set; }
        private String end_verse { get; set; }
        private DateTime datetime { get; set; }
        private VerseTag vt { get; set; }
        private int emotion_id { get; set; }
        private String description { get; set; }

        public VerseTagTask(
            long user_id, 
            int emotion_id,
            String start_verse,
            String end_verse,
            DateTime datetime,
            String description,
            VerseTag vt)
        {
            this.user_id = user_id;
            this.emotion_id = emotion_id;
            this.start_verse = start_verse;
            this.end_verse = end_verse;
            this.datetime = datetime;
            this.vt = vt ;
        }

        public void AddVerseTask()
        {
            Thread t = new Thread(new ThreadStart(AddVerseTaskToDB));
            t.Start();
        }

        private void AddVerseTaskToDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                if (end_verse == null)
                    end_verse = "";
                
                string sqlQuery =
                    "INSERT INTO emotion_tag VALUES(NULL,'" + emotion_id + "','" + user_id + "','" +
                      datetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + start_verse + "','" + end_verse+ "','"+description+"')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                vt.id = cmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                conn.Close();
            }
        }
    }
}