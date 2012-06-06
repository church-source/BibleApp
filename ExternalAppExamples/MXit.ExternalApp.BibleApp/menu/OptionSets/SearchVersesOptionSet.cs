using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class SearchVersesOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public SearchVersesOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            List<SearchVerseRecord> search_list = us.search_results;
            //LinkedList<VerseHistoryRecord> 
            if (search_list != null)
            {
                String verse_ref = "";
                SearchVerseRecord svr = null;
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                for (int i = 0; i < search_list.Count; i++)
                {
                    if (search_list[i] != null)
                    {
                        svr = search_list[i];
                        //call methods in a handler...not so good. I should of moved this method into a common class
                        Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), svr.start_verse);
                        verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(start_verse, start_verse);
                        final_list.Add(
                            new VerseMenuOptionItem(
                                (i + 1).ToString(),
                                (i+1).ToString()/*(book_list[i].name).ToString()*/,
                                target_page,
                                verse_ref,
                                svr));
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
