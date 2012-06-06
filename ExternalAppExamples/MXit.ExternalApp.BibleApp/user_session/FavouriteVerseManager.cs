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
    public class FavouriteVerseManager
    {
        LinkedList<FavouriteVerseRecord> favourite_list = new LinkedList<FavouriteVerseRecord>();
        public FavouriteVerseManager(
            UserProfile user_profile,
            UserSession user_session)
        {
            loadFavouriteVersesFromDB(user_profile, user_session);//load favourite verses at start. 
        }

        private void loadFavouriteVersesFromDB(
            UserProfile user_profile,
            UserSession user_session)
        {
            string sqlQuery = 
            "SELECT id, user_id, session_id, datetime, start_verse, end_verse " +
            " FROM favouriteverses WHERE user_id = '" + user_profile.id +"'" +
            " ORDER BY id desc LIMIT 0," + FAVOURITE_VERSES_LIST_MAX_SIZE;
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
                    FavouriteVerseRecord vhr = new FavouriteVerseRecord(
                        id,
                        user_id,
                        session_id,
                        datetime,
                        start_verse,
                        end_verse);
                    favourite_list.AddLast(vhr);
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

        public ReadOnlyCollection<FavouriteVerseRecord> getFavouriteListForDisplay()
        {
            ReadOnlyCollection<FavouriteVerseRecord> readOnly = new ReadOnlyCollection<FavouriteVerseRecord>(
                new List<FavouriteVerseRecord>(favourite_list));
            return readOnly;

        }

        /*
         * this stores the favourite verse. this shouldnt be called if the list is full...the user should first delete 
         * a favourite verse and then add this. 
         */
        public int saveFavouriteVerse(
            UserSession user_session,
            UserProfile user_profile,
            Verse start_verse,
            Verse end_verse)
        {
            if (favourite_list.Count() >= FAVOURITE_VERSES_LIST_MAX_SIZE)
            {
                return FAVOURITE_VERSE_LIST_FULL;
            }
           
            foreach (var fvr in favourite_list)
            {
                if (fvr.isEqual(start_verse.getVerseReference(), end_verse.getVerseReference()))
                    return FAVOURITE_ALREADY_ADDED;
            }
            DateTime dt = DateTime.Now;
            //update DB
            long id = saveFavouriteVerseToDB(
                user_session,
                user_profile,
                start_verse,
                end_verse,
                dt);

            String verse_start_str = start_verse.getVerseReference();
            String verse_end_str;

            if (end_verse == null)
                verse_end_str = "NULL";
            else
                verse_end_str = end_verse.getVerseReference();

            //now update in Memory View. 
            FavouriteVerseRecord vhr = new FavouriteVerseRecord(
                id,
                user_profile.id,
                user_session.session_id,
                dt,
                verse_start_str,
                verse_end_str);

            favourite_list.AddFirst(vhr);
            return FAVOURITE_VERSE_ADDED_SUCCCESS;
        }

        public int deleteFavouriteVerse(
            UserSession us, 
            long favourite_verse_id)
        {
            foreach (var fvr in favourite_list)
            {
                if (fvr.id == favourite_verse_id)
                {
                    favourite_list.Remove(fvr);
                    deleteFavouriteVerseFromDB(us, favourite_verse_id);
                    return FAVOURITE_VERSE_REMOVED_SUCCESS;
                }
            }
            return FAVOURITE_VERSE_REMOVAL_NOT_FOUND;
        }

        public Boolean isFavouriteListFull()
        {
            if (favourite_list.Count() >= FAVOURITE_VERSES_LIST_MAX_SIZE)
            {
                return true;
            }
            return false;
        }


        /*this gets a list of the top favourite list verses of all time. Note this is not tied
         * to the user's session
         */
        public static List<TopFavouriteVerseRecord> getTopFavouriteList()
        {
            List<TopFavouriteVerseRecord> list = loadTopFavouriteVersesFromDB();
            return list;
        }

        private static List<TopFavouriteVerseRecord> loadTopFavouriteVersesFromDB()
        {
            string sqlQuery =
            "SELECT start_verse, end_verse, count(start_verse) as favourite_count FROM favouriteverses WHERE start_verse = end_verse" +
            " GROUP BY start_verse, end_verse ORDER BY favourite_count desc LIMIT 0,"
            + TOP_FAVOURITE_VERSES_LIST_MAX_SIZE;
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long top_verse_count;
                String start_verse;
                String end_verse;
                List<TopFavouriteVerseRecord> list = new List<TopFavouriteVerseRecord>();
                while (rdr.Read())
                {
                    start_verse = rdr[0].ToString();
                    end_verse = rdr[1].ToString();
                    top_verse_count = long.Parse((rdr[2]).ToString());
                    TopFavouriteVerseRecord thr = new TopFavouriteVerseRecord(
                        start_verse,
                        end_verse,
                        top_verse_count);
                    list.Add(thr);
                }
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();
                conn.Close();
            }
        }

        /*this gets a list of the top favourite list verses of all time. Note this is not tied
        * to the user's session
        */
        public static List<TopFavouriteVerseRecord> getTopFavouriteSectionsList()
        {
            List<TopFavouriteVerseRecord> list = loadTopFavouriteSectionsFromDB();
            return list;
        }

        private static List<TopFavouriteVerseRecord> loadTopFavouriteSectionsFromDB()
        {
            string sqlQuery =
            "SELECT start_verse, end_verse, count(start_verse) as favourite_count FROM favouriteverses WHERE start_verse <> end_verse" +
            " GROUP BY start_verse, end_verse ORDER BY favourite_count desc LIMIT 0,"
            + TOP_FAVOURITE_VERSES_LIST_MAX_SIZE;
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long top_verse_count;
                String start_verse;
                String end_verse;
                List<TopFavouriteVerseRecord> list = new List<TopFavouriteVerseRecord>();
                while (rdr.Read())
                {
                    start_verse = rdr[0].ToString();
                    end_verse = rdr[1].ToString();
                    top_verse_count = long.Parse((rdr[2]).ToString());
                    TopFavouriteVerseRecord thr = new TopFavouriteVerseRecord(
                        start_verse,
                        end_verse,
                        top_verse_count);
                    list.Add(thr);
                }
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();
                conn.Close();
            }
        }

        /*this method updates the DB, but should not be called from outside. must be called before after the 
         * favourite list has been updated so that in memory data is insync with persistant data.
         */
        private static long saveFavouriteVerseToDB(
            UserSession user_session,
            UserProfile user_profile,
            Verse start,
            Verse end,
            DateTime datetime)
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
                    "INSERT INTO favouriteverses VALUES(NULL,'" + user_profile.id + "','" + user_session.session_id+ "','" +
                      datetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verse_start_str + "','" + verse_end_str + "')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                conn.Close();
            }
            return -1;
        }


        /*deletes the favourite verse that corresponds to the verse id key
         * 
         * TODO check error handling. and probably not neccassary to check user_profile id  
         */
        private static void deleteFavouriteVerseFromDB(
            UserSession user_session,
            long fav_verse_id)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery =
                    "DELETE FROM favouriteverses WHERE user_id = '" + user_session.user_profile.id + "' AND " +
                " id = '" + fav_verse_id + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                cmd.ExecuteNonQuery();
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
      

        public const int FAVOURITE_VERSES_LIST_MAX_SIZE = 15;
        public const int TOP_FAVOURITE_VERSES_LIST_MAX_SIZE = 15;
        
        public const int FAVOURITE_VERSE_ADDED_SUCCCESS = 0;
        public const int FAVOURITE_VERSE_LIST_FULL = 1;
        public const int FAVOURITE_ALREADY_ADDED = 2;

        public const int FAVOURITE_VERSE_REMOVED_SUCCESS = 0;
        public const int FAVOURITE_VERSE_REMOVAL_NOT_FOUND = 1;
    }
}
