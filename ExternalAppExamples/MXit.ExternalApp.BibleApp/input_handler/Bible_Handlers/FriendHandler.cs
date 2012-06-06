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
    class FriendHandler : AInputHandler
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

            output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleFriendLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected InputHandlerResult handleFriendLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            long friend_id = -1;
            if (entry.StartsWith(BLOCK_FRIEND))
            {
                user_session.setVariable(ORIGINAL_ACTION, entry);
                friend_id = long.Parse(entry.Split('_')[1]);
                String user_name = UserNameManager.getUserName(friend_id);

                return new InputHandlerResult(
                    InputHandlerResult.CONF_PAGE_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                   "Are you sure that you want block " + user_name +"?");
            }
            else if (entry.StartsWith(DELETE_FRIEND))
            {
                user_session.setVariable(ORIGINAL_ACTION, entry);
                friend_id = long.Parse(entry.Split('_')[1]);
                String user_name = UserNameManager.getUserName(friend_id);

                return new InputHandlerResult(
                    InputHandlerResult.CONF_PAGE_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                   "Are you sure that you want remove " + user_name + " from your buddy list?");
            }
            if (entry.ToUpper().Equals(CONF_YES) || entry.ToUpper().Equals(CONF_Y))
            {
                String original_action = user_session.getVariable(ORIGINAL_ACTION);
                if(original_action != null)
                {
                    user_session.removeVariable(ORIGINAL_ACTION);
                    if (original_action.StartsWith(BLOCK_FRIEND))
                    {
                        friend_id = long.Parse(original_action.Split('_')[1]);
                        String user_name = UserNameManager.getUserName(friend_id);
                        user_session.friend_manager.blockFriend(friend_id);
                        user_session.setVariable(BLOCKED_FRIEND_NAME, user_name);
                    }else if(original_action.StartsWith(DELETE_FRIEND))
                    {
                        friend_id = long.Parse(original_action.Split('_')[1]);
                        user_session.friend_manager.deleteFriendRequest(friend_id);
                        String user_name = UserNameManager.getUserName(friend_id);
                        user_session.setVariable(DELETED_FRIEND_NAME, user_name);
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
                    if(original_action != null)
                    {
                        user_session.removeVariable(ORIGINAL_ACTION);
                    }
                    return new InputHandlerResult(
                        InputHandlerResult.DO_NOTHING_ACTION,
                        InputHandlerResult.DEFAULT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (entry.StartsWith(FILTER_LIST))
            {
                String filter = entry.Split('_')[1];
                user_session.setVariable(FRIEND_LIST_FILTER, filter);

                    return new InputHandlerResult(
                        InputHandlerResult.DO_NOTHING_ACTION,
                        InputHandlerResult.DEFAULT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else
            {
                String original_action = user_session.getVariable(ORIGINAL_ACTION);
                if (original_action != null)
                {
                    user_session.removeVariable(ORIGINAL_ACTION);
                }
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        public const String ORIGINAL_ACTION = "ORIGINAL_ACTION";
        public const String BLOCK_FRIEND = "BLOCK_";
        public const String DELETE_FRIEND = "DELETE_";
        public const String CONF_YES = "YES";
        public const String CONF_NO = "NO";
        public const String CONF_Y = "Y";
        public const String CONF_N = "N";
        public const String FILTER_LIST = "FILTER_";

        public const String BLOCKED_FRIEND_NAME = "Friend.friend_blocked";
        public const String DELETED_FRIEND_NAME = "Friend.friend_deleted";

        public const String FRIEND_LIST_FILTER =  "Friend.filter";
    }


}
