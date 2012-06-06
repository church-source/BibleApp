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
    class VerseListScreenOutputAdapter : AScreenOutputAdapter
    {

        //in here we should rather call a this method and from here call the implemented output screen 
        //message method so that we can do common things in here. anyway too late now. 
        public override MessageToSend getOutputScreenMessage(
            UserSession us, 
            MenuPage mp,
            MessageToSend ms,
            InputHandlerResult ihr)
        {
            ms.Append(MessageBuilder.Elements.CreateClearScreen());
            if (!mp.GetType().FullName.Equals("MxitTestApp.DynMenuPage"))//TODO: Should be constant
                throw new Exception("Invalid menu page passed into getScreen method ");
            
            DynMenuPage dmp = (DynMenuPage)mp;
            ms.Append(dmp.title + "\r\n",TextMarkup.Bold);
            if (ihr.action == InputHandlerResult.CONF_PAGE_ACTION
                 && ihr.message != null)
            {
                ms.Append(ihr.message + "\r\n");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Y", "Yes"));
                ms.Append(" | ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "N", "No"));
            }
            else
            {
                if (ihr.action == InputHandlerResult.INVALID_MENU_ACTION
                && ihr.error != null)
                {
                    ms.Append((string)ihr.error + "\r\n");
                }
                else
                {
                    ms.Append(parseMessage(us, dmp.message) + "\r\n");
                }
                /*List<MenuOptionItem> options = dmp.options;
                int count =1 ;
                foreach (MenuOptionItem option in options)
                {
                    ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", option.link_val));
                    ms.Append(option.display_text + "\r\n");
                    count++;
                }*/
                List<MenuOptionItem> dyn_options = dmp.dynamic_set.getOptionList(us);
                if (dyn_options.Count() == 0)
                {
                    String empty_msg = dmp.dynamic_set.getListEmptyMessage();
                    if (empty_msg != null && empty_msg != "")
                        ms.Append(dmp.dynamic_set.getListEmptyMessage() + "\r\n");
                }
                addLinksToMessageFromList(us, dyn_options, ref ms);

                appendPaginateLinks(us, ref ms, dyn_options.Count);
                appendExtraCommandLinks(dmp.dynamic_set.getExtraCommandString(), ms);
                appendBackMainLinks(us, ref ms);
                appendMessageConfig(true, ref ms);
            }
            return ms;
            //return output;
        }

        public override void addLinksToMessageFromList(
            UserSession us,
            List<MenuOptionItem> list,
            ref MessageToSend ms)
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
                //ms.Append(createMessageLink(MENU_LINK_NAME, "[x]", "del:"+ count));
                ms.Append("\r\n");
                count++;
            }
        }

        public void appendExtraCommandLinks(String extra_commands, MessageToSend ms)
        {
            if (!(extra_commands == ""))
            {
                //TODO Complete this to split on | and add different commands
                ms.Append(createMessageLink(MENU_LINK_NAME, extra_commands, extra_commands.ToUpper()));
                ms.Append("\r\n");
            }
        }

        public const int SUMMARY_WORD_COUNT = 7;
    }


}
