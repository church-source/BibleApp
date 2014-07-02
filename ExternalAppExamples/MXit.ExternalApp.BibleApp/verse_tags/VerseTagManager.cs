using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;

using System.Threading;

namespace MxitTestApp
{
    public class VerseTagManager
    {
        private Object thisLock = new Object();
        private static VerseTagManager instance = new VerseTagManager();

        private static Dictionary<int, VerseTagEmotion> emotions = new Dictionary<int, VerseTagEmotion>();
        private static Dictionary<String, List<VerseTag>> verses_emotions = new Dictionary<String, List<VerseTag>>();
        private static Dictionary<int, List<VerseTag>> emotion_verses = new Dictionary<int, List<VerseTag>>();

        //probably a waste of memory...
        private static Dictionary<String, String> unique_check_map = new Dictionary<String, String>();

        static VerseTagManager()
        {
            loadEmotions();
            loadVerseTags();
        }

        private VerseTagManager()
        {
            //do nothing.
        }

        public static VerseTagManager getInstance()
        {
            return instance;
        }

        public int getVerseTagCountOnEmotion(int emotion_id)
        {
            if (emotion_verses.ContainsKey(emotion_id))
                return emotion_verses[emotion_id].Count;

            return 0;
        }

        public static void loadEmotions()
        {
            string sqlQuery = "SELECT id, emotion FROM emotions ORDER BY id";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                int id = -1;
                String emotion = "";
                while (rdr.Read())
                {
                    id = Int32.Parse((rdr[0]).ToString());
                    emotion = (rdr[1]).ToString();
                    emotions.Add(id, new VerseTagEmotion(id, emotion));
                }
                rdr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static void loadVerseTags()
        {
            string sqlQuery = "SELECT id, emotion_id, user_id, datetime, start_verse, end_verse, description FROM emotion_tag";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                long id = -1;
                int emotion_id;
                long user_id;
                DateTime datetime;
                String start_verse;
                String end_verse;
                String description;
                while (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    emotion_id = Int32.Parse((rdr[1]).ToString());
                    user_id = long.Parse((rdr[2]).ToString());
                    datetime = DateTime.Parse(rdr[3].ToString());
                    start_verse = rdr[4].ToString();
                    end_verse = rdr[5].ToString();
                    description = rdr[6].ToString();
                    String key = start_verse + "|" + end_verse;
                    VerseTag vt = new VerseTag(
                            id,
                            emotion_id,
                            user_id,
                            datetime,
                            start_verse,
                            end_verse,
                            description);
                    addVerseTagDuringLoad(vt);
                }
                rdr.Close();
                conn.Close();
                loadVerseTagLikes();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static void loadVerseTagLikes()
        {
            string sqlQuery = "SELECT etl.id, emo_tag_id, etl.user_id, etl.datetime, emotion_id  from emotion_tag_likes etl " + 
                               " INNER JOIN emotion_tag et ON etl.emo_tag_id = et.id";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                long id = -1;
                int emotion_tag_id;
                long user_id;
                DateTime datetime;
                int emotion;
                while (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    emotion_tag_id = Int32.Parse((rdr[1]).ToString());
                    user_id = long.Parse((rdr[2]).ToString());
                    datetime = DateTime.Parse(rdr[3].ToString());
                    emotion = Int32.Parse((rdr[4]).ToString());
                    VerseTagEmotionLike like = new VerseTagEmotionLike(id, emotion_tag_id, user_id, datetime);
                    List<VerseTagEmotion> emotions = getInstance().getListOfEmotions();
                    VerseTag vt = getInstance().getVerseTag(like, emotion);
                    vt.addLikeLoadedFromDB(like);
                }
                rdr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private VerseTag getVerseTag(VerseTagEmotionLike like, int emotion_id)
        {
            List<VerseTag> vts = getInstance().getListOfVerseTagsForEmotion(emotion_id);
            foreach (VerseTag vt in vts)
            {
                if (vt.id == like.emotion_tag_id)
                {
                    return vt;
                }
            }
            return null;
        }

        public static void addVerseTagDuringLoad(VerseTag vt)
        {
            //parts of this method should be synchronized not only called during startup.
            String unique_check_key = vt.emotion_id + "|" + vt.start_verse + "|" + vt.end_verse;


            String key = vt.start_verse + "|" + vt.end_verse;
            if (unique_check_map.ContainsKey(unique_check_key))
            {
                throw new VerseEmotionTagAlreadyPresentException("Attempted to add a verse tag which is already present for emotion: " + vt.emotion_id + " and verse_key: " + key);
            }

   
            unique_check_map.Add(unique_check_key, null);


            List<VerseTag> vts;
            if (verses_emotions.ContainsKey(key))
            {
                vts = verses_emotions[key];
            }
            else
            {
                vts = new List<VerseTag>();
                verses_emotions.Add(key, vts);
            }
            vts.Add(vt);


            //no check for duplicates when adding verse to emotion
            List<VerseTag> ets;
            if (emotion_verses.ContainsKey(vt.emotion_id))
            {
                ets = emotion_verses[vt.emotion_id];
            }
            else
            {
                ets = new List<VerseTag>();
                emotion_verses.Add(vt.emotion_id, ets);
            }
            ets.Add(vt);


        }

        public void addVerseTag(VerseTag vt)
        {
            //parts of this method should be synchronized not only called during startup.
            String unique_check_key = vt.emotion_id + "|" + vt.start_verse + "|" + vt.end_verse;


            String key = vt.start_verse + "|" + vt.end_verse;
            lock (thisLock)
            {
                if (unique_check_map.ContainsKey(unique_check_key))
                {
                    throw new VerseEmotionTagAlreadyPresentException("Attempted to add a verse tag which is already present for emotion: " + vt.emotion_id + " and verse_key: " + key);
                }
                unique_check_map.Add(unique_check_key, null);
            }

            List<VerseTag> vts;
            if (verses_emotions.ContainsKey(key))
            {
                vts = verses_emotions[key];
            }
            else
            {
                vts = new List<VerseTag>();
                verses_emotions.Add(key, vts);
            }
            vts.Add(vt);


            //no check for duplicates when adding verse to emotion
            List<VerseTag> ets;
            if (emotion_verses.ContainsKey(vt.emotion_id))
            {
                ets = emotion_verses[vt.emotion_id];
            }
            else
            {
                ets = new List<VerseTag>();
                emotion_verses.Add(vt.emotion_id, ets);
            }
            ets.Add(vt);


        }

        public List<VerseTagEmotion> getListOfEmotions()
        {
            return ListUtils.convertEmotionDictionaryToList(emotions);
        }

        public List<VerseTag> getListOfEmotionTagsForVerse(String verse_key)
        {
            if (verses_emotions.ContainsKey(verse_key))
                return verses_emotions[verse_key];

            return null;
        }

        public List<VerseTag> getListOfVerseTagsForEmotion(int emo_id)
        {
            if (emotion_verses.ContainsKey(emo_id))
                return emotion_verses[emo_id];

            return null;
        }

        public int getEmotionCount()
        {
            return emotions.Count;
        }

        public int getEmotionTagCountOnVerse(String verse_key)
        {
            if (verses_emotions[verse_key] != null)
                return verses_emotions[verse_key].Count;

            return 0;
        }

        public String getEmotionFromEmotionID(int emo_id)
        {
            if(emotions.ContainsKey(emo_id))
               return emotions[emo_id].emotion;

            return null;
        }

        public void addVerseTag(
            long user_id,
            String start_verse,
            String end_verse,
            int emo_id)
        {
            DateTime dt = DateTime.Now;
            VerseTag vt = new VerseTag(-1, emo_id, user_id, dt, start_verse, end_verse, null);
            //this will add verse tag to memory. and also does some unique checks.
            addVerseTag(vt);
            //this will spawn new thread to update DB
            new VerseTagTask(user_id, emo_id, start_verse, end_verse, dt, null, vt).AddVerseTask();
        }

    }
}
