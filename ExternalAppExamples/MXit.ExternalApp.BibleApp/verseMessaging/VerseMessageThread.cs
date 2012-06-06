using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class VerseMessageThread : IComparable
    {
        public long thread_id { get; set; }
        public String start_verse { get; set; }
        public String end_verse { get; set; }
        public int translation { get; set; }
        public String subject { get; set; }
        public DateTime datetime_created { get; set; }
        public DateTime datetime_last_modified { get; set; }
        public long user_created_id { get; set; }
        //public Boolean is_private { get; set; }
        public long thread_state { get; set; }

        private List<VerseMessage> messages { get; set; }
        private Dictionary<long,VerseMessageParticipant> participants { get; set; }

        private Boolean isLoaded = false;
        private Object thisLock = new Object(); 

        public VerseMessageThread(
            long thread_id,
            String start_verse,
            String end_verse,
            int translation,
            String subject,
            DateTime datetime_created,
            DateTime datetime_last_modified,
            long user_created_id,
            /*Boolean is_private,*/
            long thread_state)
        {
            this.thread_id = thread_id;
            this.start_verse = start_verse;
            this.end_verse = end_verse;
            this.translation = translation;
            this.subject = subject;
            this.datetime_created = datetime_created;
            this.datetime_last_modified = datetime_last_modified;
            this.user_created_id = user_created_id;
            //this.is_private = is_private;
            this.thread_state = thread_state;
        }

        private void loadMessagesAndParticipantsFromDB()
        {
               if (!isLoaded)
                {
                    //TODO: load Messages and Participants. 
                    messages = new List<VerseMessage>();
                    loadMessagesByThreadID();
                    participants = new Dictionary<long,VerseMessageParticipant>();
                    loadParticipantsByThreadID();
                    isLoaded = true;
                }
        }

        private void loadMessagesByThreadID()
        {
            string sqlQuery = "SELECT message_id, thread_id, datetime_sent, message_text, sender_id " +
            " FROM versemessages" +
            " WHERE thread_id = " +thread_id +" ORDER BY datetime_sent";

            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long message_id = -1;
                long sender_id = -1;
                String message_text = "";
                DateTime datetime_sent;
                VerseMessage tmp_vm = null;
                while (rdr.Read())
                {
                    message_id = long.Parse((rdr[0]).ToString());
                    datetime_sent = DateTime.Parse((rdr[2]).ToString());
                    message_text = (rdr[3].ToString());
                    sender_id = long.Parse(rdr[4].ToString());
                    tmp_vm = new VerseMessage(
                        message_id,
                        thread_id,
                        datetime_sent,
                        message_text,
                        sender_id);
                    messages.Add(tmp_vm);
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

        private void loadParticipantsByThreadID()
        {
            string sqlQuery = "SELECT participant_row_id, thread_id, user_id, datetime_joined, datetime_last_read " +
            " FROM versemsgparticipants" +
            " WHERE thread_id = " + thread_id;

            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long participant_row_id = -1;
                long user_id = -1;
                DateTime datetime_joined;
                DateTime datetime_last_read;
                VerseMessageParticipant tmp_vmp = null;
                while (rdr.Read())
                {
                    participant_row_id = long.Parse((rdr[0]).ToString());
                    user_id = long.Parse(rdr[2].ToString());
                    datetime_joined = DateTime.Parse((rdr[3]).ToString());
                    if (!Convert.IsDBNull(rdr[4]))  
                        datetime_last_read = DateTime.Parse((rdr[4]).ToString());
                    else
                        datetime_last_read = DateTime.MinValue;

                    tmp_vmp = new VerseMessageParticipant(
                        participant_row_id,
                        thread_id,
                        user_id,
                        datetime_joined,
                        datetime_last_read);
                    participants.Add(user_id,tmp_vmp);
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

        public List<VerseMessage> getMessages()
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();
                }
            }
            return messages;
        }

        public Dictionary<long, VerseMessageParticipant> getParticipants()
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();//syncrhonisation is handled within this method. 
                }
            }
            return participants;
        }

        public VerseMessageParticipant getParticipant(long id)
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();
                }
            }
            if (participants.ContainsKey(id))
                    return participants[id];

            return null;
        }

        public List<VerseMessageParticipant> getListOfParticipants()
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();
                }
            }
            return ListUtils.convertVMPDictionaryToList(participants);
        }

        public Boolean isThreadLoaded()
        {
            return isLoaded;
        }

        public void addMessage(VerseMessage vm)
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();
                }
                else
                {
                    messages.Add(vm);
                }
                //TODO: this message at the moment is always added in DB first, check this because we might change it to threaded task later on then it might be buggy
            }
            
            
        }

        public void addParticipant(VerseMessageParticipant vmp)
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();
                }
            }
            if(vmp != null && !participants.ContainsKey(vmp.user_id))
                    participants.Add(vmp.user_id, vmp);
        }

        public void removeParticipant(VerseMessageParticipant vmp)
        {
            lock (thisLock)
            {
                if (!isLoaded)
                {
                    loadMessagesAndParticipantsFromDB();
                }
            }
            if (vmp != null && participants.ContainsKey(vmp.user_id))
                participants.Remove(vmp.user_id);
        }

        int IComparable.CompareTo(Object obj)
        {
            if (obj == null)
                return -1;
            else
            {
                VerseMessageThread vmt = (VerseMessageThread)obj;
                if (this.datetime_last_modified > vmt.datetime_last_modified)
                    return -1;
                if (this.datetime_last_modified == vmt.datetime_last_modified)
                    return 0;
                else 
                    return 1;
            }
        }

        public const int THREAD_STATE_ACTIVE    = 0;
        public const int THREAD_STATE_SUSPENDED   = 1;

        public const String NOTIFICATION_THREAD = "NOTIF";
    }
}
