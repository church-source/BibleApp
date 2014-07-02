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
    class TaggedVersesHandler : AInputHandler
    {
        public override void init(UserSession us)
        {
            Object curr_page = us.getVariableObject(TAGGED_VERSE_CURRENT_PAGE);
            if (curr_page != null)
            {
                int curr_page_i = (int)curr_page;
                us.removeVariable(TAGGED_VERSE_CURRENT_PAGE);
                us.current_menu_page = curr_page_i; 
            }
        }

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

            output = handleRefreshLink(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleVerseTagLikeLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;
            

            List<MenuOptionItem> options = mp.getOptionList(user_session);
            string output_var = dmp.output_var;
            //this is a waste, if we do change input then its found so we can already return.
            input = dmp.dynamic_set.parseInput(input, user_session);
            foreach (MenuOptionItem option in options)
            {
                if (option.is_valid && (option.link_val.Equals(input) || option.menu_option_id.Equals(input)))
                {
                    
                    user_session.setVariable(output_var, input);
                    user_session.setVariable(TAGGED_VERSE_CURRENT_PAGE, (user_session.current_menu_page));
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


        protected InputHandlerResult handleVerseTagLikeLinks(
          UserSession user_session,
          string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            long verse_tag_id = -1;
            if (entry.StartsWith(TaggedVersesScreenOutputAdapter.LIKE_TAG))
            {

                verse_tag_id = long.Parse(entry.Split('_')[1]);
                long emotion_id = Int32.Parse(
                        user_session.getVariable(
                            TaggedVersesOptionSet.SELECTED_EMOTION_VAR_NAME));

                List<VerseTag> tagged_verses = VerseTagManager.getInstance().getListOfVerseTagsForEmotion(
                    Int32.Parse(
                        user_session.getVariable(
                            TaggedVersesOptionSet.SELECTED_EMOTION_VAR_NAME)));
                VerseTag selected_tag = null;
                foreach (VerseTag vt in tagged_verses)
                {
                    if (vt.id == verse_tag_id)
                    {
                        selected_tag = vt;
                    }
                }
                if (selected_tag == null)
                {
                    return new InputHandlerResult(InputHandlerResult.UNDEFINED_MENU_ACTION);
                }
                selected_tag.addNewLike(new VerseTagEmotionLike(-1,verse_tag_id,user_session.user_profile.id, DateTime.Now));

                return new InputHandlerResult(
                    InputHandlerResult.DO_NOTHING_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID); //the menu id is retreived from the session in this case.  
            }
            return new InputHandlerResult(
                InputHandlerResult.UNDEFINED_MENU_ACTION,
                InputHandlerResult.DEFAULT_MENU_ID,
                InputHandlerResult.DEFAULT_PAGE_ID);

        }

        public const String TAGGED_VERSE_CURRENT_PAGE = "TAGGED_VERSE_CURRENT_PAGE";
    }
}
