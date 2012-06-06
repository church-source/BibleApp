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
    class MyProfileHandler : AInputHandler
    {
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

            output = handleMyProfileLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected InputHandlerResult handleMyProfileLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            if (BUDDY_CODE_HELP.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.BIBLE_BUDDY_CODE_HELP_ID); //the menu id is retreived from the session in this case. 
            }
            else if (USER_NAME_CHANGE.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.CHANGE_USER_NAME_ID); //the menu id is retreived from the session in this case.
            }
            else if (USER_NAME_HELP.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.CHANGE_USER_NAME_HELP_ID); //the menu id is retreived from the session in this case.
            }
            else if (FRIEND_REQUESTS.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.MY_FRIEND_REQUESTS_ID); //the menu id is retreived from the session in this case.
            }
            else if (SEND_FRIEND_REQUESTS.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.SEND_FRIEND_REQUESTS_ID); //the menu id is retreived from the session in this case.
            }
            else if (FRIENDS.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.MY_FRIENDS_ID); //the menu id is retreived from the session in this case.
            }
            else if (MESSAGE_INBOX.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.MESSAGE_INBOX_ID); //the menu id is retreived from the session in this case.
            }
            else if (REFRESH_PROFILE.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    user_session.current_menu_page);
            }
            else if (DAILY_VERSE_SUBSCRIBE.Equals(entry))
            {
                user_session.user_profile.setIsSubscribedToDailyVerseAndUpdateDB(true);
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    user_session.current_menu_page);
            }
            else if (DAILY_VERSE_UNSUBSCRIBE.Equals(entry))
            {
                user_session.user_profile.setIsSubscribedToDailyVerseAndUpdateDB(false);
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    user_session.current_menu_page);
            }
            else if (entry.StartsWith("CREATE_"))
            {
                String[] array = entry.Split('_');
                user_session.verse_messaging_manager.createThreadAndAddPrivateMessage(array[2], long.Parse(array[1]), "Romans 8:28", "Romans 8:30", "In all things...");
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        public const String BUDDY_CODE_HELP = "HELP_BUDDY_CODE";
        public const String USER_NAME_CHANGE = "CHANGE_USER_NAME";
        public const String USER_NAME_HELP = "HELP_USER_NAME";
        public const String FRIEND_REQUESTS = "FRIEND_REQUESTS";
        public const String FRIENDS = "FRIENDS";
        public const String MESSAGE_INBOX = "MESSAGE_INBOX";
        public const String SEND_FRIEND_REQUESTS = "SEND_FRIEND_REQUESTS";
        public const String DAILY_VERSE_SUBSCRIBE = "DAILY_VERSE_SUBSCRIBE";
        public const String DAILY_VERSE_UNSUBSCRIBE = "DAILY_VERSE_UNSUBSCRIBE";
        public const String REFRESH_PROFILE = "REFRESH_PROFILE";
       
    }


}
