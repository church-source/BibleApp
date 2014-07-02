using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;

namespace MxitTestApp
{
    class FavouriteListScreenOutputAdapter : VerseListScreenOutputAdapter
    {
        public override void addLinksToMessageFromList(
            UserSession us,
            List<MenuOptionItem> list,
            MessageToSend ms)
        {
            int count = (us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT) + 1;

            int starting_index = us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT;
            VerseMenuOptionItem an_option;
            String summary = "";
            for (int i = starting_index;
                i < list.Count && i < starting_index + MenuDefinition.PAGE_ITEM_COUNT;
                i++)
            {
                an_option = (VerseMenuOptionItem)list.ElementAt(i);
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", an_option.link_val));
                ms.Append(an_option.display_text);
                String start_verse = an_option.fvr.start_verse;
                Verse verse_summ = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), an_option.fvr.start_verse);
                //NetBible method should not be used because this is not always a NET Bible


                if (an_option.is_valid && verse_summ != null)
                {
                    summary = BibleContainer.getSummaryOfVerse(verse_summ, SUMMARY_WORD_COUNT);
                    ms.Append(" - " + summary + "...");
                }
                else
                {
                    ms.Append(" - The verse is not available in this translation", TextMarkup.Bold);
                }


                ms.Append(createMessageLink(MENU_LINK_NAME, "[x]", "del:"+ count));
                ms.Append("\r\n");
                count++;
            }
        }

        public const int SUMMARY_WORD_COUNT = 7;
    }


}
