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
    class InputScreenOutputAdapter : AScreenOutputAdapter
    {

        public override MessageToSend getOutputScreenMessage(
            UserSession us, 
            MenuPage mp,
            MessageToSend ms,
            InputHandlerResult ihr)
        {
            ms.Append(MessageBuilder.Elements.CreateClearScreen());
            if (!mp.GetType().FullName.Equals("MxitTestApp.VerseMenuPage"))//TODO: Should be constant
                throw new Exception("Invalid menu page passed into getScreen method ");


            VerseMenuPage omp = (VerseMenuPage)mp;
            ms.Append(omp.title + "\r\n", TextMarkup.Bold);

            if (ihr.error != null && ihr.action == InputHandlerResult.INVALID_MENU_ACTION)
            {
                ms.Append((string)ihr.error + "\r\n");
            }

            Boolean should_display_conf_message = displayMessage(us,ms,ihr);
            if (should_display_conf_message)
            {
                return ms;
            }
            ms.Append(parseMessage(us, omp.message) + "\r\n");

            List<MenuOptionItem> options = omp.options;
            int count =1 ;
            foreach (MenuOptionItem option in options)
            {
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", option.link_val));
                ms.Append(option.display_text + "\r\n");
                count++;
            }
            appendBackMainLinks(us,  ms);
            appendMessageConfig(true, ms);
            return ms;
            //return output;
        }


    }
}
