using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class VerseHistoryOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public VerseHistoryOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            ReadOnlyCollection<VerseHistoryRecord> history_list = us.verse_history.getHistoryListForDisplay();
            //LinkedList<VerseHistoryRecord> 
            if (history_list != null)
            {
                String verse_ref = "";
                VerseHistoryRecord vhr = null;
                MenuOptionItem m_o = null;
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                for (int i = 0; i < history_list.Count; i++)
                {
                    if (history_list[i] != null)
                    {
                        vhr = history_list[i];
                        //call methods in a handler...not so good. I should of moved this method into a common class
                        Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), vhr.start_verse);
                        if (start_verse != null)
                        {
                            Verse end_verse;
                            if (vhr.end_verse == null || vhr.start_verse.Equals(vhr.end_verse))
                                end_verse = null;
                            else if ("NULL".Equals(vhr.end_verse))
                                end_verse = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start_verse);
                            else
                                end_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), vhr.end_verse);

                            verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(start_verse, end_verse);
                            m_o = new VerseMenuOptionItem(
                                    vhr.id.ToString(),
                                    (i + 1).ToString()/*(book_list[i].name).ToString()*/,
                                    target_page,
                                    verse_ref + " (" + vhr.datetime.ToString("dd/MM/yyyy") + ")",
                                    vhr);
                            final_list.Add(m_o);
                        }
                        else
                        {
                            m_o = new VerseMenuOptionItem(
                                        vhr.id.ToString(),
                                        (i + 1).ToString(),
                                        target_page,
                                        vhr.start_verse + " (" + vhr.datetime.ToString("dd/MM/yyyy") + ")",
                                        vhr);
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

        public override InputHandlerResult handleExtraCommandInput(UserSession us, String input)
        {
            if (getExtraCommandString() != null && getExtraCommandString() != "")
            {
                
              //  Boolean confirmed_delete;
                Boolean is_confirming = false;
                try
                {
                    Object o = us.removeVariable("ConfirmedHistoryDelete");
                    if (o != null)
                    {
                        is_confirming = (Boolean)o;
                    }
                    else
                    {
                        is_confirming = false;
                    }
                    /*is_confirming = true;*/
                }
                catch (Exception e)
                {
                    is_confirming = false;
                    /*is_confirming = false;*/
                }

                if (!is_confirming && CLEAR_HISTORY.Equals(input.ToUpper()))
                {
                    us.setVariable("ConfirmedHistoryDelete", true);
                    return new InputHandlerResult(
                            InputHandlerResult.CONF_PAGE_ACTION,
                            InputHandlerResult.DEFAULT_MENU_ID,
                           "Are you sure that you want to clear the history (Y/N)?");
                }
                if (is_confirming)
                {
                    if (CONFIRMED_DELETE.Equals(input.ToUpper()) || CONFIRMED_DELETE_2.Equals(input.ToUpper()))
                    {
                        us.verse_history.clearHistory(us.user_profile);
                        return new InputHandlerResult("The history has been cleared");
                    }
                    else if (CANCELLED_DELETE.Equals(input.ToUpper()) || CANCELLED_DELETE_2.Equals(input.ToUpper()))
                    {
                        return new InputHandlerResult();
                    }
                }
                if (!is_confirming)
                {
                    return new InputHandlerResult(
                           InputHandlerResult.UNDEFINED_MENU_ACTION,
                           InputHandlerResult.DEFAULT_MENU_ID,
                           InputHandlerResult.DEFAULT_PAGE_ID);
                }
            }
            return new InputHandlerResult(
                   InputHandlerResult.UNDEFINED_MENU_ACTION,
                   InputHandlerResult.DEFAULT_MENU_ID,
                   InputHandlerResult.DEFAULT_PAGE_ID);
        }

        public const String CLEAR_HISTORY = "CLEAR";
        public const String CONFIRMED_DELETE = "Y";
        public const String CONFIRMED_DELETE_2 = "YES";

        public const String CANCELLED_DELETE = "N";
        public const String CANCELLED_DELETE_2 = "NO";
    }
}
