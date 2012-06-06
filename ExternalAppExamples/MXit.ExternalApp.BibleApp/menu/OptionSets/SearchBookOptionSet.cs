using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class SearchBookOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";

        static List<Book> book_list = new List<Book>();
        static SearchBookOptionSet(){
            book_list = BibleHelper.getListOfBooks();
        }

        public SearchBookOptionSet(String target_page) 
        {
            this.target_page = target_page;
            init();
        }

        public override void init()
        {
            if (book_list != null)
            {
                for (int i = 0; i < book_list.Count; i++)
                {
                    list.Add(
                        new MenuOptionItem(
                            (book_list[i].name).ToString(),
                            (book_list[i].name).ToString(),
                            target_page,
                            book_list[i].name));
                }
            }
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            if (book_list != null)
            {
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                for (int i = 0; i < book_list.Count; i++)
                {
                    if (book_list[i].testament.testament_id
                        == Int32.Parse(us.getVariable(SearchTestamentHandler.SEARCH_TESTAMENT_VAR_NAME)))
                    {
                        final_list.Add(
                            new MenuOptionItem(
                                (book_list[i].name).ToString(),
                                (book_list[i].name).ToString(),
                                target_page,
                                book_list[i].name));
                    }
                }
                return final_list;
            } 
            return null;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if(input==list[i].display_text)
                    return list[i].link_val;
            }

            int starting_index = 0;//us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT;

            string test_id = (String)us.getVariable(SearchTestamentHandler.SEARCH_TESTAMENT_VAR_NAME);
            if (test_id == "1")
                starting_index += 39;

            try{
                int book_id = starting_index + Int32.Parse(input) - 1 ;
                if (book_id < book_list.Count)
                {
                    return book_list.ElementAt(book_id).name;
                }
                else
                {
                    return input;
                }
            }catch(Exception e)
            {
                input = BibleHelper.getFullBookName(input);
                return input;
            }
            //return input;

        }
    }
}
