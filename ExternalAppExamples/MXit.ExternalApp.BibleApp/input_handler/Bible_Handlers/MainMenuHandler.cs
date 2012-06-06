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
    class MainMenuHandler : AInputHandler
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

            output = handleReferLink(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleReferCompleteLink(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleBookmarkLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleAdminLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;


            output = handleShortcutLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;


            if (user_session.user_profile.is_suspended)
            {
                return new InputHandlerResult
                (InputHandlerResult.DO_NOTHING_ACTION,
                InputHandlerResult.DEFAULT_MENU_ID,
                InputHandlerResult.DEFAULT_PAGE_ID);
            }
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

        protected InputHandlerResult handleBookmarkLinks(
            UserSession us,
            string input)
        {
            if (BOOKMARK_ENTERED.Equals(input.Trim().ToUpper()))
            {
                BookmarkVerseRecord bvr = us.bookmark_manager.bookmark_verse;
                if (bvr == null)
                {
                    //we should never get here, the bookmark should not be available if the user has not browsed before, but just in case :)
                    return new InputHandlerResult(
                                    "You do not currently have a bookmark set, please choose something else. if you have been browsing the Bible before this, then you should have one. Let us know if that is the case so we can look into it.");
                }
                Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), bvr.start_verse);
                Verse end_verse;
                if (bvr.end_verse == null || bvr.start_verse.Equals(bvr.end_verse))
                    end_verse = null;
                else if ("NULL".Equals(bvr.end_verse))
                    end_verse = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start_verse);
                else
                    end_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), bvr.end_verse);

                String verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(start_verse, end_verse);
                us.setVariable(BOOKMARK_VERSE_VAR_NAME, verse_ref);

                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.BROWSE_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);

            }
            return new InputHandlerResult(
                         InputHandlerResult.UNDEFINED_MENU_ACTION,
                         InputHandlerResult.DEFAULT_MENU_ID,
                         InputHandlerResult.DEFAULT_PAGE_ID);

        }

        protected InputHandlerResult handleAdminLinks(
            UserSession us,
            string input)
        {
            if (SEND_NOTIFICATION.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.SEND_NOTIF_MESSAGE,
                             InputHandlerResult.DEFAULT_PAGE_ID);

            }
            else
            {
                return new InputHandlerResult(
                             InputHandlerResult.UNDEFINED_MENU_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }

        }

        protected InputHandlerResult handleReferLink(
            UserSession us,
            string input)
        {
            if (REFER_A_FRIEND.Equals(input.Trim().ToUpper()))
            {
                us.setVariable(REFER_A_FRIEND, REFER_A_FRIEND);
                return new InputHandlerResult(
                             InputHandlerResult.DO_NOTHING_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else
            {
                return new InputHandlerResult(
                             InputHandlerResult.UNDEFINED_MENU_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        protected InputHandlerResult handleReferCompleteLink(
            UserSession us,
            string input)
        {
            if (REFER_A_FRIEND_COMPLETED.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.DO_NOTHING_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else
            {
                return new InputHandlerResult(
                             InputHandlerResult.UNDEFINED_MENU_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        protected InputHandlerResult handleShortcutLinks(
            UserSession us,
            string input)
        {
            if (MESSAGE_INBOX.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.MESSAGE_INBOX_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);

            }
            else if (BUDDY_REQUESTS.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.MY_FRIEND_REQUESTS_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);

            }
            else if (HELP.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.HELP_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);

            }
            else if (ABOUT.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.ABOUT_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);

            }
            else if (COLOUR_CHANGE.Equals(input.Trim().ToUpper()))
            {

                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.COLOUR_THEME_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else
            {
                return new InputHandlerResult(
                             InputHandlerResult.UNDEFINED_MENU_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }

        }

        public const String BOOKMARK_ENTERED = "BOOKMARK";
        public const String SEND_NOTIFICATION = "SEND_NOTIFICATION";
        public const String MESSAGE_INBOX = "MESSAGE_INBOX";
        public const String BUDDY_REQUESTS = "BUDDY_REQUESTS";
        public const String HELP = "HELP";
        public const String ABOUT = "ABOUT";
        public const String COLOUR_CHANGE = "COLOUR_CHANGE";
        public const String REFER_A_FRIEND = "REFER";
        public const String REFER_A_FRIEND_COMPLETED = "REFER_COMPLETE";
        public const String BOOKMARK_VERSE_VAR_NAME = "MainMenu.bookmark_verse";
    }


}
