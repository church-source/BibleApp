using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class VerseThreadManager
    {
        private static VerseThreadManager instance;
        private Object thisLock = new Object();
        private static Object staticLock = new Object();
        private static Dictionary<long, VerseMessageThread> threads = new Dictionary<long, VerseMessageThread>();

        private static Dictionary<long, List<long>> users_threads = new Dictionary<long, List<long>>();

        static VerseThreadManager()
        {
            string sqlQuery = "SELECT thread_id, verse_start, verse_end, translation, subject, datetime_created, datetime_last_modified, user_id_created, state  FROM versemessagethreads";
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long thread_id = -1;
                String start_verse = "";
                String end_verse = "";
                int translation = -1;
                String subject = "";
                DateTime datetime_created;
                DateTime datetime_last_modified;
                VerseMessageThread tmp_vmt = null;
                long user_created_id = -1;
                /*Boolean is_private = true;*/
                int state = -1;
                while (rdr.Read())
                {
                    thread_id = long.Parse((rdr[0]).ToString());
                    start_verse = (rdr[1]).ToString();
                    end_verse = (rdr[2]).ToString();
                    translation = Int32.Parse((rdr[3]).ToString());
                    subject = (rdr[4]).ToString();
                    datetime_created = DateTime.Parse(rdr[5].ToString());
                    datetime_last_modified = DateTime.Parse(rdr[6].ToString());
                    
                    user_created_id = long.Parse(rdr[7].ToString());
                    //is_private = Boolean.Parse(rdr[6].ToString());
                    state = Int32.Parse(rdr[8].ToString());
                    
                    tmp_vmt = new VerseMessageThread(
                        thread_id,
                        start_verse,
                        end_verse,
                        translation,
                        subject,
                        datetime_created,
                        datetime_last_modified,
                        user_created_id,
                        /*is_private,*/
                        state);
                    threads.Add(thread_id, tmp_vmt);
                }
                rdr.Close();
                conn.Close();

                sqlQuery = "SELECT user_id, thread_id FROM versemsgparticipants";
                conn = DBManager.getConnection();
                rdr = null;
                conn.Open();
                cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long t_id = -1;
                long u_id = -1;
                List<long> list;
                while (rdr.Read())
                {
                    u_id = long.Parse((rdr[0]).ToString());
                    t_id = long.Parse((rdr[1]).ToString());
                    if (users_threads.ContainsKey(u_id))
                    {
                        list = users_threads[u_id];                        
                    }
                    else
                    {
                        list = new List<long>();
                        users_threads.Add(u_id, list);
                    }
                    list.Add(t_id);
                }
                rdr.Close();
                conn.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();
                conn.Close();
            }
        }

        //use the double null lock check for now. 
        public static VerseThreadManager getInstance()
        {
            if (instance == null)
            {
                lock (staticLock)
                {
                    if (instance == null)
                    {
                        instance = new VerseThreadManager();
                    }
                }
            }
            return instance;
        }

        public VerseMessageThread getVerseMessageThread(long thread_id)
        {
            if(threads.ContainsKey(thread_id))
                return threads[thread_id];
            return null;
        }

        public List<long> getThreadIDsOfUser(long user_id)
        {
            if (users_threads.ContainsKey(user_id))
            {
                return users_threads[user_id];
            }
            return null;
        }

        public void addParticipant(VerseMessageThread vmt, VerseMessageParticipant vmp)
        {
            if (vmp != null && vmt!= null)
            {
                long u_id = vmp.user_id;
                if (users_threads.ContainsKey(u_id))
                {
                    if (!users_threads[u_id].Contains(vmp.thread_id))
                    {
                        users_threads[u_id].Add(vmp.thread_id);
                    }
                }
                else 
                {
                    List<long> list = new List<long>();
                    users_threads.Add(u_id, list);
                    list.Add(vmp.thread_id);
                }
                vmt.addParticipant(vmp);
            }
        }

        public void removeParticipant(VerseMessageThread vmt, VerseMessageParticipant vmp)
        {
            if (vmp != null && vmt != null)
            {
                long u_id = vmp.user_id;
                if (users_threads.ContainsKey(u_id))
                {
                    if (users_threads[u_id].Contains(vmp.thread_id))
                    {
                        users_threads[u_id].Remove(vmp.thread_id);
                    }
                }
                vmt.removeParticipant(vmp);
            }
        }

        public void addThread(VerseMessageThread vmt)
        {
            threads.Add(vmt.thread_id,vmt);
        }
    }
}
