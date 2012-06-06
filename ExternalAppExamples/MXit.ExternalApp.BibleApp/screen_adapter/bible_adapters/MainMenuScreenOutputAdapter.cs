using System;
using System.Drawing;
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
    class MainMenuScreenOutputAdapter : AScreenOutputAdapter
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
            if (ihr.action == InputHandlerResult.INVALID_MENU_ACTION 
                && ihr.error != null)
            {
                ms.Append((string)ihr.error + "\r\n");
            }
            else
            {
                ms.Append(parseMessage(us, omp.message) + "\r\n");
            }

            if (us.user_profile.is_suspended)
            {
                ms.Append("\r\n");
                ms.Append("You have been suspended from this application. Please email us at info@BibleApp.za.org.");
                ms.Append("\r\n");
                ms.Append("\r\n");
                return ms;
            }

            if (us.hasVariable(UserSession.GUEST_USER_NAME_ASSIGNED))
            {
                ms.Append("\r\n");
                ms.Append("You have been assigned a guest user name. To remove this message please change your user name in the profile option below.");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }

            if (us.bookmark_manager.bookmark_verse != null)
            {
                ms.Append("To continue reading where you left off ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Click Here", "BOOKMARK"));
                ms.Append("\r\n");
                ms.Append("\r\n");
                ms.Append("Or choose an option below...\r\n");
            }
            else
            {
                ms.Append("Choose an option below...\r\n");
            }


            List<MenuOptionItem> options = omp.options;
            int count =1 ;
            foreach (MenuOptionItem option in options)
            {
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", option.link_val));
                if (option.menu_option_id == MY_PROFILE_OPTION_ID)
                {
                    if (us.hasNewEvent())
                    {
                        ms.Append(option.display_text + " (!)\r\n", TextMarkup.Bold);
                    }
                    else
                    {
                        ms.Append(option.display_text + "\r\n");
                    }
                }
                else
                {
                    ms.Append(option.display_text + "\r\n");
                }

                count++;
            }
            appendBackMainLinks(us, ref ms);
            appendMessageConfig(true, ref ms);

            ms.AppendLine(""); 
            ms.AppendLine("");
            UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
            if (uct != null)
            {
                Color color = uct.getTipTextColour();
                ms.AppendLine("Tip: To Refer a Friend click the below link. ", color);
            }
            else
            {
                ms.AppendLine("Tip: To Refer a Friend click the below link. ");
            }
            ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Spread The Word", MainMenuHandler.REFER_A_FRIEND));

            ms.AppendLine("");
            ms.AppendLine("Shortcuts...", TextMarkup.Bold);
            ms.Append(createMessageLink(MENU_LINK_NAME, "Inbox", MainMenuHandler.MESSAGE_INBOX));
            if (us.hasNewMessageEvent())
            {
                ms.Append(" (NEW)", TextMarkup.Bold);
            }
            
            if (us.hasNewFriendRequest())
            {
                ms.Append(" | ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Buddy Requests", MainMenuHandler.BUDDY_REQUESTS));
                ms.Append(" (NEW)", TextMarkup.Bold);
            }

            ms.Append(" | ");
            ms.Append(createMessageLink(MENU_LINK_NAME, "Help", MainMenuHandler.HELP));

            ms.Append(" | ");
            ms.Append(createMessageLink(MENU_LINK_NAME, "About Us", MainMenuHandler.ABOUT));

            ms.Append(" | ");
            ms.Append(createMessageLink(MENU_LINK_NAME, "Change Colours", MainMenuHandler.COLOUR_CHANGE));


            ms.AppendLine();
            ms.AppendLine();

            if (uct != null)
            {
                Color color = uct.getTipTextColour();
                ms.AppendLine("Tip: Check out the profile section and if your friends use the BibleApp add them as buddies so that you can send them verses. For a start add us, our code is CBTJXP. ",color);
            }
            else
            {
                ms.AppendLine("Tip: Check out the profile section and if your friends use the BibleApp add them as buddies so that you can send them verses. For a start add us, our code is CBTJXP. ");
            }
            ms.AppendLine();
            ms.AppendLine();
            //ADMIN AREA
            if (us.user_profile.is_admin)
            {
                ms.AppendLine("ADMIN AREA");
                ms.AppendLine("");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Send Notification", MainMenuHandler.SEND_NOTIFICATION));
                //ms.AppendLine("");
                //ms.Append(createMessageLink(MENU_LINK_NAME, "Send Notification", MainMenuHandler.SEND_NOTIFICATION));
            }

            return ms;
            //return output;
        }

        public const String MY_PROFILE_OPTION_ID = "5";
        
    }
}
