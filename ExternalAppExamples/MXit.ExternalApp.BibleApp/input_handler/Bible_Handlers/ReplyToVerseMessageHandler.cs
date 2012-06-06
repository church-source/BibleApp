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
    class ReplyToVerseMessageHandler : AInputHandler
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

            if (input.Count() > MAX_MESSAGE_LENGTH)
            {
                return new InputHandlerResult(
                   "Your message is too long, please keep it less than " + MAX_MESSAGE_LENGTH + " characters.\r\n"); //invalid choice
            }
            else if (input.Trim().Equals(""))
            {
                return new InputHandlerResult(
                   "You entered a blank message. please try again.\r\n"); //blank input
            }
            else if (input.Trim().ToUpper().Equals(MessageThreadHandler.REPLY))
            {
                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_MENU_ID); //blank input
            }
            else
            {
                try
                {
                    long thread_id = long.Parse(user_session.getVariable(MessageInboxHandler.CURRENTLY_VIEWING_TRHEAD));
                    VerseMessageThread vmt = VerseThreadManager.getInstance().getVerseMessageThread(thread_id);
                    if (vmt != null)
                    {
                        //check the current state of the friendship before along the user to send the message. 
                        if (user_session.friend_manager.getFriendStatus(vmt.user_created_id) == FriendRelation.FRIEND_BLOCKED_BY_YOU)
                        {
                            return new InputHandlerResult(
                                InputHandlerResult.DISPLAY_MESSAGE,
                                InputHandlerResult.DEFAULT_MENU_ID,
                                "You blocked the original sender of this message. As a result you cant send them a message until you re-add you as a friend. You have to use their buddy code and send a friend request to them again. Because you blocked them they can't add you.");
                        }
                        else if (user_session.friend_manager.getFriendStatus(vmt.user_created_id) == FriendRelation.FRIEND_BLOCKED_YOU)
                        {
                            return new InputHandlerResult(
                                   InputHandlerResult.DISPLAY_MESSAGE,
                                   InputHandlerResult.DEFAULT_MENU_ID,
                                   "The original sender of this message blocked you as a buddy. This means that you cant send them any messages. The only way to be unblocked is for them to resend a friend request to you.");
                        }
                        user_session.verse_messaging_manager.addMessageToThread(vmt, input);
                    }
                    //if vmt is null there is something wrong. need to handle this because we might delete the thread while someone is viewing it. 
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
