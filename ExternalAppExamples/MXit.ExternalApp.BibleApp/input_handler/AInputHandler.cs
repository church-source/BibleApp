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
    abstract class AInputHandler : IInputHandler
    {
        public abstract InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved);

        public virtual void init(UserSession us)
        {
            //do nothing
        }

        protected string extractReply(MessageReceived messageReceived)
        {
            if (messageReceived.Type == MessageType.Normal || messageReceived.Type == MessageType.Chat)
            {
                return messageReceived.Body;
            }
            else if (messageReceived.Type == MessageType.MXitCommand)
            {
                Dictionary<string, IReply> request = messageReceived.ExtractReply();
                return request[OptionScreenOutputAdapter.MENU_LINK_NAME].Value;
            }
            else if (messageReceived.Type == MessageType.ServiceRedirect
                        || messageReceived.Type == MessageType.Service2Service)
            {
                return MainMenuHandler.REFER_A_FRIEND_COMPLETED;
            }
            else
            {
                throw new Exception("Unsupported Message Received");
            }
        }

        protected bool IsMyLink(MessageReceived messageReceived)
        {
            return (messageReceived.Type == MessageType.Normal) &&
            (messageReceived.Body == "Reply to return to the application");
        }


        /*this method either returns the new screen id or the main or prev command string*/
        protected virtual InputHandlerResult handleStdNavLinks(
            UserSession user_session,
            string input)
        {
            return handleStdNavLinks(user_session, input, false);
        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected virtual InputHandlerResult handleStdNavLinks(
            UserSession user_session,
            string input,
            Boolean back_without_init)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();

            if (MenuDefinition.UNDEFINED_MENU_ID.Equals(curr_user_page))
                return new InputHandlerResult(
                    InputHandlerResult.ROOT_MENU_ACTION,
                    MenuDefinition.ROOT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);//return root menu if this is first request; 
            else if (PREV_MENU.Equals(entry) || PREVIOUS_MENU.Equals(entry))
            {
                //check if 
                MenuManager mm = MenuManager.getInstance();
                MenuPage mp;
                mp = mm.menu_def.getMenuPage(user_session.current_menu_loc);
                //only allow back input if back link is enabled. 
                if (mp.isBackLinkEnabled())
                {
                    if (!back_without_init)
                        return new InputHandlerResult(
                            InputHandlerResult.BACK_MENU_ACTION,
                            InputHandlerResult.DEFAULT_MENU_ID,
                            InputHandlerResult.DEFAULT_PAGE_ID); //the menu id is retreived from the session in this case. 

                    return new InputHandlerResult(
                         InputHandlerResult.BACK_WITHOUT_INIT_MENU_ACTION,
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
            else if (MAIN_MENU.Equals(entry))
            {
                MenuManager mm = MenuManager.getInstance();
                MenuPage mp;
                mp = mm.menu_def.getMenuPage(user_session.current_menu_loc);
                if (mp.isMainLinkEnabled())
                {
                    return new InputHandlerResult(
                        InputHandlerResult.ROOT_MENU_ACTION,
                        MenuDefinition.ROOT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID);//return root menu
                }
                else
                {
                    return new InputHandlerResult(
                        InputHandlerResult.UNDEFINED_MENU_ACTION,
                        InputHandlerResult.DEFAULT_MENU_ID,
                        InputHandlerResult.DEFAULT_PAGE_ID);
                }
            }
            else if (HELP_MENU.Equals(entry))
            {
                MenuManager mm = MenuManager.getInstance();
                MenuPage mp;
                mp = mm.menu_def.getMenuPage(user_session.current_menu_loc);
                if (mp.hasHelpPage())
                {
                    return new InputHandlerResult(
                        InputHandlerResult.NEW_MENU_ACTION,
                        mp.help_page_id);
                }
                else
                {
                    return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
                }
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
        protected InputHandlerResult handleStdPageLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            if (PREV_PAGE.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.PREV_PAGE_ACTION,
                    user_session.current_menu_loc,
                    user_session.current_menu_page - 1); //the menu id is retreived from the session in this case. 
            }
            else if (NEXT_PAGE.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEXT_PAGE_ACTION,
                    user_session.current_menu_loc,
                    user_session.current_menu_page + 1);
            }
            else if(FIRST_PAGE.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.CHANGE_PAGE_ACTION,
                    user_session.current_menu_loc,
                   0);
            }
            else if (entry.StartsWith(LAST_PAGE))
            {
                int page_id = Int32.Parse(entry.Split('_')[1]);
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


        protected InputHandlerResult handleDisplayMessageLinks(
            UserSession user_session,
            string input,
            String error_message)
        {
            return handleDisplayMessageLinks(user_session, input, error_message, false);
        }

        protected InputHandlerResult handleDisplayMessageLinks(
            UserSession user_session,
            string input,
            String error_message,
            Boolean back_without_init)
        {
            bool message_page = user_session.getVariable(DISPLAY_MESSAGE) != null;
            if (message_page == true)
            {
                user_session.removeVariable(DISPLAY_MESSAGE);
            }

            InputHandlerResult output = handleStdNavLinks(user_session, input, back_without_init);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //if this was a messsage then the only options is the std Nav links. any other input is invalid so reshow message
            if (message_page)
            {
                user_session.setVariable(DISPLAY_MESSAGE, "Message sent");//you must be sure to remove this from hash table in handler. 
                return new InputHandlerResult(
                    InputHandlerResult.DISPLAY_MESSAGE,
                    InputHandlerResult.DEFAULT_MENU_ID, //not used
                    error_message);
            }
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID
                    );
            }
        }

        //constants are all over the place. this is terrible. 
        public const string PREVIOUS_MENU = "PREVIOUS";
        public const string PREV_MENU = "BACK";
        public const string MAIN_MENU = "MAIN";
        public const string HELP_MENU = "HELP";

        public const string PREV_PAGE   = "PREV";
        public const string NEXT_PAGE   = "NEXT";
        public const string FIRST_PAGE  = "FIRSTPAGE";
        public const string LAST_PAGE   = "LASTPAGE";
        public const String DISPLAY_MESSAGE = "DISPLAY_MESSAGE";
        /*
        public const string PREV_MENU_ACTION = "BACK";
        public const string MAIN_MENU_ACTION = "MAIN";
        public const string UNEDEFINED_MENU_ACTION = "UNDEFINED";
        public const string INVALID_MENU_ACTION = "INVALID";*/


    }
}
