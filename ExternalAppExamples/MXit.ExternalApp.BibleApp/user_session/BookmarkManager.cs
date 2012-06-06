using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class BookmarkManager
    {
        public BookmarkVerseRecord bookmark_verse {get; private set;}
        public BookmarkManager(
            UserProfile user_profile,
            UserSession user_session)
        {
            bookmark_verse = null;
            loadVBookMarkFromDB(user_profile, user_session);//load history at start. 
        }

        private void loadVBookMarkFromDB(
            UserProfile user_profile,
            UserSession user_session)
        {
            string sqlQuery =
            "SELECT id, user_id, session_id, datetime, start_verse, end_verse " +
            " FROM bookmarks WHERE user_id = '" + user_profile.id + "'";
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long id = -1;
                long user_id= -1;
                long session_id= -1;
                DateTime datetime;
                String start_verse;
                String end_verse;

                if (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    user_id = long.Parse((rdr[1]).ToString());
                    session_id = long.Parse((rdr[2]).ToString());
                    datetime = DateTime.Parse(rdr[3].ToString());
                    start_verse = rdr[4].ToString();
                    end_verse = rdr[5].ToString();
                    bookmark_verse = new BookmarkVerseRecord(
                        id,
                        user_id,
                        session_id,
                        datetime,
                        start_verse,
                        end_verse);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();
                conn.Close();
            }
        }

        /*this stores the bookmark  and is responsible for maintaining synchronisation between memory
         * and DB views by also calling the save/update to db method. 
         */
        public int saveOrUpdateBookmark(
            UserSession user_session,
            UserProfile user_profile,
            Verse start_verse,
            Verse end_verse)
        {
            DateTime dt = DateTime.Now;
            //update DB
            
            String verse_start_str = start_verse.getVerseReference();
            String verse_end_str;

            if (end_verse == null)
                verse_end_str = "NULL";
            else
                verse_end_str = end_verse.getVerseReference();

            //now update in Memory View. 
            long b_id = -1;
            bool isNew = false;
            if (bookmark_verse != null)
            {
                b_id = bookmark_verse.id;
            }
            else
            {
                isNew = true;
            }
            bookmark_verse = new BookmarkVerseRecord(
                    b_id,
                    user_profile.id,
                    user_session.session_id,
                    dt,
                    verse_start_str,
                    verse_end_str);

            saveOrUpdateVerseRequestToDB(
                user_session,
                user_profile,
                start_verse,
                end_verse,
                dt,
                bookmark_verse,
                isNew);


            return 0;
        }




        /*this method updates the DB, but should not be called from outside. must be called before after the 
         * history list has been updated so that in memory data is insync with persistant data.
         */
        private void saveOrUpdateVerseRequestToDB(
            UserSession user_session,
            UserProfile user_profile,
            Verse start,
            Verse end,
            DateTime datetime,
            BookmarkVerseRecord bvr,
            bool isNew)
        {
                VerseBookMarkTask vbmt = new VerseBookMarkTask(
                    user_session,
                    user_profile,
                    start,
                    end,
                    datetime,
                    bvr);
                if (isNew)
                {
                    vbmt.BookMarkVerse();
                }
                else
                {
                    vbmt.UpdateBookMarkVerse();
                }
        }

        //gets bookmark id, and if it doesnt exist returns -1
        public long getBookMarkID(String user_id)
        {
            string sqlQuery = "SELECT id FROM bookmarks WHERE user_id = '" + user_id + "'";
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
    }
}
