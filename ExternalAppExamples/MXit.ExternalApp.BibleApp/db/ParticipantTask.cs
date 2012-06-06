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
    public class ParticipantTask
    {
        private UserSession us { get; set; }
        private VerseMessageParticipant vmp { get; set; }

        public ParticipantTask(UserSession us, VerseMessageParticipant vmp)
        {
            this.us = us;
            this.vmp = vmp;
        }

        public void AddParticipantToThread()
        {
            Thread t = new Thread(new ThreadStart(AddParticipantToThreadDB));
            t.Start();
        }

        private void AddParticipantToThreadDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "INSERT INTO versemsgparticipants VALUES (NULL, " + vmp.thread_id + "," + vmp.user_id + ",'" + vmp.datetime_joined.ToString("yyyy-MM-dd HH:mm:ss") + "','" + vmp.datetime_last_read.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);

                int output = cmd.ExecuteNonQuery();
                long row_id = cmd.LastInsertedId;
                vmp.participant_row_id = row_id; //TODO: Check if this actually works. 
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

        public void RemoveParticipantFromThread()
        {
            Thread t = new Thread(new ThreadStart(RemoveParticipantFromThreadDB));
            t.Start();
        }

        private void RemoveParticipantFromThreadDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "DELETE FROM versemsgparticipants WHERE thread_id = " + vmp.thread_id + " AND user_id = "+ vmp.user_id;
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