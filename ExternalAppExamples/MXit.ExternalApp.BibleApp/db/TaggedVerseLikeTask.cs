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
    public class TaggedVerseLikeTask
    {
        private VerseTagEmotionLike vtel { get; set; }

        public TaggedVerseLikeTask(VerseTagEmotionLike vtel)
        {
            this.vtel = vtel;
        }

        public void AddTaggedVerseLike()
        {
            Thread t = new Thread(new ThreadStart(AddTaggedVerseLikeDB));
            t.Start();
        }

        private void AddTaggedVerseLikeDB()
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                //later on we will do db updates in seperate thread. 
                string sqlQuery =
                    "INSERT INTO emotion_tag_likes VALUES (NULL, " + vtel.emotion_tag_id + "," + vtel.user_id + ",'" + vtel.datetime.ToString("yyyy-MM-dd HH:mm:ss")+ "')";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);

                int output = cmd.ExecuteNonQuery();
                long row_id = cmd.LastInsertedId;
                vtel.id = row_id; 
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