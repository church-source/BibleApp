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
    public class VerseHistoryTask
    {
        private UserSession us { get; set; }
        private UserProfile user_profile { get; set; }
        private Verse start { get; set; }
        private Verse end { get; set; }
        private DateTime datetime { get; set; }
        private VerseHistoryRecord vhr { get; set; }
        public VerseHistoryTask(
            UserSession us, 
            UserProfile user_profile,
            Verse start,
            Verse end,
            DateTime datetime,
            VerseHistoryRecord vhr)
        {
            this.us = us;
            this.user_profile = user_profile;
            this.start = start;
            this.end = end;
            this.datetime = datetime;
            this.vhr = vhr;
        }

        public void AddVerseToVerseHistory()
        {
            Thread t = new Thread(new ThreadStart(AddVerseToVerseHistoryDB));
            t.Start();
        }

        private void AddVerseToVerseHistoryDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                String verse_start_str = start.getVerseReference();
                String verse_end_str;

                if (end == null)
                    verse_end_str = "NULL";
                else
                    verse_end_str = end.getVerseReference();

                string sqlQuery =
                    "INSERT INTO versehistory VALUES(NULL,'" + user_profile.id + "','" + us.session_id + "','" +
                      datetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verse_start_str + "','" + verse_end_str + "',0)";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                vhr.id = cmd.LastInsertedId;
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