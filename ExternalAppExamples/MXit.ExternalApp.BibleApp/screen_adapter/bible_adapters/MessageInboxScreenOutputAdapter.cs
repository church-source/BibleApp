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
    class MessageInboxScreenOutputAdapter : AScreenOutputAdapter
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
                addThreadLinks(us, ms);
                //addQuickFilterLinksToMessageFromList(us, ms);
                ms.AppendLine(createMessageLink(MENU_LINK_NAME, "Refresh", MessageInboxHandler.REFRESH_INBOX));
                ms.AppendLine();
                appendBackMainLinks(us, ref ms);
                appendMessageConfig(true, ref ms);
            }
            return ms;
            //return output;
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
            int current_page = Int32.Parse(us.getVariable(MessageInboxHandler.CURRENT_MESSAGE_THREAD));
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


        public void addThreadLinks(
            UserSession us,
            MessageToSend ms)
        {
            int current_page = Int32.Parse(us.getVariable(MessageInboxHandler.CURRENT_MESSAGE_THREAD));
            int count = (current_page * THREAD_COUNT_PER_PAGE) + 1;
            VerseMessageThread vmt;
            int starting_index = current_page * THREAD_COUNT_PER_PAGE;
            List<VerseMessageThread> threads = us.verse_messaging_manager.getParticipatingThreads();
            if (threads.Count() == 0)
            {
                ms.Append("Your inbox is empty");
                return;
            }
            
            for (int i = starting_index;
                i < threads.Count && i < starting_index + THREAD_COUNT_PER_PAGE;
                i++)
            {
                vmt = threads.ElementAt(i);
                if (vmt == null)
                    continue;
                appendMessageThread(us, ms, vmt);
            }
            appendPaginateLinks(us, ref ms, threads.Count, THREAD_COUNT_PER_PAGE);
        }

        protected void appendMessageThread(
            UserSession us,
            MessageToSend ms,
            VerseMessageThread vmt)
        {
            Boolean is_new = false; 
            String verse_ref = "";
            String message;
            List<VerseMessage> messages = vmt.getMessages();
            VerseMessage first_vm = messages.First();
            VerseMessage last_vm = messages.Last();
            
            ms.AppendLine("");
            ms.Append(createMessageLink(MENU_LINK_NAME, "*", MessageInboxHandler.OPEN_THREAD + vmt.thread_id));
            //determine if there is a new message in this thread by comparing last updated and read dates. 
            ms.Append(" ");

            DateTime last_mod_date = vmt.datetime_last_modified;
            VerseMessageParticipant vmp = vmt.getParticipant(us.user_profile.id);
            

            DateTime last_acc_date = DateTime.MaxValue;
            if (vmp != null)
                last_acc_date = vmp.datetime_last_read;
            is_new = last_mod_date > last_acc_date;
            String subject = vmt.subject;
            ms.Append("From: ");
            if (first_vm.sender_id == us.user_profile.id)
            {
                ms.AppendLine("You ", TextMarkup.Bold);
            }
            else
            {
                ms.AppendLine(" " + UserNameManager.getUserName(first_vm.sender_id), TextMarkup.Bold);
            }

            ms.Append("To: ");
            List<VerseMessageParticipant> parts = vmt.getListOfParticipants();
            String receivers = getRecieverString(us, parts,first_vm.sender_id);
            
            ms.AppendLine(receivers, TextMarkup.Bold);

            if (!VerseMessageThread.NOTIFICATION_THREAD.Equals(vmt.start_verse))
            {
                verse_ref = BibleHelper.getVerseSectionReference(us, vmt.start_verse, vmt.end_verse);
                ms.AppendLine("Verse: " + verse_ref);
            }
            if (!(subject == null || subject.Equals("") || subject.Equals("NULL")))
            {
                ms.AppendLine("Subject: '" + subject + "'");
                //ms.Append();
                //ms.AppendLine("'");
            }
            message = last_vm.message_text;
            if(!(message== null || message.Equals("") || message.Equals("NULL")))
                ms.AppendLine("Newest Message: " + StringUtils.getTextSummary(message, 4) + "...'  ("+messages.Count+" messages)");


            //ms.AppendLine("");
            if (last_vm != null)
            {
                ms.Append("Last Updated by ");
                if (last_vm.sender_id == us.user_profile.id)
                {
                    ms.Append("You (");
                }
                else
                {
                    ms.Append(UserNameManager.getUserName(last_vm.sender_id) + " (");
                }
                if (is_new)
                {
                    ms.Append(DateUtils.RelativeDate(last_mod_date),TextMarkup.Bold);
                    ms.AppendLine(")");
                }
                else
                {
                    ms.AppendLine(DateUtils.RelativeDate(last_mod_date) + ")");
                }
            }

            ms.Append(createMessageLink(MENU_LINK_NAME, "[DELETE]", MessageInboxHandler.DELETE_THREAD + vmt.thread_id));
            ms.AppendLine(" ");
            ms.Append("\r\n");

        }

        public static String getRecieverString(UserSession us, List<VerseMessageParticipant> vmp, long sender_id)
        {
            String receiver_list = "";
            int count = 0;
            foreach (var participant in vmp)
            {
                if (participant.user_id != sender_id)
                {
                    if (count == 0)
                    {
                        if (us.user_profile.id == participant.user_id)
                        {
                            receiver_list += "You";
                        }
                        else
                        {
                            receiver_list += UserNameManager.getUserName(participant.user_id);
                        }
                    }
                    else
                    {
                        if (us.user_profile.id == participant.user_id)
                        {
                            receiver_list += ", You";
                        }
                        else
                        {
                            receiver_list += ", " + UserNameManager.getUserName(participant.user_id);
                        }
                    }
                    count++;
                }
            }
            return receiver_list;
        }
        public const int SUMMARY_WORD_COUNT = 7;
        public const int THREAD_COUNT_PER_PAGE = 4;
    }
}
