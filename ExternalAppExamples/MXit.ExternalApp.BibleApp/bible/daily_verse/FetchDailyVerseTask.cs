using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Net;
using System.IO;

using System.Threading;

namespace MxitTestApp
{
    public class FetchDailyVerseTask
    {
        private DailyVerseObservable daily_verse_observable { get; set; }

        public FetchDailyVerseTask(DailyVerseObservable daily_verse_observable)
        {
            this.daily_verse_observable = daily_verse_observable;
        }

        public void AttemptToUpdateVerse()
        {
            Thread t = new Thread(new ThreadStart(FetchLatestVerse));
            t.Start();
        }

        private void FetchLatestVerse()
        {
         /*   StringBuilder sUrl = new StringBuilder();
            sUrl.Append("http://www.esvapi.org/v2/rest/dailyVerse");
            sUrl.Append("?key=IP&output-format=plain-text&include-passage-horizontal-lines=false&include-heading-horizontal-lines=false&include-headings=false&include-subheadings=false");
            sUrl.Append("&include-headings=true");

            WebRequest oReq = WebRequest.Create(sUrl.ToString());
            StreamReader sStream = new StreamReader(oReq.GetResponse().GetResponseStream());

            String verse_ref = sStream.ReadLine().Trim();
            String verse_text = sStream.ReadToEnd();
            //            Console.WriteLine(verse_ref + " - " + verse_text);
            if (daily_verse_observable.daily_verses.Count > 0)
            {
                if (verse_ref.ToUpper().Equals(
                        daily_verse_observable.daily_verses[daily_verse_observable.daily_verses.Count - 1].verse_ref.Trim().ToUpper())) ;
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
            daily_verse_observable.daily_verses.Add(dv);
            daily_verse_observable.Notify();*/
        }

        public static long insertDailyVerseIntoDB(
            DateTime datetime,
            String verse_ref,
            String verse_text)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                string sqlQuery =
                    "INSERT INTO dailyverses VALUES(NULL,'" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verse_ref + "','" + verse_text + "')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
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

    }
}