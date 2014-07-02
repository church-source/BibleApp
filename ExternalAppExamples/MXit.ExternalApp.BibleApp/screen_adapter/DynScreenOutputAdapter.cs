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
    class DynScreenOutputAdapter : AScreenOutputAdapter
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

            Boolean should_display_conf_message = displayMessage(us, ms, ihr);
            if (should_display_conf_message)
            {
                return ms;
            }

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
                addLinksToMessageFromList(us, dyn_options,  ms);
                appendPaginatedLinksForMenu(us, ms, dyn_options);
                appendExtraCommandLinks(dmp.dynamic_set.getExtraCommandString(), ms);
                appendBackMainLinks(us, ms);
            }
            appendMessageConfig(true, ms);

            return ms;
            //return output;
        }

        public void appendExtraCommandLinks(String extra_commands, MessageToSend ms)
        {
            if (! (extra_commands == ""))
            {
                //TODO Complete this to split on | and add different commands
                ms.Append(createMessageLink(MENU_LINK_NAME, extra_commands, extra_commands.ToUpper()));
                ms.Append("\r\n");
            }
        }

        protected virtual void appendPaginatedLinksForMenu(UserSession us, MessageToSend ms, List<MenuOptionItem> dyn_options)
        {
              appendPaginateLinks(us, ms, dyn_options.Count);
        }
    }
}
