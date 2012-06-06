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
    public class VerseBookMarkTask
    {
        private UserSession us { get; set; }
        private UserProfile user_profile { get; set; }
        private Verse start { get; set; }
        private Verse end { get; set; }
        private DateTime datetime { get; set; }
        private BookmarkVerseRecord bvr { get; set; }
        
        public VerseBookMarkTask(
            UserSession us, 
            UserProfile user_profile,
            Verse start,
            Verse end,
            DateTime datetime,
            BookmarkVerseRecord bvr)
        {
            this.us = us;
            this.user_profile = user_profile;
            this.start = start;
            this.end = end;
            this.datetime = datetime;
            this.bvr = bvr;
        }

        public void BookMarkVerse()
        {
            Thread t = new Thread(new ThreadStart(BookMarkVerseDB));
            t.Start();
        }

        private void BookMarkVerseDB()
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

                String sqlQuery =
               "INSERT INTO bookmarks VALUES(NULL,'" + user_profile.id + "','" + us.session_id + "','" +
                 datetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verse_start_str + "','" + verse_end_str + "')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                bvr.id = cmd.LastInsertedId;
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

        public void UpdateBookMarkVerse()
        {
            Thread t = new Thread(new ThreadStart(UpdateBookMarkVerseDB));
            t.Start();
        }

        private void UpdateBookMarkVerseDB()
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

                String sqlQuery =
                 "Update bookmarks SET session_id = '" + us.session_id + "'," +
                   "datetime = '" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "',start_verse ='" + verse_start_str + "', end_verse='" + verse_end_str + "' WHERE id = " + bvr.id;
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                //bvr.id = cmd.LastInsertedId;
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