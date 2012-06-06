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
    class VerseMessageSendOutputAdapter : AScreenOutputAdapter
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

            if (us.friend_manager.getFriends().Count <= 0)
            {
                ms.Append("You dont have any buddies added, you need to first invite buddies in order to send Verses to them.");
                ms.Append("\r\n");
                ms.Append("\r\n");
            }
            else
            {
                Boolean recip_is_set = false;
                //check if recipient is set already


                if (us.hasVariable(ChooseFriendHandler.RECIPIENT_LIST) && ((List<long>)us.getVariableObject(ChooseFriendHandler.RECIPIENT_LIST)).Count > 0)
                {
                    String friend_list = getCurrentSendList(us);
                    ms.Append("To: ");
                    ms.Append(friend_list);
                    ms.Append(" ");
                    ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", VerseMessageSendHandler.CHOOSE_FRIEND_ID));
                    recip_is_set = true;
                }
                else
                {
                    ms.Append("To: ");
                    ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", VerseMessageSendHandler.CHOOSE_FRIEND_ID));
                    ms.Append(" *");
                }
                
                
                ms.Append("\r\n");
                ms.Append("\r\n");

                VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
                if (vs == null)
                {
                    Console.WriteLine("Expected Browse.verse_section present, but not found");
                }
                else
                {
                    Verse start_verse = vs.start_verse;
                    Verse end_verse = vs.end_verse;
                    if (end_verse == null)
                    {
                        end_verse = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start_verse);
                    }


                    ms.Append("Verse: ");
                    ms.Append(BibleHelper.getVerseSectionReference(start_verse, end_verse), TextMarkup.Bold);
                    ms.Append("\r\n");
                    ms.Append("\r\n");
                }

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
                ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", VerseMessageSendHandler.ENTER_MESSAGE_SUBJECT));
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
                ms.Append(createMessageLink(MENU_LINK_NAME, "[ edit ]", VerseMessageSendHandler.ENTER_MESSAGE));
                ms.Append("\r\n");
                ms.Append("\r\n");

                if (!recip_is_set)
                    ms.AppendLine("Fields marked with * has to be set before you can send the message");
                else
                    ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Send Message", VerseMessageSendHandler.SEND_MESSAGE));
            }
            ms.AppendLine("");
            appendBackMainLinks(us, ref ms);
            appendMessageConfig(true, ref ms);
            return ms;
            //return output;
        }

        public String getCurrentSendList(UserSession us)
        {
            String send_list = "";
            if (us.hasVariable(ChooseFriendHandler.RECIPIENT_LIST))
            {
                List<long> recipient_list = (List<long>)us.getVariableObject(ChooseFriendHandler.RECIPIENT_LIST);
                for (int i = 0; i < recipient_list.Count; i++)
                {
                    send_list += (UserNameManager.getUserName(recipient_list[i]));
                    if (recipient_list.Count > 1 && (i != recipient_list.Count - 1))
                        send_list+=", ";
                }
            }
            return send_list;
        }
        public const String FRIEND_TO_SEND_ID = "FriendToSendID";
        public const String MESSAGE_SUBJECT = "MessageSubject";
        public const String MESSAGE_TEXT = "MessageText";
    }
}
