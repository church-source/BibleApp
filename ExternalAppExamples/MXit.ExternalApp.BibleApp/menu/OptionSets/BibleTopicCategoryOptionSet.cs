using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class BibleTopicCategoryOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";

        public BibleTopicCategoryOptionSet(String target_page) 
        {
            this.target_page = target_page;
            init();
        }

        public override void init()
        {
            List<Category> category_list = BibleTopicManager.getInstance().getListOfCategories();
            if (category_list != null)
            {
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                for (int i = 0; i < category_list.Count; i++)
                {
                    final_list.Add(
                        new MenuOptionItem(
                            (category_list[i].category_id).ToString(),
                            (i+1).ToString(),
                            target_page,
                            category_list[i].name));
                }
                list = final_list;
            }
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            return list;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if(input==list[i].link_val)
                    return list[i].menu_option_id;
            }
            return input;
        }
    }
}
