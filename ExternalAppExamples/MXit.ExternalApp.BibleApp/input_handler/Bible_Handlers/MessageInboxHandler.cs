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
    class MessageInboxHandler : AInputHandler
    {
        public override void init(UserSession us)
        {
            us.setVariable(CURRENT_MESSAGE_THREAD, "0");
        }

        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            //Console.WriteLine("in input handler: " + input);
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);
            //get reply
            string curr_user_page = user_session.current_menu_loc;

            InputHandlerResult output = handleStdNavLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleMessagePageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleMessageInboxLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }


        protected InputHandlerResult handleMessagePageLinks(
             UserSession user_session,
             string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            //if(user_session.getVariable(CURRENT_THREAD_PAGE)==null)
            //    user_session.setVariable(CURRENT_THREAD_PAGE, 0);
            int current_page_id = Int32.Parse(user_session.getVariable(CURRENT_MESSAGE_THREAD));
            String entry = input.ToUpper();
            if (PREV_PAGE.Equals(entry))
            {
                user_session.setVariable(CURRENT_MESSAGE_THREAD, (current_page_id - 1).ToString());
                return new InputHandlerResult(
                    InputHandlerResult.PREV_PAGE_ACTION,
                    user_session.current_menu_loc,
                    current_page_id - 1); //the menu id is retreived from the session in this case. 
            }
            else if (NEXT_PAGE.Equals(entry))
            {
                user_session.setVariable(CURRENT_MESSAGE_THREAD, (current_page_id + 1).ToString());
                return new InputHandlerResult(
                    InputHandlerResult.NEXT_PAGE_ACTION,
                    user_session.current_menu_loc,
                    current_page_id + 1);
            }
            else if (FIRST_PAGE.Equals(entry))
            {
                user_session.setVariable(CURRENT_MESSAGE_THREAD, "0");
                return new InputHandlerResult(
                    InputHandlerResult.CHANGE_PAGE_ACTION,
                    user_session.current_menu_loc,
                   0);
            }
            else if (entry.StartsWith(LAST_PAGE))
            {
                int page_id = Int32.Parse(entry.Split('_')[1]);
                user_session.setVariable(CURRENT_MESSAGE_THREAD, page_id.ToString());
                return new InputHandlerResult(
                    InputHandlerResult.CHANGE_PAGE_ACTION,
                    user_session.current_menu_loc,
                    page_id);
            }
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }


        /*this method either returns the new screen id or the main or prev command string*/
        protected InputHandlerResult handleMessageInboxLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            String thread_id = "";
            long t_id = -1;
            if (entry.StartsWith(OPEN_THREAD))
            {
                thread_id = entry.Split('_')[1];
                user_session.setVariable(CURRENTLY_VIEWING_TRHEAD, thread_id);

                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.VIEW_THREAD_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (entry.StartsWith(DELETE_THREAD))
            {
                user_session.setVariable(ORIGINAL_ACTION, entry);                
                return new InputHandlerResult(
                    InputHandlerResult.CONF_PAGE_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                   "Are you sure that you want remove this message from your inbox?");
            }
            if (entry.ToUpper().Equals(CONF_YES) || entry.ToUpper().Equals(CONF_Y))
            {
                String original_action = user_session.getVariable(ORIGINAL_ACTION);
                if (original_action != null)
                {
                    user_session.removeVariable(ORIGINAL_ACTION);
                    if (original_action.StartsWith(DELETE_THREAD))
                    {
                        t_id= long.Parse(original_action.Split('_')[1]);
                        VerseMessageThread vmt = VerseThreadManager.getInstance().getVerseMessageThread(t_id);
                        if(vmt != null)
                        {
                            user_session.verse_messaging_manager.removeParticipantFromThread(vmt);
                            return new InputHandlerResult("Message Deleted..");
                        }
                        else{
                            return new InputHandlerResult("Something went wrong when attempting to delete the message from your inbox. Please let us know so that we can look into the issue.");
                        }
                    }

                    return new InputHandlerResult(
                        InputHandlerResult.BACK_MENU_ACTION,
                        InputHandlerResult.DEFAULT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID); //the menu id is retreived from the session in this case. 
                }
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (entry.ToUpper().Equals(CONF_NO) || entry.ToUpper().Equals(CONF_N))
            {
                String original_action = user_session.getVariable(ORIGINAL_ACTION);
                if (original_action != null)
                {
                    user_session.removeVariable(ORIGINAL_ACTION);
                }
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (entry.ToUpper().Equals(REFRESH_INBOX))
            {
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    user_session.current_menu_page);
            }
            return new InputHandlerResult(
                InputHandlerResult.UNDEFINED_MENU_ACTION,
                InputHandlerResult.DEFAULT_MENU_ID,
                InputHandlerResult.DEFAULT_PAGE_ID);
        }

        public const String ORIGINAL_ACTION = "ORIGINAL_THREAD_ACTION";
        public const String OPEN_THREAD = "OPENTHREAD_";
        public const String DELETE_THREAD = "DELETE_";
        public const String REFRESH_INBOX = "REFRESH_INBOX";
        public const String CONF_YES = "YES";
        public const String CONF_NO = "NO";
        public const String CONF_Y = "Y";
        public const String CONF_N = "N";


        public const String CURRENTLY_VIEWING_TRHEAD = "current_thread";
        public const String CURRENT_MESSAGE_THREAD= "current_message_page";
    }


}
