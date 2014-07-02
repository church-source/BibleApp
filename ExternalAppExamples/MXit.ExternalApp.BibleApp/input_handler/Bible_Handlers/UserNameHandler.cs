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
    class UserNameHandler : AInputHandler
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
           

            if (input.Count() > UserNameManager.MAX_USER_NAME_LENGTH)
            {
                return new InputHandlerResult(
                   "Your user name is too long, please keep it less than " + UserNameManager.MAX_USER_NAME_LENGTH + " characters.\r\n"); //invalid choice
            }
            else if (input.Equals("") || input.Equals(MyProfileHandler.USER_NAME_CHANGE))
            {
                return new InputHandlerResult(
                   "You entered a blank user name, this is not allowed. please try again.\r\n"); //blank input
            }
            else if (!UserNameManager.getInstance().isUserNameUnique(input))
            {
                return new InputHandlerResult(
                   "Sorry but that user name is already taken. Please try again.\r\n"); //blank input
            }
            else
            {
                try
                {
                    user_session.user_profile.setUserName(input);
                    return new InputHandlerResult(
                     InputHandlerResult.NEW_MENU_ACTION,
                     vmp.input_item.target_page,
                     InputHandlerResult.DEFAULT_PAGE_ID);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return new InputHandlerResult(
                   "Your user name is invalid or has been taken already, please try again."); //invalid choice
                }
            }

        }
    }


}
