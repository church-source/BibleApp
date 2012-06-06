using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class ChapterOptionSet : AMenuDynamicOptionSet
    {
        
       
        private String target_page = "";
        static Hashtable book_chapters = new Hashtable();
        static ChapterOptionSet()
        {
            book_chapters = BibleHelper.getListOfChapters();
        }

        public ChapterOptionSet(String target_page) 
        {
            this.target_page = target_page;
            init();
        }

        public override void init()
        {
            //nothing to do here
        }


        //TODO: check this method. check if it will work in multi threaded app
        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            String selected_book_id = us.getVariable("BookOptionSet.book_id");
            //we should always get a result here, because the above variable is set in 
            //a previous menu. but change this
            String string_chapter_count = (String)book_chapters[selected_book_id];
            if (string_chapter_count != null)
            {
                int chapter_count = (Int32.Parse(string_chapter_count));
                List<MenuOptionItem> list = new List<MenuOptionItem>();

                for (int i = 0; i < chapter_count; i++)
                {
                    list.Add(
                        new MenuOptionItem(
                            (i + 1).ToString(),
                            (i + 1).ToString(),
                            target_page,
                            "Chapter " + (i + 1)));
                }

                return list;
            }
            return null;
        }

        public override string parseInput(String input, UserSession us)
        {
            return input;
        } 
    }
}
