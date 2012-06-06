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
    class Favourite_Verse_Menu_Handler : AInputHandler
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
            MenuPage mp = mm.menu_def.getMenuPage(curr_user_page);
            //for now we assume this. must correct this later
            DynMenuPage dmp = (DynMenuPage)mm.menu_def.getMenuPage(curr_user_page);
            List<MenuOptionItem> options = mp.getOptionList(user_session);
            
            output = handleFavouriteDeleteLink(user_session, input, options);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

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

            /*else if (mp.GetType().Name == "MxitTestApp.OptionMenuPage")
            {
                OptionMenuPage omp = (OptionMenuPage)mm.menu_def.getMenuPage(curr_user_page);
                List<MenuOptionItem> options = omp.options;
                foreach (MenuOptionItem option in options)
                {
                    if (option.link_val.Equals(input))
                    {
                        user_session.setVariable("SELECTED_BOOK_ID", input);
                        return new InputHandlerResult(
                            InputHandlerResult.NEW_MENU_ACTION,
                            option.select_action,
                            InputHandlerResult.DEFAULT_PAGE_ID);
                    }
                }
            }*/

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected InputHandlerResult handleFavouriteDeleteLink(
            UserSession user_session,
            string input,
            List<MenuOptionItem> menu_options)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            if (entry.StartsWith(DEL_PREFIX))
            {
                String index = entry.Replace(DEL_PREFIX, "");
                int delete_index = -1;
                if (!Int32.TryParse(index, out delete_index))
                {
                    return new InputHandlerResult(
                        "The entry could not be deleted. You entered an invalid ID to delete."); //invalid choice
                }
                delete_index = delete_index - 1;
                if (delete_index < 0 || delete_index >= menu_options.Count())
                {
                    return new InputHandlerResult(
                        "The index you requested to be deleted is out of range. "); //invalid choice
                }

                VerseMenuOptionItem fvmo = (VerseMenuOptionItem) menu_options[delete_index];
                if (fvmo == null)
                {
                    return new InputHandlerResult(
                       "Your entry could not be deleted."); //invalid choice
                }
                user_session.deleteFavouriteSelection(((FavouriteVerseRecord)fvmo.fvr).id);
                return new InputHandlerResult(
                        "Your favourite verse entry has been deleted"); //invalid choice
            }
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        public const String DEL_PREFIX = "DEL:";
    }


}
