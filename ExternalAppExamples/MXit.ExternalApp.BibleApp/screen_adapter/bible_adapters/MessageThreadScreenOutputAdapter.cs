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
    class MessageThreadScreenOutputAdapter : AScreenOutputAdapter
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
            if (!mp.GetType().FullName.Equals("MxitTestApp.OptionMenuPage"))//TODO: Should be constant
                throw new Exception("Invalid menu page passed into getScreen method ");

            OptionMenuPage omp = (OptionMenuPage)mp;
            ms.Append(omp.title + "\r\n", TextMarkup.Bold);
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
                    ms.Append(parseMessage(us, omp.message) + "\r\n");
                }
                /*List<MenuOptionItem> options = dmp.options;
                int count =1 ;
                foreach (MenuOptionItem option in options)
                {
                    ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", option.link_val));
                    ms.Append(option.display_text + "\r\n");
                    count++;
                }*/
                //get colour theme
                UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
                int current_page = Int32.Parse(us.getVariable(MessageThreadHandler.CURRENT_THREAD_PAGE));
                int starting_index = current_page * MenuDefinition.PAGE_ITEM_COUNT;
                long thread_id = long.Parse(us.getVariable(MessageInboxHandler.CURRENTLY_VIEWING_TRHEAD));
                VerseMessageThread vmt = VerseThreadManager.getInstance().getVerseMessageThread(thread_id);

                if (vmt != null)
                {
                    if (vmt.getParticipants().Count > 2)
                    {
                        List<VerseMessageParticipant> parts = vmt.getListOfParticipants();

                        List<VerseMessage> messages = vmt.getMessages();
                        VerseMessage first_vm = messages.First();
                        String receivers = getReceiverListWithSender(us, parts, first_vm.sender_id);
                        ms.Append("Recipients: ", TextMarkup.Bold);
                        ms.AppendLine(receivers);
                        ms.AppendLine("");
                        
                        if (uct != null)
                        {
                            Color color = uct.getTipTextColour();
                            ms.Append("Tip: ",color, TextMarkup.Bold);
                            ms.Append("This is a group message. The sender of this message has sent this message to more than one person...so when you reply everyone in the group will see your reply",
                                color);
                        }
                        else
                        {

                            ms.Append("Tip: ", TextMarkup.Bold);
                            ms.Append("This is a group message. The sender of this message has sent this message to more than one person...so when you reply everyone in the group will see your reply");
                        }
                        ms.AppendLine("");
                        ms.AppendLine("");
                    }

                    String subject = vmt.subject;
                    if (!(subject == null || subject.Equals("") || subject.Equals("NULL")))
                    {
                        ms.Append("Subject: ");
                        ms.AppendLine(subject, TextMarkup.Bold);
                        ms.AppendLine("");

                    }


                }
                //TODO: shoud return error message if vmt is null but...

                addThreadMessagesLinks(us, ms, vmt);
                //addQuickFilterLinksToMessageFromList(us, ms);
                //ms.AppendLine("
                
                if (uct != null)
                {
                    Color color = uct.getTipTextColour();
                    ms.AppendLine("Tip: you have to first click reply and go to the reply screen before you can reply to the message. dont just type it in on this screen",color);
                }
                else
                {
                    ms.AppendLine("Tip: you have to first click reply and go to the reply screen before you can reply to the message. dont just type it in on this screen");
                }
                ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Reply", MessageThreadHandler.REPLY));
                ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Refresh", MessageThreadHandler.REFRESH_THREAD));


                appendBackMainLinks(us, ref ms);
                appendMessageConfig(true, ref ms);
            }
            return ms;
            //return output;
        }

        public static String getReceiverListWithSender(UserSession us, List<VerseMessageParticipant> vmp, long sender_id)
        {
            String receiver_list = "";
            if (sender_id == us.user_profile.id)
            {
                receiver_list += "You";
            }
            else
            {
                receiver_list +=  UserNameManager.getUserName(sender_id);
            }
            
                
            String receiver_list_without_sender = MessageInboxScreenOutputAdapter.getRecieverString(us, vmp, sender_id);

            if (receiver_list != "" && receiver_list_without_sender != null && receiver_list_without_sender != "")
                receiver_list += ", " + receiver_list_without_sender;
            else
                receiver_list = receiver_list_without_sender;

            return receiver_list;
        }

        public void addThreadMessagesLinks(
            UserSession us,
            MessageToSend ms,
            VerseMessageThread vmt)
        {
            int current_page = Int32.Parse(us.getVariable(MessageThreadHandler.CURRENT_THREAD_PAGE));
            int count = (current_page * MenuDefinition.PAGE_ITEM_COUNT) + 1;

            int starting_index = current_page * MenuDefinition.PAGE_ITEM_COUNT;
            
            long thread_id = vmt.thread_id;
            List<VerseMessage> messages = vmt.getMessages();
            if (messages.Count() == 0)
            {
                ms.Append("No messages in this thread");
                return;
            }
            ms.AppendLine();
            if (!VerseMessageThread.NOTIFICATION_THREAD.Equals(vmt.start_verse))
            {
                //append verse that was sent too
                Verse start_verse = Verse_Handler.getStartingVerse(vmt.translation.ToString(), vmt.start_verse);
                Verse end_verse;
                if (vmt.end_verse == null || vmt.start_verse.Equals(vmt.end_verse))
                    end_verse = start_verse;
                else if ("NULL".Equals(vmt.end_verse))
                    end_verse = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start_verse);
                else
                    end_verse = Verse_Handler.getStartingVerse(vmt.translation.ToString(), vmt.end_verse);

                List<Verse> list = BrowseBibleScreenOutputAdapter.getVerseList(start_verse, end_verse);
                string section = BibleHelper.getVerseSectionReference(start_verse, end_verse);
                UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
                if (uct != null)
                    ms.Append(section, uct.getBibleTextColour(), TextMarkup.Bold);
                else
                    ms.Append(section, TextMarkup.Bold);
                BibleContainer.getInstance().getBible(
                    start_verse.translation.translation_id).parseAndAppendBibleText(
                        list,
                        ms,
                        uct);
                ms.AppendLine();
                ms.AppendLine();
            }
            IEnumerable<VerseMessage> tmp_list = messages.Reverse<VerseMessage>();
            addMessagesToMessageFromList(us, tmp_list, ms,vmt);
            appendPaginateLinks(us, ref ms, messages.Count, MESSAGES_PER_PAGE);
        }


        //this adds pagination links depending on the count passed into it and the current page the user
        //is on
        public override void appendPaginateLinks(
            UserSession us,
            ref MessageToSend ms,
            int count,
            int count_per_page)
        {
            int cur_page_limit = 0;
            int num_pages = 0;
            int current_page = Int32.Parse(us.getVariable(MessageThreadHandler.CURRENT_THREAD_PAGE));
            if (!us.current_menu_loc.Equals(MenuDefinition.ROOT_MENU_ID))
            {
                cur_page_limit = count_per_page * (current_page + 1);
                int rem = count % count_per_page;
                num_pages = count / count_per_page;
                if (rem > 0)
                    num_pages += 1;
                if ((cur_page_limit > count_per_page))
                {
                    if (num_pages > 2 && current_page > 0)
                    {
                        ms.Append(createMessageLink(MENU_LINK_NAME, "First", AInputHandler.FIRST_PAGE));
                        ms.Append(" | ");
                    }
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Previous", AInputHandler.PREV_PAGE));
                }
                if ((cur_page_limit > count_per_page)
                    && (cur_page_limit < count))
                    ms.Append(" | ");

                if ((cur_page_limit < count))
                {
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Next", AInputHandler.NEXT_PAGE));
                    if (num_pages > 2 && current_page < num_pages - 1)
                    {
                        ms.Append(" | ");
                        ms.Append(createMessageLink(MENU_LINK_NAME, "Last", AInputHandler.LAST_PAGE + "_" + (num_pages - 1)));
                    }
                }
                if (count > count_per_page)
                    ms.Append("\r\n");
            }
        }

        public void addMessagesToMessageFromList(
            UserSession us,
            IEnumerable<VerseMessage> list,
            MessageToSend ms,
            VerseMessageThread vmt)
        {

            DateTime message_sent_date = DateTime.MinValue;
            VerseMessageParticipant vmp = vmt.getParticipant(us.user_profile.id);
            DateTime thread_last_acc_date = DateTime.MaxValue;
            if (vmp != null)
            {
                 thread_last_acc_date = vmp.datetime_last_read;
            }
            int current_page = Int32.Parse(us.getVariable(MessageThreadHandler.CURRENT_THREAD_PAGE));
            int count = (current_page * MESSAGES_PER_PAGE) + 1;
            if (list.Count() > 0)
            {
                if (list.Count() > 1)
                    ms.Append("Messages", TextMarkup.Bold);
                else
                    ms.Append("Message", TextMarkup.Bold);

                if (current_page == 0)
                    ms.AppendLine(" (Newest messages are first)");
                else
                    ms.AppendLine("");
                
                ms.AppendLine("");
            }
            Color aColor;
            int starting_index = current_page * MESSAGES_PER_PAGE;
            for (int i = starting_index;
                i < list.Count() && i < starting_index + MESSAGES_PER_PAGE;
                i++)
            {
                if (i % 2 == 0)
                {
                    aColor = Color.DarkGray;
                }
                else
                {
                    aColor = Color.Gray;
                }
                if (list.ElementAt(i).message_text != null && list.ElementAt(i).message_text != "")
                {
                    UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
                    if (uct != null)
                    {
                        if (list.ElementAt(i).sender_id == us.user_profile.id)
                        {
                            ms.AppendLine("You said:", aColor, TextMarkup.Bold);
                        }
                        else
                        {
                            ms.AppendLine(UserNameManager.getUserName(list.ElementAt(i).sender_id) + " said:", aColor, TextMarkup.Bold);
                        }

                        ms.Append(list.ElementAt(i).message_text + " (" + DateUtils.RelativeDate(list.ElementAt(i).datetime_sent) + ")", aColor);
                        message_sent_date = list.ElementAt(i).datetime_sent;
                        if (thread_last_acc_date < message_sent_date)
                            ms.Append(" (NEW)", aColor, TextMarkup.Bold);
                    }
                    else
                    {
                        if (list.ElementAt(i).sender_id == us.user_profile.id)
                        {
                            ms.AppendLine("You said:", TextMarkup.Bold);
                        }
                        else
                        {
                            ms.AppendLine(UserNameManager.getUserName(list.ElementAt(i).sender_id) + " said:", TextMarkup.Bold);
                        }

                        ms.Append(list.ElementAt(i).message_text + " (" + DateUtils.RelativeDate(list.ElementAt(i).datetime_sent) + ")");
                        message_sent_date = list.ElementAt(i).datetime_sent;
                        if (thread_last_acc_date < message_sent_date)
                            ms.Append(" (NEW)", TextMarkup.Bold);
                    }

                    ms.AppendLine();
                    ms.AppendLine();
                }
                count++;
            }
            us.verse_messaging_manager.updateParticipantThreadLastAccessedTime(vmp);
        }

        public const int SUMMARY_WORD_COUNT = 7;
        public const int MESSAGES_PER_PAGE = 5;
    }
}
