using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class MessageInboxOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public MessageInboxOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            List<VerseMessageThread> threads = us.verse_messaging_manager.getParticipatingThreads();
            List<MenuOptionItem> final_list = new List<MenuOptionItem>();

            if (threads != null)
            {
                foreach (var thread in threads)
                {

                     MenuOptionItem m_o = new MessageThreadMenuOptionItem(
                                          "",
                                          "",
                                          target_page,
                                          "",
                                          thread);
                    final_list.Add(m_o);
                }
            }

            return final_list;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            return input;
        }
    }
}
