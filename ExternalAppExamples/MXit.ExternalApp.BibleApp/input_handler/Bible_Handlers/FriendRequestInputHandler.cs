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


using MySql.Data;
using MySql.Data.MySqlClient;



namespace MxitTestApp
{
    class FriendRequestInputHandler : AInputHandler
    {
        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            input = input.Trim();
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);            
            //get reply
            string curr_user_page = user_session.current_menu_loc;

            InputHandlerResult output = handleStdNavLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            /*output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;*/

            MenuManager mm = MenuManager.getInstance();
            //for now we assume this. must correct this later
            VerseMenuPage vmp = (VerseMenuPage)mm.menu_def.getMenuPage(curr_user_page);
           

            if (input.Count() != BibleUserCodeCreator.CODE_LENGTH)
            {
                return new InputHandlerResult(
                   "The code you entered is not valid. please enter a code that is 6 characters in length (it can be numbers or letters).\r\n"); //invalid choice
            }
            long friend_id = UserProfileDBManager.getIDFromUserCode(input);
            if (friend_id == -1)
            {
                return new InputHandlerResult(
                   "Sorry but there is no user with that BibleApp buddy code. \r\n"); //blank input
            }
            else
            {
                if (friend_id == user_session.user_profile.id)
                {
                    return new InputHandlerResult(
                       "Sorry but you can't add yourself as a friend. You have to ask your friend for their code and then add them using their code. \r\n"); //blank input
                }
                try
                {
                    long result = user_session.friend_manager.addFriendRequest(friend_id);
                    String user_name = UserNameManager.getInstance().getUserName(friend_id);
                    if (result == FriendManager.FRIEND_REQUEST_ALREADY_REQUESTED)
                    {
                        return new InputHandlerResult(
                            "You already have sent a buddy request to "+user_name+". \r\n"); //invalid choice
                    }
                    else if (result == FriendManager.FRIEND_REQUEST_ALREADY_FRIENDS)
                    {
                        return new InputHandlerResult(
                            "You already buddies with " + user_name + ". \r\n"); //invalid choice
                    }
                    else if (result == FriendManager.FRIEND_REQUEST_BLOCKED)
                    {
                        return new InputHandlerResult(
                            user_name +" has blocked you. You cannot send a buddy request.  \r\n"); //invalid choice
                    }
                    else if (result == FriendManager.FRIEND_REQUEST_BLOCKED_APPROVED)
                    {
                        return new InputHandlerResult(
                            "You have successfully unblocked " + user_name + ". " + user_name +" will again appear in your buddy list.\r\n"); //invalid choice
                    }
                    
                    
                    user_session.setVariable(REQUESTED_FRIEND_NAME, user_name);

                    return new InputHandlerResult(
                        InputHandlerResult.BACK_MENU_ACTION,
                        InputHandlerResult.DEFAULT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID); //the menu id is retreived from the session in this case. 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return new InputHandlerResult(
                   "Something went wrong with the request. please contact us to inform us of the problem."); //invalid choice
                }
            }

        }
        public const String REQUESTED_FRIEND_NAME = "FriendRequest.friend_requested";
    }


}
