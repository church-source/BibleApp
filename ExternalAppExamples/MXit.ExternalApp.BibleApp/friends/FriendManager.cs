using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MXit.User;
using MySql.Data;
using MySql.Data.MySqlClient;

using System.Threading;

namespace MxitTestApp
{
    public class FriendManager
    {
        public static Dictionary<long, FriendMap> user_list = new Dictionary<long, FriendMap>();
        private Object thisLock = new Object();
        private UserSession us;
        public FriendManager(
            UserProfile user_profile,
            UserSession user_session)
        {
            this.us = user_session;
            lock (thisLock)
            {
                if (user_list.ContainsKey(user_profile.id))
                {
                    user_list.Remove(user_profile.id);
                }
                user_list.Add(user_profile.id, new FriendMap(us));
            }
        }

        public void removeFriendMapFromSession()
        {
            Console.WriteLine("Removing user with id: " + us.user_profile.id + " friend map from session.");
            lock (thisLock)
            {
                if (user_list.ContainsKey(us.user_profile.id))
                {
                    user_list.Remove(us.user_profile.id);
                }
            }
        }

        public LinkedList<FriendRelation> getFriendRequests()
        {
            FriendMap fmap = user_list[us.user_profile.id];
            return fmap.getFriendRequests();
        }

        public LinkedList<FriendRelation> getFriends()
        {
            FriendMap fmap = user_list[us.user_profile.id];
            return fmap.getFriends();
        }


        /*this stores the last request and is responsible for maintaining synchronisation between memory
        * and DB views. 
        */
        public long addFriendRequest(
            long friend_id)
        {
            bool was_blocked_before = false;
            FriendRelation tmp_fr = null;
            if (user_list.ContainsKey(us.user_profile.id))
            {
                FriendMap fmap = user_list[us.user_profile.id];
                if (fmap.hasFriendRelation(friend_id))
                {
                    tmp_fr = fmap.getFriendRelation(friend_id); 
                    if (tmp_fr.status == FriendRelation.STATUS_ACCEPTED)
                    {
                        return FRIEND_REQUEST_ALREADY_FRIENDS;//user already has friend relation for this friend. 
                    }
                    else if (tmp_fr.status == FriendRelation.STATUS_PENDING)
                    {
                        return FRIEND_REQUEST_ALREADY_REQUESTED;
                    }
                    else if ((us.user_profile.id == tmp_fr.id_a && tmp_fr.status == FriendRelation.STATUS_BLOCKED_B)
                        || us.user_profile.id == tmp_fr.id_b && tmp_fr.status == FriendRelation.STATUS_BLOCKED_A)
                    {
                        return FRIEND_REQUEST_BLOCKED;
                    }
                    else if ((us.user_profile.id == tmp_fr.id_a && tmp_fr.status == FriendRelation.STATUS_BLOCKED_A)
                        || us.user_profile.id == tmp_fr.id_b && tmp_fr.status == FriendRelation.STATUS_BLOCKED_B)
                    {
                        was_blocked_before = true;
                    }
                }
            }
            DateTime datetime = DateTime.Now;
            MyFriendTask mft = new MyFriendTask(us, datetime, friend_id);
            FriendRelation fr = null;
            if (was_blocked_before)
            {
                mft.ApproveFriendRequestDBThreadTask();
                fr = tmp_fr;
                fr.setStatus(FriendRelation.STATUS_ACCEPTED);
                FriendMap fmap = user_list[us.user_profile.id];
                if (fmap != null && fmap.hasFriendRelation(friend_id))
                {
                        fmap.removeFriendRelation(friend_id);
                }
            }
            else
            {
                mft.AddFriendRequestDBThreadTask();
                fr = new FriendRelation(
                    -1,
                    us.user_profile.id,
                    friend_id,
                    FriendRelation.STATUS_PENDING,
                    datetime,
                    new DateTime());

            }
            
            if (!user_list.ContainsKey(us.user_profile.id))
            {
                user_list.Add(us.user_profile.id, new FriendMap(us));
            }

            FriendMap friend_map = user_list[us.user_profile.id];
            friend_map.addFriendRequest(friend_id, fr);//update status in memory. 

            //if the friend entry does not exist, it means the friend is not online then we leave the in memory loading to happen only when user comes online (lazy loading).
            if (user_list.ContainsKey(friend_id))
            {
                //if was blocked before there is an entry before. so remove
                if (was_blocked_before)
                {
                    FriendMap tmp_fm = (user_list[friend_id]);
                    if (tmp_fm != null && tmp_fm.hasFriendRelation(us.user_profile.id))
                    {
                        tmp_fm.removeFriendRelation(us.user_profile.id);
                    }
                }
                (user_list[friend_id]).addFriendRequest(us.user_profile.id, fr);//update status in memory. 
            }

            if (was_blocked_before)
                return FRIEND_REQUEST_BLOCKED_APPROVED;

            return 0;
        }



        /*this stores the last request and is responsible for maintaining synchronisation between memory
         * and DB views. 
         */
        public long approveFriendRequest(
            long friend_id)
        {
            FriendMap fmap;
            if (user_list.ContainsKey(us.user_profile.id))
            {
                fmap = user_list[us.user_profile.id];
                if (!fmap.hasFriendRelation(friend_id))
                {
                    return -1;//there is no friend request to approve 
                }
                DateTime datetime = DateTime.Now;

                MyFriendTask mft = new MyFriendTask(us, datetime, friend_id);
                mft.ApproveFriendRequestDBThreadTask();

                FriendRelation fr = (user_list[us.user_profile.id]).getFriendRelation(friend_id);
                fr.setStatus(FriendRelation.STATUS_ACCEPTED); //update status in memory. 
                fr.setDateTimeAccepted(datetime); //update status in memory. 

                //the friend is online so we update both
                if ((user_list.ContainsKey(friend_id) && (user_list[friend_id]).hasFriendRelation(us.user_profile.id)))
                {
                    FriendRelation fr2 = (user_list[friend_id]).getFriendRelation(us.user_profile.id);
                    fr2.setStatus(FriendRelation.STATUS_ACCEPTED); //update status in memory. 
                    fr2.setDateTimeAccepted(datetime); //update status in memory. 
                }
                return 0;
            }
            return -2;
        }

        /*this stores the last request and is responsible for maintaining synchronisation between memory
         * and DB views. 
         */
        public long rejectFriendRequest(
            long friend_id)
        {
            FriendMap fmap;
            if (user_list.ContainsKey(us.user_profile.id))
            {
                fmap = user_list[us.user_profile.id];
                if (!fmap.hasFriendRelation(friend_id))
                {
                    return -1;//there is no friend request to reject 
                }
                DateTime datetime = DateTime.Now;

                MyFriendTask mft = new MyFriendTask(us, datetime, friend_id);
                mft.RejectFriendRequestDBThreadTask();


                FriendRelation fr = (user_list[us.user_profile.id]).getFriendRelation(friend_id);
                fr.setStatus(FriendRelation.STATUS_ACCEPTED); //update status in memory. 
                fr.setDateTimeAccepted(datetime); //update status in memory. 

                //the friend is online so we update both
                if ((user_list.ContainsKey(friend_id) && (user_list[friend_id]).hasFriendRelation(us.user_profile.id)))
                {
                    FriendRelation fr2 = (user_list[friend_id]).getFriendRelation(us.user_profile.id);
                    fr2.setStatus(FriendRelation.STATUS_ACCEPTED); //update status in memory. 
                    fr2.setDateTimeAccepted(datetime); //update status in memory. 
                }
                return 0;
            }
            return -2;
        }

        public long deleteFriendRequest(
            long friend_id)
        {
           
            FriendMap fmap;
            if (user_list.ContainsKey(us.user_profile.id))
            {
                fmap = user_list[us.user_profile.id];
                if (!fmap.hasFriendRelation(friend_id))
                {
                    return -1;//there is no friend request to approve 
                }
                DateTime datetime = DateTime.Now;

                MyFriendTask mft = new MyFriendTask(us, datetime, friend_id);
                mft.DeleteFriendRequestDBThreadTask();
                (user_list[us.user_profile.id]).removeFriendRelation(friend_id);
                if ((user_list.ContainsKey(friend_id) && (user_list[friend_id]).hasFriendRelation(us.user_profile.id)))
                {
                    user_list[friend_id].removeFriendRelation(us.user_profile.id);
                }
                return 0;
            }
                    return -2;
        }

        public List<Char> getStartingCharacters()
        {
            LinkedList<FriendRelation> friend_relations = getFriends();
            Dictionary<String,Char> chars = new Dictionary<String,char>();
            long user_id = -1;
            char tmp;
            String tmp_start = "";
            foreach (var rel in friend_relations)
            {
                if(us.user_profile.id == rel.id_a)
                    user_id = rel.id_b;
                else
                    user_id = rel.id_a;
                String user_name = UserNameManager.getUserName(user_id);
                if (user_name != null && user_name.Length > 0)
                {
                    tmp_start = user_name[0].ToString().ToUpper();
                    tmp = user_name[0];
                    if (!chars.ContainsKey(tmp_start))
                    {
                        chars.Add(tmp_start, tmp);
                    }
                }
            }
            List<KeyValuePair<String, char>> list = chars.ToList();

            List<char> final_char_list = new List<char>();
            char tmp_char;
            foreach (var a_char_kvp in list)
            {
                tmp_char = a_char_kvp.Value;
                final_char_list.Add(tmp_char);

            }
            return final_char_list;
        }

        /* this clears the user's history
        */
        public long blockFriend(
            long friend_id)
        {
            FriendMap fmap;
            if (user_list.ContainsKey(us.user_profile.id))
            {
                fmap = user_list[us.user_profile.id];
                if (!fmap.hasFriendRelation(friend_id))
                {
                    return -1;//there is no friend request to approve 
                }
                DateTime datetime = DateTime.Now;

                MyFriendTask mft = new MyFriendTask(us, datetime, friend_id);
                mft.BlockFriendRequestDBThreadTask();
                FriendRelation fr = (user_list[us.user_profile.id]).getFriendRelation(friend_id);
                if (fr.id_a == us.user_profile.id)
                    fr.setStatus(FriendRelation.STATUS_BLOCKED_A); //update status in memory. 
                else
                    fr.setStatus(FriendRelation.STATUS_BLOCKED_B); //update status in memory. 

                if ((user_list.ContainsKey(friend_id) && (user_list[friend_id]).hasFriendRelation(us.user_profile.id)))
                {
                    FriendRelation fr2 =  (user_list[friend_id]).getFriendRelation(us.user_profile.id);
                    if(fr.id_a == us.user_profile.id)
                        fr2.setStatus(FriendRelation.STATUS_BLOCKED_A); //update status in memory. 
                    else
                        fr2.setStatus(FriendRelation.STATUS_BLOCKED_B); //update status in memory.  
                }
            }
            return -2;
        }

        //determines what the current status of this friend is relative to the current user
        public int getFriendStatus(long friend_id)
        {
            
            FriendMap fmap;
            if (user_list.ContainsKey(us.user_profile.id))
            {
                fmap = user_list[us.user_profile.id];
                if (!fmap.hasFriendRelation(friend_id))
                {
                    return -1;//there is no such friend
                }
                FriendRelation fr = (user_list[us.user_profile.id]).getFriendRelation(friend_id);
                if ((fr.id_a == us.user_profile.id && fr.status == FriendRelation.STATUS_BLOCKED_A)
                    || (fr.id_b == us.user_profile.id && fr.status == FriendRelation.STATUS_BLOCKED_B))
                    return FriendRelation.FRIEND_BLOCKED_BY_YOU;
                else if ((fr.id_a == us.user_profile.id && fr.status == FriendRelation.STATUS_BLOCKED_B)
                    || (fr.id_b == us.user_profile.id && fr.status == FriendRelation.STATUS_BLOCKED_A))
                    return FriendRelation.FRIEND_BLOCKED_YOU;

                return fr.status;
            }
            return -1;
        }

        public const int FRIEND_REQUEST_SUCCESSFUL = 0;
        public const int FRIEND_REQUEST_ALREADY_FRIENDS = 1;
        public const int FRIEND_REQUEST_ALREADY_REQUESTED = 2;
        public const int FRIEND_REQUEST_BLOCKED = 3;
        public const int FRIEND_REQUEST_BLOCKED_APPROVED = 4;

    }
}
