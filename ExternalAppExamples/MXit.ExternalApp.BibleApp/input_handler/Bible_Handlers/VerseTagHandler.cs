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
    class VerseTagHandler : AInputHandler
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



            output = handleStdNavLinks(user_session, input,true);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;




            List<MenuOptionItem> options = mp.getOptionList(user_session);
            string output_var = dmp.output_var;
            //this is a waste, if we do change input then its found so we can already return.
            
            foreach (MenuOptionItem option in options)
            {
                if (option.is_valid && option.link_val.Equals(input))
                {
                    input = dmp.dynamic_set.parseInput(input, user_session);        
                    user_session.setVariable(output_var, input);
                    VerseSection vs = (VerseSection)user_session.getVariableObject("Browse.verse_section");
                    String start_verse;
                    String end_verse;
                    if (vs == null)
                    {
                        Console.WriteLine("Expected Browse.verse_section present, but not found in VerseMessageSendHandler.");
                        return new InputHandlerResult("There is a problem in sending the message. Please let us know about this problem by using the feedback option");
                    }
                    else
                    {
                        Verse start = vs.start_verse;
                        Verse end = vs.end_verse;
                        if (end == null)
                        {
                            end = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start);
                        }
                        start_verse = start.getVerseReference();
                        end_verse = end.getVerseReference();
                    }
                    try
                    {
                        int emotion_id = Int32.Parse(input);
                        VerseTagManager.getInstance().addVerseTag(
                            user_session.user_profile.id,
                            start_verse,
                            end_verse,
                            emotion_id);
                    }
                    catch (VerseEmotionTagAlreadyPresentException e)
                    {
                        return new InputHandlerResult(
                    InputHandlerResult.DISPLAY_MESSAGE,
                    InputHandlerResult.DEFAULT_MENU_ID, //not used
                    "That verse has already been tagged with that emotion. You cant tag the same verse with the same emotion more than once.");
                    }
                    catch (Exception e1)
                    {
                        return new InputHandlerResult(
                    InputHandlerResult.DISPLAY_MESSAGE,
                    InputHandlerResult.DEFAULT_MENU_ID, //not used
                    "Something went wrong. please let us know what happened using the feedback option.");
                    }
                    return new InputHandlerResult(
                     InputHandlerResult.DISPLAY_MESSAGE,
                     InputHandlerResult.DEFAULT_MENU_ID, //not used
                     "Thanks you have successfully tagged the verse!! :) ");
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


    }


}
