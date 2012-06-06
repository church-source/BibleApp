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
    public class ParticipantAccessUpdateTask
    {
        private UserSession us { get; set; }
        private VerseMessageParticipant vmp { get; set; }
        private DateTime datetime_last_read { get; set; }

        public ParticipantAccessUpdateTask(UserSession us, VerseMessageParticipant vmp, DateTime datetime_last_read)
        {
            this.us = us;
            this.vmp = vmp;
            this.datetime_last_read = datetime_last_read;
        }

        public void UpdateParticipantLastAccessed()
        {
            Thread t = new Thread(new ThreadStart(UpdateParticipantLastAccessedDB));
            t.Start();
        }

        private void UpdateParticipantLastAccessedDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "UPDATE versemsgparticipants SET datetime_last_read = '" + datetime_last_read.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE thread_id = '" +vmp.thread_id +"' AND user_id = '" + vmp.user_id+"'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);

                int output = cmd.ExecuteNonQuery();
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

    }
}