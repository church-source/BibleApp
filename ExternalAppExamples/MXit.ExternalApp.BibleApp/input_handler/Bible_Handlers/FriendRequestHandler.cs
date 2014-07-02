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
    class FriendRequestHandler : AInputHandler
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

            output = handleFriendRequestLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected InputHandlerResult handleFriendRequestLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            long friend_id = -1;
            if (entry.StartsWith(APPROVE_REQUEST))
            {
                friend_id = long.Parse(entry.Split('_')[1]);
                user_session.friend_manager.approveFriendRequest(friend_id);
                String user_name = UserNameManager.getInstance().getUserName(friend_id);

                user_session.setVariable(APPROVED_FRIEND_NAME, user_name);

                return new InputHandlerResult(
                    InputHandlerResult.BACK_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID); //the menu id is retreived from the session in this case.  
            }
            else if (entry.StartsWith(REJECT_REQUEST))
            {
                friend_id = long.Parse(entry.Split('_')[1]);
                user_session.friend_manager.rejectFriendRequest(friend_id);
                String user_name = UserNameManager.getInstance().getUserName(friend_id);

                user_session.setVariable(REJECTED_FRIEND_NAME, user_name);
                return new InputHandlerResult(
                    InputHandlerResult.BACK_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID); //the menu id is retreived from the session in this case. 
            }
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        public const String APPROVE_REQUEST = "APPROVE_";
        public const String REJECT_REQUEST = "REJECT_";

        public const String APPROVED_FRIEND_NAME = "FriendRequest.friend_approved";
        public const String REJECTED_FRIEND_NAME = "FriendRequest.friend_rejected";
       
    }


}
