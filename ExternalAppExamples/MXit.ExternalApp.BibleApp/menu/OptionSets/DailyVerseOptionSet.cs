using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class DailyVerseOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public DailyVerseOptionSet(String target_page) 
        {
            this.target_page = target_page;
        }

        public override void init()
        {
     
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            List<DailyVerse> daily_verse_list = DailyVerseObservable.getInstance().getSentDailyVerses();
            //LinkedList<VerseHistoryRecord> 
            if (daily_verse_list != null)
            {
                DailyVerse dv = null;
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                String date_sent = "";
                for (int i = 0; i < daily_verse_list.Count; i++)
                {
                    if (daily_verse_list[i] != null)
                    {
                        dv = daily_verse_list[i];
                        if (dv.sent_datetime != null && dv.sent_datetime != DateTime.MinValue && !"".Equals(dv.sent_datetime))
                        {
                            date_sent = dv.sent_datetime.ToString("dd/MM/yy");
                        }
                        else
                        {
                            date_sent = "";
                        }
                        String start_verse = dv.verse_ref.Split('-')[0];
                        Verse verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), start_verse);
                        String verse_summ = BibleContainer.getSummaryOfVerse(verse, 7);
                        if (date_sent != "")
                        {
                            final_list.Add(
                               new MenuOptionItem(
                                   (dv.verse_ref).ToString(),
                                   (i + 1).ToString(),
                                   target_page,
                                   dv.verse_ref + " (" + date_sent + ") - " + verse_summ + "..."));
                        }
                        else
                        {
                            final_list.Add(
                               new MenuOptionItem(
                                   (dv.verse_ref).ToString(),
                                   (i + 1).ToString(),
                                   target_page,
                                   dv.verse_ref + " - " + verse_summ + "..."));
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
            //quick hacki
            int index = -1;
            if(Int32.TryParse(input, out index))
            {
                index -= 1;
                if (index < 0)
                {
                    return input;
                }
                List<DailyVerse> daily_verse_list = DailyVerseObservable.getInstance().getSentDailyVerses();
                if (index < daily_verse_list.Count)
                {
                    return daily_verse_list[index].verse_ref;
                }
            }
            return input;
        }

        public override InputHandlerResult handleExtraCommandInput(UserSession us, String input)
        {
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
