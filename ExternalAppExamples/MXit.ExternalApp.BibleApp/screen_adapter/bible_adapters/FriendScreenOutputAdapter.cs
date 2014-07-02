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
    class FriendScreenOutputAdapter : AScreenOutputAdapter
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
            ms.Append(dmp.title + "\r\n", TextMarkup.Bold);
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
                addLinksToMessageFromList(us, dyn_options, ms);
                appendPaginateLinks(us, ms, dyn_options.Count);
                addQuickFilterLinksToMessageFromList(us, ms);
                appendExtraCommandLinks(dmp.dynamic_set.getExtraCommandString(), ms);
                appendBackMainLinks(us, ms);
                appendMessageConfig(true,  ms);
            }
            return ms;
            //return output;
        }

        public override void addLinksToMessageFromList(
            UserSession us,
            List<MenuOptionItem> list,
            MessageToSend ms)
        {
            int count = (us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT) + 1;

            int starting_index = us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT;
            FriendRelationMenuOptionItem an_option;
            FriendRelation fr;
            for (int i = starting_index;
                i < list.Count && i < starting_index + MenuDefinition.PAGE_ITEM_COUNT;
                i++)
            {
                an_option = (FriendRelationMenuOptionItem)list.ElementAt(i);
                fr = an_option.fr;
                ms.Append("* " + UserNameManager.getInstance().getUserName(long.Parse(an_option.display_text)));
                ms.Append(" ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "[REMOVE]", "DELETE_" + an_option.display_text));
                ms.Append(" ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "[BLOCK]", "BLOCK_" + an_option.display_text));
                ms.Append(" ");
                ms.Append("\r\n");
                count++;
            }
        }

        public void addQuickFilterLinksToMessageFromList(
            UserSession us,
            MessageToSend ms)
        {
            List<char> starting_chars = us.friend_manager.getStartingCharacters();
            //.. starting_chars
            starting_chars.Sort();
            if (starting_chars.Count() > 1)
            {
                int i = 0;

                foreach (var a_char in starting_chars)
                {
                    if (i == 0)
                    {
                        i++;
                        ms.Append("\r\nFilter - ");
                        ms.Append(createMessageLink(MENU_LINK_NAME, "[ALL]", FriendHandler.FILTER_LIST + "ALL"));
                        ms.Append(" ");
                    }
                    ms.Append(createMessageLink(MENU_LINK_NAME, "[" + a_char.ToString().ToUpper() + "]", FriendHandler.FILTER_LIST + a_char));
                    ms.Append(" ");
                }
                ms.Append("\r\n\r\n");
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
