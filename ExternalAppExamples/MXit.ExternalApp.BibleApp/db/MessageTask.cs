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
    public class MessageTask
    {
        private UserSession us { get; set; }
        private VerseMessage vm { get; set; }

        public MessageTask(UserSession us, VerseMessage vm)
        {
            this.us = us;
            this.vm = vm;
        }

        public void AddMessageToThread()
        {
            Thread t = new Thread(new ThreadStart(AddMessageToThreadDB));
            t.Start();
        }

        private void AddMessageToThreadDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();



                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "INSERT INTO versemessages VALUES (NULL, " + vm.thread_id + ",'" + vm.datetime_sent.ToString("yyyy-MM-dd HH:mm:ss") + "',@message_text," + us.user_profile.id + ")";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                cmd.Parameters.Add("@message_text", MySql.Data.MySqlClient.MySqlDbType.Text);
                cmd.Parameters["@message_text"].Value = vm.message_text;

                int output = cmd.ExecuteNonQuery();
                long row_id = cmd.LastInsertedId;
                vm.message_id = row_id; //TODO: Check if this actually works. 
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


        public void UpdateLastAccessedTime()
        {
            Thread t = new Thread(new ThreadStart(UpdateLastAccessedTimeDB));
            t.Start();
        }

        private void UpdateLastAccessedTimeDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();



                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "UPDATE versemessagethreads SET datetime_last_modified = '" + vm.datetime_sent.ToString("yyyy-MM-dd HH:mm:ss")  + "' WHERE thread_id = " + vm.thread_id;
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