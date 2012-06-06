using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class VerseMessagingManager
    {
        private List<VerseMessageThread> active_threads = new List<VerseMessageThread>();
        private UserSession us;
        private Object thisLock  = new Object();

        public VerseMessagingManager(UserSession us)
        {
            this.us = us;
            loadThreadList();
        }

        private void loadThreadList()
        {
            lock (thisLock)
            {
                List<long> list = VerseThreadManager.getInstance().getThreadIDsOfUser(us.user_profile.id);
                if (list != null)
                {
                    foreach (var i in list)
                    {
                        active_threads.Add(VerseThreadManager.getInstance().getVerseMessageThread(i));
                    }
                }
            }
        }

        public List<VerseMessageThread> getParticipatingThreads()
        {
            active_threads = new List<VerseMessageThread>(); 
            loadThreadList();
            active_threads.Sort(); 

            return active_threads;
        }

        public long createThreadAndAddPrivateMessage(String message_text, List<long> recipient_list, String start_verse, String end_verse, String subject)
        {
            if (recipient_list == null || recipient_list.Count <= 0)
                return -1;

            long recip_id = recipient_list[0];
            VerseMessageThread vmt = createThreadAndAddPrivateMessage(message_text, recip_id, start_verse, end_verse, subject);
            if (recipient_list.Count > 1)
            {
                for (int i = 1; i < recipient_list.Count; i++)
                {
                    addNewParticipantToThread(vmt, recipient_list[i]);
                }
            }
            return 0;
        }

        public VerseMessageThread createThreadAndAddPrivateMessage(String message_text, long recip_id, String start_verse, String end_verse, String subject)
        {
            DateTime datetime = DateTime.Now;
            VerseMessageThread vmt = new VerseMessageThread(
                -1, 
                start_verse, 
                end_verse, 
                Int32.Parse(us.user_profile.getDefaultTranslationId()),
                subject, 
                datetime, 
                datetime, 
                us.user_profile.id, 
                VerseMessageThread.THREAD_STATE_ACTIVE);
            //create thread
            int code = AddThreadToDB(vmt);
            
            if (code == THREAD_CREATED_CODE_SUCCESSFUL)
            {
                code = addMessageToThread(vmt, message_text); // TODO check result
                if (code == MESSAGE_SENT_CODE_SUCCESSFUL)
                {
                    addParticipantToThread(vmt,us.user_profile.id, datetime, datetime.AddSeconds(1));
                    addNewParticipantToThread(vmt, recip_id);
                    VerseThreadManager.getInstance().addThread(vmt);
                    //TODO.complete jere.
                }
                else
                {
                    //TODO: Roll back THREAD 
                }
            }
            return vmt;
        }

        public int addMessageToThread(VerseMessageThread vmt, String message_text)
        {
            DateTime datetime = DateTime.Now;
            VerseMessage vm = new VerseMessage(-1, vmt.thread_id, datetime, message_text, us.user_profile.id);
            //dont do this in a seperate thread now, because we need to know if it's succesful. 
            int code = AddMessageToThreadDB(message_text,datetime,vmt,vm);
            if(code == MESSAGE_SENT_CODE_SUCCESSFUL)
            {
                //this is a little messy. we should be consistent in the way we update memory and db. 

                vmt.addMessage(vm);


                updateThreadLastModifiedTime(vmt, vm);
                VerseMessageParticipant vmp = vmt.getParticipant(us.user_profile.id);
                updateParticipantThreadLastAccessedTime (vmp);

                return MESSAGE_SENT_CODE_SUCCESSFUL;
            }
            else 
            {
                return MESSAGE_SENT_CODE_ERROR;
            }
        }

        public VerseMessageParticipant addNewParticipantToThread(VerseMessageThread vmt, long participant_id)
        {
            DateTime datetime = DateTime.Now;
            VerseMessageParticipant vmp = new VerseMessageParticipant(-1,vmt.thread_id,participant_id,datetime,DateTime.MinValue);
            ParticipantTask pt = new ParticipantTask(us, vmp);
            pt.AddParticipantToThread();
            VerseThreadManager.getInstance().addParticipant(vmt,vmp);
            return vmp;
        }

        public VerseMessageParticipant addParticipantToThread(VerseMessageThread vmt, long participant_id, DateTime datetime_joined, DateTime datetime_last_read)
        {
            VerseMessageParticipant vmp = new VerseMessageParticipant(-1, vmt.thread_id, participant_id, datetime_joined, datetime_last_read);
            ParticipantTask pt = new ParticipantTask(us, vmp);
            //TODO change this not to be done in thread. 
            pt.AddParticipantToThread();
            VerseThreadManager.getInstance().addParticipant(vmt, vmp);
            return vmp;
        }

        public void removeParticipantFromThread(VerseMessageThread vmt)
        {
            long user_id = us.user_profile.id;
            if (vmt.getParticipant(user_id) != null)
            {
                VerseMessageParticipant vmp = vmt.getParticipant(user_id);
                ParticipantTask pt = new ParticipantTask(us, vmp);
                //TODO change this not to be done in thread. 
                pt.RemoveParticipantFromThread();
                VerseThreadManager.getInstance().removeParticipant(vmt, vmp);
            }
        }

         private int AddMessageToThreadDB(String message_text, DateTime datetime_sent, VerseMessageThread vmt, VerseMessage vm)
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "INSERT INTO versemessages VALUES (NULL, " + vmt.thread_id + ",'" + datetime_sent.ToString("yyyy-MM-dd HH:mm:ss") + "',@message_text," + us.user_profile.id + ")";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                cmd.Parameters.Add("@message_text", MySql.Data.MySqlClient.MySqlDbType.Text);
                cmd.Parameters["@message_text"].Value = message_text;

                int output = cmd.ExecuteNonQuery();
                long row_id = cmd.LastInsertedId;
                vm.message_id = row_id; //TODO: Check if this actually works. 
                return MESSAGE_SENT_CODE_SUCCESSFUL;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return MESSAGE_SENT_CODE_ERROR;
            }
            finally
            {
                conn.Close();
            }
        }

         private int AddThreadToDB(VerseMessageThread vmt)
         {
             MySqlConnection conn = DBManager.getConnection();
             try
             {
                 conn.Open();
                 //later on we will do db updates in seperate thread. 
                 string sqlQuery =
                     "INSERT INTO versemessagethreads VALUES (NULL, '" + 
                                                vmt.start_verse + "','" + 
                                                vmt.end_verse + "','" +
                                                vmt.translation + "','" +
                                                vmt.subject+ "','" +
                                                vmt.datetime_created.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 
                                                vmt.datetime_last_modified.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 
                                                us.user_profile.id + "','" + 
                                                /*vmt.is_private + "','" +*/ 
                                                vmt.thread_state+ "')";

                 MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);

                 int output = cmd.ExecuteNonQuery();
                 long row_id = cmd.LastInsertedId;
                 vmt.thread_id = row_id; //TODO: Check if this actually works. 
                 return THREAD_CREATED_CODE_SUCCESSFUL;
             }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.StackTrace);
                 return THREAD_CREATED_CODE_ERROR;
             }
             finally
             {
                 conn.Close();
             }
         }

         public Boolean isAThreadUpdatedSinceLastAccess()
         {
             List<VerseMessageThread> threads = us.verse_messaging_manager.getParticipatingThreads();
             DateTime last_accessed_date = DateTime.MinValue;
             DateTime last_mod_date = DateTime.MaxValue;
             VerseMessageParticipant vmp = null;
             Dictionary<long, VerseMessageParticipant> participants = null;
             foreach (var thread in threads)
             {
                 participants = thread.getParticipants();
                 if (participants == null)
                     continue;
                 vmp = participants[us.user_profile.id];
                 if (vmp == null)
                     continue;
                 last_accessed_date = vmp.datetime_last_read;
                 last_mod_date = thread.datetime_last_modified;

                 if (last_mod_date > last_accessed_date)
                     return true;
             }
             return false; 
         }

         public void updateThreadLastModifiedTime(VerseMessageThread vmt, VerseMessage vm)
         {
             if (vmt != null)
             {
                 DateTime datetime = DateTime.Now;
                 vmt.datetime_last_modified = datetime;
                 MessageTask mt = new MessageTask(us, vm);
                 mt.UpdateLastAccessedTime();
             }
         }

         public void updateParticipantThreadLastAccessedTime(VerseMessageParticipant vmp)
         {
             if (vmp != null)
             {
                 DateTime datetime = DateTime.Now;
                 vmp.updateDateTimeLastRead(datetime);
                 ParticipantAccessUpdateTask paut = new ParticipantAccessUpdateTask(us, vmp, datetime);
                 paut.UpdateParticipantLastAccessed();
             }
         }



        public const int MESSAGE_SENT_CODE_SUCCESSFUL = 0;
        public const int MESSAGE_SENT_CODE_ERROR = 1;
        public const int THREAD_CREATED_CODE_SUCCESSFUL = 0;
        public const int THREAD_CREATED_CODE_ERROR = 1;
    }
}
