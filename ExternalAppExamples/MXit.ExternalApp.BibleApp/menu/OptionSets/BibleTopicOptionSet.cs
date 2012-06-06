using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class BibleTopicOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";

        public BibleTopicOptionSet(String target_page) 
        {
            this.target_page = target_page;
            init();
        }

        public override void init()
        {
        }


        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            String current_category_id = us.getVariable(CATEGORY_ID_VAR_NAME);
            if(current_category_id == null)
            {
                return null;
            }
            int cat_id = Int32.Parse(current_category_id);
            List<Topic> topic_list = BibleTopicManager.getInstance().getCategory(cat_id).topics;
            if (topic_list != null)
            {
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                for (int i = 0; i < topic_list.Count; i++)
                {
                        final_list.Add(
                            new MenuOptionItem(
                                (topic_list[i].verse_ref).ToString(),
                                (i+1).ToString(),
                                target_page,
                                topic_list[i].topic + " - " + topic_list[i].verse_ref));
                }
                return final_list;
            } 
            return null;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            List<MenuOptionItem> list = getOptionList(us);
            for (int i = 0; i < list.Count; i++)
            {
                if (input == list[i].link_val)
                    return list[i].menu_option_id;
            }
            return input;
        }

        public const String CATEGORY_ID_VAR_NAME = "Bible_Topic.category_id";
    }
}
