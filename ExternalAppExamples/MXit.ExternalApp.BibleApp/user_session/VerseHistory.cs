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
    public class VerseHistory
    {
        LinkedList<VerseHistoryRecord> history_list = new LinkedList<VerseHistoryRecord>();
        public VerseHistory(
            UserProfile user_profile,
            UserSession user_session)
        {
            loadVerseHistoryFromDB(user_profile, user_session);//load history at start. 
        }

        private void loadVerseHistoryFromDB(
            UserProfile user_profile,
            UserSession user_session)
        {
            string sqlQuery = 
            "SELECT id, user_id, session_id, datetime, start_verse, end_verse " +
            " FROM versehistory WHERE user_id = '" + user_profile.id +"'" +
            " AND deleted = 0 ORDER BY id desc LIMIT 0," +HISTORY_MAX_SIZE;
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

                while (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    user_id = long.Parse((rdr[1]).ToString());
                    session_id = long.Parse((rdr[2]).ToString());
                    datetime = DateTime.Parse(rdr[3].ToString());
                    start_verse = rdr[4].ToString();
                    end_verse = rdr[5].ToString();
                    VerseHistoryRecord vhr = new VerseHistoryRecord(
                        id,
                        user_id,
                        session_id,
                        datetime,
                        start_verse,
                        end_verse);
                    history_list.AddLast(vhr);
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

        public ReadOnlyCollection<VerseHistoryRecord> getHistoryListForDisplay()
        {
            ReadOnlyCollection<VerseHistoryRecord> readOnly = new ReadOnlyCollection<VerseHistoryRecord>(
                new List<VerseHistoryRecord>(history_list));
            return readOnly;

        }

        /*this stores the last request and is responsible for maintaining synchronisation between memory
         * and DB views. 
         */
        public void saveVerseRequest(
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
            VerseHistoryRecord vhr = new VerseHistoryRecord(
                -1,
                user_profile.id,
                user_session.session_id,
                dt,
                verse_start_str,
                verse_end_str);

            saveVerseRequestToDB(
                user_session,
                user_profile,
                start_verse,
                end_verse,
                dt,
                vhr);


            if (history_list.Count() < HISTORY_MAX_SIZE)
            {
                history_list.AddFirst(vhr);
            }
            else //this should correspond to the list HISTORY_MAX_SIZE size because it should never grow more than this 
            {
                history_list.RemoveLast();
                history_list.AddFirst(vhr);
            }
        }

        /* this clears the user's history
        */
        public void clearHistory(UserProfile user_profile)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();

                string sqlQuery =
                    "UPDATE versehistory SET deleted = 1 WHERE user_id = '" + user_profile.id + "' AND deleted = 0";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                history_list.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                conn.Close();
            }
        }


        /*this method updates the DB in seperate thread. to increase performance. 
         */
        private static void saveVerseRequestToDB(
            UserSession user_session,
            UserProfile user_profile,
            Verse start,
            Verse end,
            DateTime datetime,
            VerseHistoryRecord vhr)
        {
            VerseHistoryTask vht = new VerseHistoryTask(
                user_session,
                user_profile,
                start,
                end,
                datetime,
                vhr);

            vht.AddVerseToVerseHistory();
        }

       
        public const int HISTORY_MAX_SIZE = 15;
    }
}
