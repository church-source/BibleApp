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
    class ColourThemeHandler : AInputHandler
    {
        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);
            //get reply
            string curr_user_page = user_session.current_menu_loc;


            MenuManager mm = MenuManager.getInstance();
            MenuPage mp = mm.menu_def.getMenuPage(curr_user_page);
            //for now we assume this. must correct this later
            DynMenuPage dmp = (DynMenuPage)mm.menu_def.getMenuPage(curr_user_page);

            //handle extra commands
            InputHandlerResult output = dmp.dynamic_set.handleExtraCommandInput(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;



            output = handleStdNavLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;


            output = handleShortcutLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;
    

/*            List<MenuOptionItem> options = mp.getOptionList(user_session);
            string output_var = dmp.output_var;
            //this is a waste, if we do change input then its found so we can already return.
            input = dmp.dynamic_set.parseInput(input, user_session);
            foreach (MenuOptionItem option in options)
            {
                if (option.is_valid && option.link_val.Equals(input))
                {
                    user_session.setVariable(output_var, input);
                    return new InputHandlerResult(
                        InputHandlerResult.NEW_MENU_ACTION,
                        option.select_action,
                        InputHandlerResult.DEFAULT_PAGE_ID);
                }
            }

            */


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        protected InputHandlerResult handleShortcutLinks(
           UserSession us,
           string input)
        {

            if (input == RESET)
            {
                us.user_profile.user_profile_custom.setColourTheme(UserColourTheme.NO_THEME);
                us.setVariable(AScreenOutputAdapter.COLOUR_CHANGED, "COLOUR_CHANGED");
                return new InputHandlerResult(
                 InputHandlerResult.DO_NOTHING_ACTION,
                 InputHandlerResult.DEFAULT_MENU_ID,
                 InputHandlerResult.DEFAULT_PAGE_ID);
            }
            int colour_theme = -1;
            if (!Int32.TryParse(input, out colour_theme))
            {
                return new InputHandlerResult(
                   InputHandlerResult.INVALID_MENU_ACTION,
                   "Invalid Input...");
            }
            //colour_theme -= 1;
            if (!UserColourTheme.isColourThemeValid(colour_theme))
            {
                    return new InputHandlerResult(
                       InputHandlerResult.INVALID_MENU_ACTION,
                       "Invalid Input...");
            }
            us.user_profile.user_profile_custom.setColourTheme(colour_theme);
            us.setVariable(AScreenOutputAdapter.COLOUR_CHANGED, "COLOUR_CHANGED");
            return new InputHandlerResult(
                             InputHandlerResult.DO_NOTHING_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            

        }

        public const String RESET = "RESET";
    }


}
