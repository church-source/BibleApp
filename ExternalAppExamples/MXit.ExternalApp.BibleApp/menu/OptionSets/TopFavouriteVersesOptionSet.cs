using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class TopFavouriteVersesOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public TopFavouriteVersesOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            List<TopFavouriteVerseRecord> favourite_list = FavouriteVerseManager.getTopFavouriteList();

            if (favourite_list != null)
            {
                String verse_ref = "";
                TopFavouriteVerseRecord tfvr = null;
                MenuOptionItem m_o = null;
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                for (int i = 0; i < favourite_list.Count; i++)
                {
                    if (favourite_list[i] != null)
                    {
                        tfvr = favourite_list[i];
                        //call methods in a handler...not so good. I should of moved this method into a common class
                        Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), tfvr.start_verse);
                        Verse end_verse = start_verse;
                        if (start_verse != null)
                        {
                            verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(start_verse, end_verse);
                            m_o = new VerseMenuOptionItem(
                                    (i + 1).ToString(),
                                    verse_ref/*(i + 1).ToString()/*(book_list[i].name).ToString()*/,
                                    target_page,
                                    verse_ref + " (" + tfvr.verse_count + ")",
                                    tfvr);
                            final_list.Add(m_o);
                        }
                        else
                        {
                            m_o = new VerseMenuOptionItem(
                                        (i + 1).ToString(),
                                        tfvr.start_verse,
                                        target_page,
                                        tfvr.start_verse + " (" + tfvr.verse_count + ")",
                                        tfvr);
                            m_o.is_valid = false;
                            final_list.Add(m_o);
                        }


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
