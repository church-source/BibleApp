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
    class HelpMenuHandler : AInputHandler
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

            MenuManager mm = MenuManager.getInstance();
            //for now we assume this. must correct this later
            OptionMenuPage omp = (OptionMenuPage)mm.menu_def.getMenuPage(curr_user_page);
            List<MenuOptionItem> options = omp.options;
            foreach (MenuOptionItem option in options)
            {
                if (option.link_val.Equals(input))
                    return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    option.select_action,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected override InputHandlerResult handleStdNavLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();

            if (PREV_MENU.Equals(entry) || PREVIOUS_MENU.Equals(entry))
            {
                //check if 
                MenuManager mm = MenuManager.getInstance();
                MenuPage mp;
                mp = mm.menu_def.getMenuPage(user_session.current_menu_loc);
                //only allow back input if back link is enabled. 
                if (mp.isBackLinkEnabled())
                {
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
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

       
    }


}
