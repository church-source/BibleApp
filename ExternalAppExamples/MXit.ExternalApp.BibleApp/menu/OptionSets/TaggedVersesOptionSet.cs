using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class TaggedVersesOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public TaggedVersesOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            List<VerseTag> tagged_verses = VerseTagManager.getInstance().getListOfVerseTagsForEmotion(Int32.Parse(us.getVariable(SELECTED_EMOTION_VAR_NAME)));
            //LinkedList<VerseHistoryRecord> 

            List<MenuOptionItem> final_list = new List<MenuOptionItem>();
            if (tagged_verses != null)
            {
                if(tagged_verses.Count > 0)
                    tagged_verses.Sort();
                int index = 0;
                foreach (VerseTag verse_tag in tagged_verses)
                {
                    index++;
                    if (verse_tag != null)
                    {
                        try
                        {
                            String start_verse = verse_tag.start_verse;
                            String end_verse = verse_tag.end_verse;

                            /*if (verse_tag.datetime != null && verse_tag.datetime != DateTime.MinValue && !"".Equals(verse_tag.datetime))
                            {
                                date_tagged = verse_tag.datetime.ToString("dd/MM/yy");
                            }
                            else
                            {
                                date_tagged = "";
                            }*/

                            int like_count = verse_tag.getLikeCount();
                            String like_string;
                            if (like_count == 0)
                            {
                                like_string = "";
                            }
                            else if (like_count == 1)
                            {
                                like_string = "1 person likes this";
                            }
                            else
                            {
                                like_string = like_count + " people likes this";
                            }

                            String rel_date = DateUtils.RelativeDate(verse_tag.datetime);

                            Verse s_v = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), start_verse);
                            Verse e_v;
                            if (s_v != null)
                            {
                                if (end_verse == null || "".Equals(end_verse) || end_verse.Equals(start_verse))
                                    e_v = null;
                                else if ("NULL".Equals(end_verse))
                                    e_v = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(s_v);
                                else
                                    e_v = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), end_verse);

                                //VerseRecord vr = new VerseRecord(start_verse, end_verse);
                                String verse_summ = BibleContainer.getSummaryOfVerse(s_v, 7);
                                String user_name = UserNameManager.getInstance().getUserName(verse_tag.user_id);
                                TaggedVerseMenuOptionItem m_o;
                                String verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(s_v, e_v);
                                if (start_verse == null || end_verse == null)
                                {
                                    m_o = new TaggedVerseMenuOptionItem(
                                            (index).ToString(),
                                            (index).ToString()/*(book_list[i].name).ToString()*/,
                                            target_page,
                                            "N/A",
                                            verse_tag);
                                    m_o.is_valid = false;
                                    final_list.Add(m_o);
                                }
                                else
                                {

                                    m_o = new TaggedVerseMenuOptionItem(
                                        (index).ToString(),
                                        verse_ref/*(i + 1).ToString()/*(book_list[i].name).ToString()*/,
                                        target_page,
                                        verse_ref + " (" + rel_date + ") - " + verse_summ + "... \r\nTagged by: " + user_name + " \r\n" + like_string,
                                        verse_tag);
                                    m_o.is_valid = true;
                                    final_list.Add(m_o);
                                }
                            }
                            else
                            {
                                TaggedVerseMenuOptionItem m_o = new TaggedVerseMenuOptionItem(
                                        (index).ToString(),
                                        (index).ToString()/*(book_list[i].name).ToString()*/,
                                        target_page,
                                        "Currently not available in your chosen translation (this could be due to a S/W bug).",
                                        verse_tag);
                                m_o.is_valid = false;
                                final_list.Add(m_o);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                            VerseMenuOptionItem m_o = new VerseMenuOptionItem(
                                        (index).ToString(),
                                        (index).ToString()/*(book_list[i].name).ToString()*/,
                                        target_page,
                                        "N/A",
                                        null);
                            m_o.is_valid = false;
                            final_list.Add(m_o);
                        }
                    }
                   
                }
                return final_list;
            }
            return final_list;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            return input;
        }

        public override InputHandlerResult handleExtraCommandInput(UserSession us, String input)
        {
            return new InputHandlerResult(
                   InputHandlerResult.UNDEFINED_MENU_ACTION,
                   InputHandlerResult.DEFAULT_MENU_ID,
                   InputHandlerResult.DEFAULT_PAGE_ID);
        }

        public const String SELECTED_EMOTION_VAR_NAME = "VerseTagEmotion.emotion_selected";
    }
}
