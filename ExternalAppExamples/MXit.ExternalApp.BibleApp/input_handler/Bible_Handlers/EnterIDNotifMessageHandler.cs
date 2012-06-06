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
    class EnterIDNotifMessageHandler : AInputHandler
    {
        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);            
            //get reply
            string curr_user_page = user_session.current_menu_loc;
            InputHandlerResult output = handleDisplayMessageLinks(
                user_session, 
                input,
                "Your input was invalid. You message has been sent already but please click Back/Main to continue");
                
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;



            output = handleStdNavLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;



            /*output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;*/

            MenuManager mm = MenuManager.getInstance();
            //for now we assume this. must correct this later
            VerseMenuPage vmp = (VerseMenuPage)mm.menu_def.getMenuPage(curr_user_page);
            List<MenuOptionItem> options = vmp.options;
            foreach (MenuOptionItem option in options)
            {
                if (option.link_val.Equals(input))
                    return new InputHandlerResult(
                         InputHandlerResult.NEW_MENU_ACTION,
                         option.select_action,
                         InputHandlerResult.DEFAULT_PAGE_ID);
            }
            long id = -1;
            if (!long.TryParse(input,out id))
            {
                return new InputHandlerResult(
                   "You have to enter the numeric profile ID of the recipient."); //invalid choice
            }
            else if (input.Trim().Equals(""))
            {
                return new InputHandlerResult(
                   "You entered a blank message. please try again.\r\n"); //blank input
            }
            else
            {
                try
                {

                    user_session.setVariable(NotifMessageSendOutputAdapter.RECIPIENT_ID, id.ToString());

                    return new InputHandlerResult(
                        InputHandlerResult.BACK_WITHOUT_INIT_MENU_ACTION,
                        InputHandlerResult.DEFAULT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return new InputHandlerResult(
                   "Something went wrong when trying to send your message, please try again later. "); //invalid choice
                }
            }

        }

        public const int MAX_MESSAGE_LENGTH = 500;
        
       
    }


}
