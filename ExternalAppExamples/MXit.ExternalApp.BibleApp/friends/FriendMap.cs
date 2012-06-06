using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class FriendMap
    {
        public Dictionary<long, FriendRelation> friend_map = new Dictionary<long, FriendRelation>();
        private UserSession us;
        private Object thisLock = new Object();
        public FriendMap(UserSession us)
        {
            this.us = us;
            loadFriendRelations(us.user_profile);
        }

        private void loadFriendRelations(
            UserProfile user_profile)
        {
            lock (thisLock)
            {
                LinkedList<FriendRelation> friend_list = getFriendRelationsFromDB(-1);
                foreach (var friend_relation in friend_list)
                {
                    long friend_id = -1;
                    if (user_profile.id == friend_relation.id_a)
                        friend_id = friend_relation.id_b;
                    else
                        friend_id = friend_relation.id_a;

                    friend_map.Add(friend_id, friend_relation);
                }

            }
        }

        public LinkedList<FriendRelation> getFriendRelationsFromDB(
            int status)
        {
            string sqlQuery =
             "SELECT row_id, id_a, id_b, status, datetime_created, datetime_accepted" +
             " FROM friends WHERE id_a = '" + us.user_profile.id + "' OR id_b ='" + us.user_profile.id + "'";
            if (status != -1)
                sqlQuery = sqlQuery + " AND status='" + status + "'";
            MySqlConnection conn = DBManager.getConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                rdr = cmd.ExecuteReader();
                long row_id = -1;
                long id_a = -1;
                long id_b = -1;
                int status_of_relation;
                DateTime datetime_created;
                DateTime datetime_accepted = new DateTime();
                LinkedList<FriendRelation> friend_list = new LinkedList<FriendRelation>();
                String tmp_date = "";
                while (rdr.Read())
                {
                    row_id = long.Parse((rdr[0]).ToString());
                    id_a = long.Parse((rdr[1]).ToString());
                    id_b = long.Parse((rdr[2]).ToString());
                    status_of_relation = Int32.Parse(rdr[3].ToString());
                    datetime_created = DateTime.Parse(rdr[4].ToString());
                    tmp_date = rdr[5].ToString();
                    if (tmp_date != null && !"".Equals(tmp_date))
                        datetime_accepted = DateTime.Parse(rdr[5].ToString());
                    FriendRelation fr = new FriendRelation(
                        row_id,
                        id_a,
                        id_b,
                        status_of_relation,
                        datetime_created,
                        datetime_accepted);
                    friend_list.AddLast(fr);
                }
                return friend_list;
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
            }
        }

        public LinkedList<FriendRelation> getFriendRequests()
        {
            List<KeyValuePair<long, FriendRelation>> list;
            lock (thisLock)
            {
                list = friend_map.ToList();
            }
            LinkedList<FriendRelation> friend_request_list = new LinkedList<FriendRelation>();
            foreach (var fr_kvp in list)
            {
                FriendRelation fr = fr_kvp.Value;
                if (fr.id_b == us.user_profile.id && fr.status == FriendRelation.STATUS_PENDING)
                {
                    friend_request_list.AddLast(fr);
                }
            }
            return friend_request_list;
        }

        /*gets only approved active friends*/
        public LinkedList<FriendRelation> getFriends()
        {
            List<KeyValuePair<long, FriendRelation>> list;
            lock (thisLock)
            {
                list = friend_map.ToList();
            }
            LinkedList<FriendRelation> friend_request_list = new LinkedList<FriendRelation>();
            foreach (var fr_kvp in list)
            {
                FriendRelation fr = fr_kvp.Value;
                if ((fr.id_a == us.user_profile.id || fr.id_b == us.user_profile.id) && fr.status == FriendRelation.STATUS_ACCEPTED)
                {
                    friend_request_list.AddLast(fr);
                }
            }
            return friend_request_list;
        }

        public Boolean hasFriendRelation(long friend_id)
        {
            lock(thisLock)
            {
                if (friend_map.ContainsKey(friend_id))
                {
                    return true;//user already has friend request for this friend. 
                }
            }
            return false;
        }

        public void addFriendRequest(long friend_id, FriendRelation fr)
        {
            lock (thisLock)
            {
                friend_map.Add(friend_id, fr);
            }
        }

        public FriendRelation getFriendRelation(long friend_id)
        {
            lock (thisLock)
            {
                if (!friend_map.ContainsKey(friend_id))
                    return null;
                return friend_map[friend_id];
            }
        }

        public bool removeFriendRelation(long friend_id)
        {
            lock (thisLock)
            {
                if (!friend_map.ContainsKey(friend_id))
                    return false;
                return friend_map.Remove(friend_id);
            }
        }
    }
}
