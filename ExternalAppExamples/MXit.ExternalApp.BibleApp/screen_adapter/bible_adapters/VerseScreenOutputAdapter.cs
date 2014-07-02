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
    class VerseScreenOutputAdapter : AScreenOutputAdapter
    {

        public override MessageToSend getOutputScreenMessage(
            UserSession us, 
            MenuPage mp,
            MessageToSend ms,
            InputHandlerResult ihr)
        {
            ms.Append(MessageBuilder.Elements.CreateClearScreen());
            if (!mp.GetType().FullName.Equals("MxitTestApp.OptionMenuPage"))//TODO: Should be constant
                throw new Exception("Invalid menu page passed into getScreen method ");

            OptionMenuPage omp = (OptionMenuPage)mp;
            
            ms.Append(omp.title + "\r\n", TextMarkup.Bold);
            if (ihr.error != null && ihr.action == InputHandlerResult.INVALID_MENU_ACTION)
            {
                ms.Append((string)ihr.error + "\r\n");
            }

            ms.Append(omp.message + "\r\n");
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

        }

    }
}
