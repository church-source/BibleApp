using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Timers;
using System.Net;
using System.IO;



namespace MxitTestApp
{
    public class DailyVerseObservable
    {
        private static DailyVerseObservable instance = null;
        private static List<DailyVerse> daily_verses;
        private static Object lockObject = new Object();

        private List<IDailyVerseObserver> observers = new List<IDailyVerseObserver>();

        static DailyVerseObservable()
        {

            daily_verses = new List<DailyVerse>();
            string sqlQuery = "SELECT id, datetime,verse_ref,verse_text,sent, sent_datetime FROM dailyverses ORDER BY id";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                int dv_id = -1;
                DateTime datetime;
                String verse_ref;
                String verse_text;
                int sent=-1;
                while (rdr.Read())
                {
                    dv_id = Int32.Parse((rdr[0]).ToString());
                    datetime = DateTime.Parse(rdr[1].ToString());
                    verse_ref = (rdr[2]).ToString();
                    verse_text = (rdr[3]).ToString();

                    DailyVerse dv = new DailyVerse(
                        dv_id,
                        datetime,
                        verse_ref,
                        verse_text);
                    sent = Int32.Parse((rdr[4]).ToString());
                    if (sent == 1)
                    {
                        dv.is_sent = true;
                        if (rdr[5] != null && !"".Equals(rdr[5].ToString().Trim()))
                        {
                            dv.sent_datetime = DateTime.Parse(rdr[5].ToString());
                        }
                    }
                    daily_verses.Add(dv);
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
        
        public DailyVerseObservable()
        {
            Timer myTimer = new Timer();
            GetDailyVerse(null, null);//call it just after loading
            myTimer.Elapsed += new ElapsedEventHandler( GetDailyVerse );
            myTimer.Interval = 3600000;// every hour

            myTimer.Start();
        }

        //this has to be called only after the Bible Load
        public static DailyVerseObservable getInstance()
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new DailyVerseObservable();
                    }
                }
            }
            return instance;
        }

        public void AddObservers(IDailyVerseObserver observer)
        {
            observers.Add(observer);
        }

        public void RemoveObserver(IDailyVerseObserver observer)
        {
            observers.Remove(observer);
        }

        //only call this on update of daily Verse.
        public void NotifyObservers(DailyVerse dv)
        {
            // Notify the observers by looping through
            // all the registered observers.
            foreach (IDailyVerseObserver observer in observers)
            {
                observer.updateDailyVerse(dv);
            }
        }

        private void GetDailyVerse(object source, ElapsedEventArgs e)
        {
            attempUpdateOfCurrentDailyVerse();
        }

        private void attempUpdateOfCurrentDailyVerse()
        {
            try
            {
                StringBuilder sUrl = new StringBuilder();
                sUrl.Append("http://www.esvapi.org/v2/rest/dailyVerse");
                sUrl.Append("?key=IP&output-format=plain-text&include-passage-horizontal-lines=false&include-heading-horizontal-lines=false&include-headings=false&include-subheadings=false");
                sUrl.Append("&include-headings=true");

                WebRequest oReq = WebRequest.Create(sUrl.ToString());
                StreamReader sStream = new StreamReader(oReq.GetResponse().GetResponseStream());

                String verse_ref = sStream.ReadLine().Trim();
                //validate verse 
                if (!Verse_Handler.validateVerseReference(verse_ref))
                {
                    Console.WriteLine("INVALID DAILY VERSE OBTAINED!!!!!!!!!!!!!");
                    return; // do nothing if invalid.
                }
                String verse_text = sStream.ReadToEnd();

                if (daily_verses.Count > 0)
                {
                    if (verse_ref.ToUpper().Equals(daily_verses[daily_verses.Count - 1].verse_ref.Trim().ToUpper()))
                    {
                        return; //do nothing
                    }
                }
                Console.WriteLine("NEW VERSE FOUND!!!!!!!!!!!!!!!!!!!!!");
                //new verse found, so we update table
                DateTime datetime = DateTime.Now;
                long id = insertDailyVerseIntoDB(datetime, verse_ref, verse_text);
                DailyVerse dv = new DailyVerse(
                    id,
                    datetime,
                    verse_ref,
                    verse_text);
                daily_verses.Add(dv);
                NotifyObservers(dv);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static long insertDailyVerseIntoDB(
            DateTime datetime,
            String verse_ref,
            String verse_text)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery =
                    "INSERT INTO dailyverses VALUES(NULL,'" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "','"+ verse_ref + "', @verse_text,0, NULL)";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                cmd.Parameters.Add("@verse_text", MySql.Data.MySqlClient.MySqlDbType.Text);
                cmd.Parameters["@verse_text"].Value = verse_text;
                
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

        public DailyVerse loadLatestDailyVerse()
        {
           /* string sqlQuery =
            "SELECT id, datetime, verse_ref, verse_text FROM dailyverses where sent = 0 ORDER BY id LIMIT 0,1";
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long id = -1;
                DateTime datetime;
                String verse_ref = "";
                String verse_text = "";
                DailyVerse dv = null;
                if (rdr.Read())
                {
                    id = long.Parse((rdr[0]).ToString());
                    datetime = DateTime.Parse(rdr[1].ToString());
                    verse_ref = rdr[2].ToString();
                    verse_text = rdr[3].ToString();
                    dv = new DailyVerse(
                        id,
                        datetime,
                        verse_ref,
                        verse_text);
                    return dv;
                }
                return null;
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
            }*/
            LinkedList<DailyVerse> daily_verses_to_return = new LinkedList<DailyVerse>();
            foreach (DailyVerse daily_verse in daily_verses)
            {
                if (daily_verse.is_sent == false)
                {
                    return daily_verse;
                }
            }
            return null;
        }

        public void updateSentStatusOfDailyVerseToSent(
            long id,
            DateTime datetime)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery =
                    "UPDATE dailyverses SET sent = 1, sent_datetime = '"+ datetime.ToString("yyyy-MM-dd HH:mm:ss") + "'  WHERE id = '" + id + "'";
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

        public List<DailyVerse> getSentDailyVerses()
        {
            LinkedList<DailyVerse> daily_verses_to_return = new LinkedList<DailyVerse>();
            foreach (DailyVerse daily_verse in daily_verses)
            {
                if (daily_verse.is_sent == true)
                {
                    daily_verses_to_return.AddFirst(daily_verse);
                }
            }
            return daily_verses_to_return.ToList<DailyVerse>();
        }

        public override String ToString()
        {
            return "A TEST";
        }
    }
}
