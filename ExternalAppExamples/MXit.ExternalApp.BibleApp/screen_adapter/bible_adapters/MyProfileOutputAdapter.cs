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
    class MyProfileOutputAdapter : AScreenOutputAdapter
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
            if (us.user_profile.is_suspended)
            {
                ms.Append("You are suspended from the social aspect of this App. You can continue to browse the Bible however.");
                appendBackMainLinks(us,  ms);
                return ms;
            }
            VerseMenuPage omp = (VerseMenuPage)mp;
            ms.Append(omp.title + "\r\n", TextMarkup.Bold);
            ms.Append("\r\n");
            String friend_name = "";
            if (us.getVariable(FriendRequestInputHandler.REQUESTED_FRIEND_NAME) != null)
            {
                friend_name = (String)us.removeVariable(FriendRequestInputHandler.REQUESTED_FRIEND_NAME);
                ms.Append("Your buddy request has been sent to " + friend_name + ". Your buddy will appear in your buddy list once he/she approves the request.");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else if (us.getVariable(FriendRequestHandler.APPROVED_FRIEND_NAME) != null)
            {
                friend_name = (String)us.removeVariable(FriendRequestHandler.APPROVED_FRIEND_NAME);
                ms.Append(friend_name + " has been added to your buddy list. ");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else if (us.getVariable(FriendRequestHandler.REJECTED_FRIEND_NAME) != null)
            {
                friend_name = (String)us.removeVariable(FriendRequestHandler.REJECTED_FRIEND_NAME);
                ms.Append("You have rejected a friend request from "+friend_name + ".");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else if (us.getVariable(FriendHandler.BLOCKED_FRIEND_NAME) != null)
            {
                friend_name = (String)us.removeVariable(FriendHandler.BLOCKED_FRIEND_NAME);
                ms.Append("You have blocked " + friend_name + ". This person will not be able to interact with you until you send a buddy request to them.");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else if (us.getVariable(FriendHandler.DELETED_FRIEND_NAME) != null)
            {
                friend_name = (String)us.removeVariable(FriendHandler.DELETED_FRIEND_NAME);
                ms.Append("You have removed " + friend_name + " from you buddy list.");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }




            if (us.friend_manager.getFriendRequests().Count() > 0)
            {
                ms.Append("You have new buddy requests. To see them ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Click Here", MyProfileHandler.FRIEND_REQUESTS));
                ms.Append("\r\n");
                ms.Append("\r\n");
            }

            ms.Append("Your Bible buddy code is ");
            ms.Append(us.user_profile.user_profile_custom.user_code, TextMarkup.Bold);
            ms.Append(". ");
            ms.AppendLine(createMessageLink(MENU_LINK_NAME, " ? ", MyProfileHandler.BUDDY_CODE_HELP));
            //get colour theme
            UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
            Color color = Color.Empty;
            if (uct != null)
                color = uct.getTipTextColour();

            if (uct != null)
            {
                ms.Append("Tip: give this code to your friends so that they can add you as a buddy on the BibleApp. Then you can send each other verses. Or ask them for their code and use the buddy request option below to add them",
                    color);
            }
            else
            {
                ms.Append("Tip: give this code to your friends so that they can add you as a buddy on the BibleApp. Then you can send each other verses. Or ask them for their code and use the buddy request option below to add them");
            }
            ms.Append("\r\n");
            ms.Append("\r\n");

            ms.Append("Your current User Name is ");
            ms.Append(us.user_profile.user_profile_custom.user_name, TextMarkup.Bold);
            ms.Append(". ");
            ms.Append(createMessageLink(MENU_LINK_NAME, " ? ", MyProfileHandler.USER_NAME_HELP));
            ms.Append(" | ");
            ms.Append(createMessageLink(MENU_LINK_NAME, "Change", MyProfileHandler.USER_NAME_CHANGE));
            ms.Append("\r\n");
            ms.Append("\r\n");

            if (us.friend_manager.getFriends().Count() > 0)
            {
                ms.Append("To see your buddy list ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Click Here", MyProfileHandler.FRIENDS));
                ms.Append("\r\n");
                ms.Append("\r\n");
                ms.Append("To send a buddy request ");
            }
            else
            {
                ms.Append("You have not added any buddies yet, to send a buddy request ");
            }
            ms.Append(createMessageLink(MENU_LINK_NAME, "Click Here", MyProfileHandler.SEND_FRIEND_REQUESTS));
            ms.Append("\r\n");
            ms.Append("\r\n");

            if (us.verse_messaging_manager.getParticipatingThreads().Count() > 0)
            {
                ms.Append(createMessageLink(MENU_LINK_NAME, "Message Inbox", MyProfileHandler.MESSAGE_INBOX));
                if (us.verse_messaging_manager.isAThreadUpdatedSinceLastAccess())
                    ms.Append(" (New)", TextMarkup.Bold);

                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else
            {
                ms.Append("Your Message Inbox is empty");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }

            if (us.user_profile.user_profile_custom.is_subscribed_to_dv)
            {
                ms.Append("You are currently ");
                ms.Append("subscribed ",TextMarkup.Bold);
                ms.Append("to receive daily verses from the BibleApp. To unsubscribe ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Click Here", MyProfileHandler.DAILY_VERSE_UNSUBSCRIBE));
                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else
            {
                ms.Append("You are currently ");
                ms.Append("not subscribed ", TextMarkup.Bold);
                ms.Append("to receive daily verses from the BibleApp. To subscribe ");
                ms.Append(createMessageLink(MENU_LINK_NAME, "Click Here", MyProfileHandler.DAILY_VERSE_SUBSCRIBE));
                ms.Append("\r\n");
                ms.Append("\r\n");
            }

            ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Refresh", MyProfileHandler.REFRESH_PROFILE));
            appendBackMainLinks(us, ms);
            appendMessageConfig(true,  ms);
            return ms;
            //return output;
        }
    }
}
