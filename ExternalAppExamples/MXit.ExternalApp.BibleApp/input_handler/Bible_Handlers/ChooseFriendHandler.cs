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
    class ChooseFriendHandler : AInputHandler
    {

        public override void init(UserSession us)
        {
            if (us.hasVariable(FRIEND_LIST_FILTER))
                us.removeVariable(FRIEND_LIST_FILTER);
        }

        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            //Console.WriteLine("in input handler: " + input);
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);            
            //get reply
            string curr_user_page = user_session.current_menu_loc;

            InputHandlerResult output = handleStdNavLinks(user_session, input,true);
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
            String friend_id ="";
            /*if (entry.StartsWith(RECIPIENT_SEND))
            {
                friend_id = entry.Split('_')[1];
                user_session.setVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID,friend_id);

                return new InputHandlerResult(
                    InputHandlerResult.BACK_WITHOUT_INIT_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }*/
            if (entry.StartsWith(RECIPIENT_ADD))
            {
                List<long> recipient_list = null;
                if (!user_session.hasVariable(RECIPIENT_LIST))
                {
                    recipient_list = new List<long>();
                    user_session.setVariable(RECIPIENT_LIST, recipient_list);
                }
                else
                {
                    recipient_list = (List<long>)user_session.getVariableObject(RECIPIENT_LIST);
                }

                friend_id = entry.Split('_')[1];
                long f_id = long.Parse(friend_id);
                String name = UserNameManager.getUserName(f_id);
                if (!recipient_list.Contains(f_id))
                    recipient_list.Add(f_id);
                    //user_session.setVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID, friend_id);
                else
                    return new InputHandlerResult(
                    name + " is already in the list of recipients. ");

                return new InputHandlerResult(
                    name + " has been added to the list of recipients.");
            }
            if (entry.StartsWith(RECIPIENT_REMOVE))
            {
                List<long> recipient_list = null;
                if (!user_session.hasVariable(RECIPIENT_LIST))
                {

                    return new InputHandlerResult(
                        "The chosen buddy is not in the current buddy send list.");
                }
                else
                {
                    recipient_list = (List<long>)user_session.getVariableObject(RECIPIENT_LIST);
                }

                friend_id = entry.Split('_')[1];
                long f_id = long.Parse(friend_id);
                String name = UserNameManager.getUserName(f_id);
                if (recipient_list.Contains(f_id))
                    recipient_list.Remove(f_id);
                //user_session.setVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID, friend_id);
                else
                    return new InputHandlerResult(
                    name + " is has already been removed from the list of recipients.");

                return new InputHandlerResult(
                    name + " has been removed from the list of recipients.");
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
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        public const String ORIGINAL_ACTION = "ORIGINAL_ACTION";
        /*public const String BLOCK_FRIEND = "BLOCK_";
        public const String DELETE_FRIEND = "DELETE_";*/
        public const String CONF_YES = "YES";
        public const String CONF_NO = "NO";
        public const String CONF_Y = "Y";
        public const String CONF_N = "N";
        public const String FILTER_LIST = "FILTER_";

        public const String RECIPIENT_SEND = "SEND_";


        public const String RECIPIENT_ADD = "ADD_";
        public const String RECIPIENT_REMOVE = "REMOVE_";
        public const String RECIPIENT_LIST = "RECIPIENT_LIST";


        /*public const String BLOCKED_FRIEND_NAME = "Friend.friend_blocked";
        public const String DELETED_FRIEND_NAME = "Friend.friend_deleted";
        */
        public const String FRIEND_LIST_FILTER =  "Friend.filter";
        
    }


}
