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
    public class MyFriendTask
    {
        private UserSession us { get; set; }
        private DateTime datetime { get; set; }
        private long friend_id { get; set; }


        public MyFriendTask(UserSession us, DateTime datetime, long friend_id)
        {
            this.us = us;
            this.datetime= datetime;
            this.friend_id = friend_id;
        }

        public void AddFriendRequestDBThreadTask()
        {
            Thread t = new Thread(new ThreadStart(AddFriendRequestToDB));
            t.Start();
        }

        private void AddFriendRequestToDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();


                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "INSERT INTO friends VALUES (NULL, " + us.user_profile.id + "," + friend_id + "," + FriendRelation.STATUS_PENDING + ",'" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "', NULL)";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                int output = cmd.ExecuteNonQuery();
                long row_id = cmd.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                conn.Close();
            }
            OnFinished();
        }

        public void ApproveFriendRequestDBThreadTask()
        {
            Thread t = new Thread(new ThreadStart(ApproveFriendRequestToDB));
            t.Start();
        }

        private void ApproveFriendRequestToDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();

                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "UPDATE friends SET datetime_accepted = '" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "', status = " + FriendRelation.STATUS_ACCEPTED +
                    " WHERE (id_a = '" + us.user_profile.id + "' AND id_b = '" + friend_id + "') OR " +
                    " (id_a = '" + friend_id + "' AND id_b = '" + us.user_profile.id + "')";
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
            OnFinished();
        }

        public void RejectFriendRequestDBThreadTask()
        {
            Thread t = new Thread(new ThreadStart(RejectFriendRequestToDB));
            t.Start();
        }

        private void RejectFriendRequestToDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                DateTime datetime = DateTime.Now;
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "UPDATE friends SET datetime_accepted = '" + datetime.ToString("yyyy-MM-dd HH:mm:ss") + "', status = " + FriendRelation.STATUS_REJECTED +
                    " WHERE (id_a = '" + us.user_profile.id + "' AND id_b = '" + friend_id + "') OR " +
                    " (id_a = '" + friend_id + "' AND id_b = '" + us.user_profile.id + "')";
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
            OnFinished();
        }

        public void DeleteFriendRequestDBThreadTask()
        {
            Thread t = new Thread(new ThreadStart(DeleteFriendRequestToDB));
            t.Start();
        }

        private void DeleteFriendRequestToDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                DateTime datetime = DateTime.Now;
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "DELETE FROM friends " +
                    " WHERE (id_a = '" + us.user_profile.id + "' AND id_b = '" + friend_id + "') OR " +
                    " (id_a = '" + friend_id + "' AND id_b = '" + us.user_profile.id + "')";
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
            OnFinished();
        }

        public void BlockFriendRequestDBThreadTask()
        {
            Thread t = new Thread(new ThreadStart(BlockFriendRequestToDB));
            t.Start();
        }

        private void BlockFriendRequestToDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();

                string sqlQuery =
                    "UPDATE friends SET status = CASE " +
                    " WHEN (id_a = '" + us.user_profile.id + "') THEN '" + FriendRelation.STATUS_BLOCKED_A + "' " +
                    " WHEN (id_b = '" + us.user_profile.id + "') THEN '" + FriendRelation.STATUS_BLOCKED_B + "' END " +
                    " WHERE (id_a = '" + us.user_profile.id + "' AND id_b = '" + friend_id + "') OR " +
                    " (id_a = '" + friend_id + "' AND id_b = '" + us.user_profile.id + "')";
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
            OnFinished();
        }

        protected virtual void OnFinished()
        {
            // raise finished in a threadsafe way 
            Console.WriteLine("Finsihed with friend db task");
        }
    }
}



/*public class MyTask
{
   string _a;
   int _b;
   int _c;
   float _d;

   public event EventHandler Finished;

   public MyTask( string a, int b, int c, float d )
   {
      _a = a;
      _b = b;
      _c = c;
      _d = d;
   }

   public void DoWork()
   {
       Thread t = new Thread(new ThreadStart(DoWorkCore));
       t.Start();
   }

   private void DoWorkCore()
   {
      // do some stuff
      OnFinished();
   }

   protected virtual void OnFinished()
   {
      // raise finished in a threadsafe way 
   }
}
*/