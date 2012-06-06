using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class MyFriendRequestOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public MyFriendRequestOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            LinkedList<FriendRelation> friend_list = us.friend_manager.getFriendRequests();

            if (friend_list != null)
            {
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                int i=0;
                long friend_id = -1;
                foreach(var friend in friend_list)
                {
                    if (friend != null)
                    {
                        

                        if(friend.id_a == us.user_profile.id)
                        {
                            friend_id = friend.id_b;
                        }
                        else
                        {
                                friend_id = friend.id_a;
                        }
                        MenuOptionItem m_o = new FriendRelationMenuOptionItem(
                                        "*",
                                        friend.friendship_id.ToString(),
                                        target_page,
                                        friend_id.ToString(),
                                        friend);
                        i++;
                        final_list.Add(m_o);

                    }
                }
                return final_list;
            }
            return null;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            return input;
        }
    }
}
