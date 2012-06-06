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
    class MessageThreadHandler : AInputHandler
    {
        public override void init(UserSession us)
        {
            us.setVariable(CURRENT_THREAD_PAGE, "0");
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
            {
                //map to without init
                if (output.action == InputHandlerResult.BACK_MENU_ACTION)
                    output.action = InputHandlerResult.BACK_WITHOUT_INIT_MENU_ACTION;
                return output;
            }

            output = handleMessageThreadLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleThreadPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        protected InputHandlerResult handleThreadPageLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            //if(user_session.getVariable(CURRENT_THREAD_PAGE)==null)
            //    user_session.setVariable(CURRENT_THREAD_PAGE, 0);
            int current_page_id = Int32.Parse(user_session.getVariable(CURRENT_THREAD_PAGE));
            String entry = input.ToUpper();
            if (PREV_PAGE.Equals(entry))
            {
                user_session.setVariable(CURRENT_THREAD_PAGE, (current_page_id - 1).ToString());
                return new InputHandlerResult(
                    InputHandlerResult.PREV_PAGE_ACTION,
                    user_session.current_menu_loc,
                    current_page_id - 1); //the menu id is retreived from the session in this case. 
            }
            else if (NEXT_PAGE.Equals(entry))
            {
                user_session.setVariable(CURRENT_THREAD_PAGE, (current_page_id + 1).ToString());
                return new InputHandlerResult(
                    InputHandlerResult.NEXT_PAGE_ACTION,
                    user_session.current_menu_loc,
                    current_page_id + 1);
            }
            else if (FIRST_PAGE.Equals(entry))
            {
                user_session.setVariable(CURRENT_THREAD_PAGE, "0");
                return new InputHandlerResult(
                    InputHandlerResult.CHANGE_PAGE_ACTION,
                    user_session.current_menu_loc,
                   0);
            }
            else if (entry.StartsWith(LAST_PAGE))
            {
                int page_id = Int32.Parse(entry.Split('_')[1]);
                user_session.setVariable(CURRENT_THREAD_PAGE, page_id.ToString());
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
        protected InputHandlerResult handleMessageThreadLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            if (entry.StartsWith(REPLY))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.REPLY_TO_THREAD_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (entry.ToUpper().Equals(REFRESH_THREAD))
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

        public const String CURRENT_THREAD_PAGE = "CURRENT_THREAD_PAGE";
        public const String ORIGINAL_ACTION = "ORIGINAL_ACTION";
        public const String REPLY = "REPLY";
        public const String CONF_YES = "YES";
        public const String CONF_NO = "NO";
        public const String CONF_Y = "Y";
        public const String CONF_N = "N";
        public const String REFRESH_THREAD = "REFRESH_THREAD";
        public const String CURRENTLY_VIEWING_TRHEAD = "current_thread";

    }


}
