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
    class NotifMessageSendOutputAdapter : AScreenOutputAdapter
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
            ms.Append("\r\n");
            ms.Append("\r\n");
            if (ihr.error != null && ihr.action == InputHandlerResult.INVALID_MENU_ACTION)
            {
                ms.Append((string)ihr.error + "\r\n");
            }

            Boolean should_display_conf_message = displayMessage(us, ms, ihr);
            if (should_display_conf_message)
            {
                return ms;
            }
            ms.Append(parseMessage(us, omp.message) + "\r\n");

            /*else if (us.getVariable(FriendHandler.DELETED_FRIEND_NAME) != null)
            {
                friend_name = (String)us.removeVariable(FriendHandler.DELETED_FRIEND_NAME);
                ms.Append("You have removed " + friend_name + " from you buddy list.");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }*/


                Boolean recip_is_set = false;
                //check if recipient is set already
                if (us.hasVariable(RECIPIENT_ID))
                {
                    String friend_id = us.getVariable(RECIPIENT_ID);
                    long l_friend_id = long.Parse(friend_id);
                    String user_name = UserNameManager.getInstance().getUserName(l_friend_id);
                    ms.Append("To: ");
                    ms.Append(user_name, TextMarkup.Bold);
                    ms.Append(" ");
                    ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", NotifMessageSendHandler.CHOOSE_FRIEND_ID));
                    recip_is_set = true;
                }
                else
                {
                    ms.Append("To: ");
                    ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", NotifMessageSendHandler.CHOOSE_FRIEND_ID));
                    ms.Append(" *");
                }
                
                
                ms.Append("\r\n");
                ms.Append("\r\n");

                if (us.hasVariable(MESSAGE_SUBJECT))
                {
                    String subject = us.getVariable(MESSAGE_SUBJECT);
                    ms.Append("Subject: ");
                    ms.Append(subject);
                    ms.Append(" ");
                }
                else
                {
                    ms.Append("Subject: ");
                }
                ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", NotifMessageSendHandler.ENTER_MESSAGE_SUBJECT));
                ms.Append("\r\n");
                ms.Append("\r\n");

                if (us.hasVariable(MESSAGE_TEXT))
                {
                    String message = us.getVariable(MESSAGE_TEXT);
                    ms.Append("Message: ");
                    ms.Append(message);
                    ms.Append(" ");
                }
                else
                {
                    ms.Append("Message: ");
                }
                ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", NotifMessageSendHandler.ENTER_MESSAGE));
                ms.Append("\r\n");
                ms.Append("\r\n");

                if (!recip_is_set)
                    ms.AppendLine("Fields marked with * has to be set before you can send the message");
                else
                    ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Send Message", NotifMessageSendHandler.SEND_MESSAGE));

            ms.AppendLine("");
            appendBackMainLinks(us, ms);
            appendMessageConfig(true,  ms);
            return ms;
            //return output;
        }
        public const String RECIPIENT_ID = "RecipientID";
        public const String MESSAGE_SUBJECT = "MessageSubject";
        public const String MESSAGE_TEXT = "MessageText";
    }
}
