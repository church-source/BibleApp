using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sphinx.Client.Commands.Search;
using Sphinx.Client.Connections;

using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;


namespace MxitTestApp
{
    class SearchHandler : AInputHandler
    {

        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            user_session.search_results = null;
            string input = extractReply(message_recieved);
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
                   "Your search query is too long " + MAX_MESSAGE_LENGTH + " characters.\r\n"); //invalid choice
            }
            else if (input== null || "".Equals(input.Trim()) )
            {
                return new InputHandlerResult(
                   "You search query was blank. You need to send the words that you would like to search for. \r\n"); //invalid choice
            }
            else
            {
                try
                {
                    searchBible(user_session, input);
                    return new InputHandlerResult(
                     InputHandlerResult.NEW_MENU_ACTION,
                     vmp.input_item.target_page,
                     InputHandlerResult.DEFAULT_PAGE_ID);
                }
                catch (Exception e)
                {
                    try
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        searchBible(user_session, input);
                        return new InputHandlerResult(
                         InputHandlerResult.NEW_MENU_ACTION,
                         vmp.input_item.target_page,
                         InputHandlerResult.DEFAULT_PAGE_ID);
                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine(e2.Message);
                        Console.WriteLine(e2.StackTrace);
                        return new InputHandlerResult(
                       "Something went wrong when searching the Bible, please try again later. ");
                    }
                }
            }

        }

        public void searchBible(UserSession us, String input) 
        {
            String search_testament = us.getVariable(SearchTestamentHandler.SEARCH_TESTAMENT_VAR_NAME);
            int test_search = -1;
            if (search_testament != null)
            {
                test_search = Int32.Parse(search_testament);
            }
            String search_book = us.getVariable(BOOK_SEARCH_VAR_NAME);
            int book_search = -1;
            if (search_book != null)
            {
                book_search = BibleContainer.getBookId(us.user_profile.getDefaultTranslationId(),search_book).id;
            }
            IList<SearchQueryResult> results = BibleSearch.getInstance().searchBible(input, Int32.Parse(us.user_profile.getDefaultTranslationId()), book_search, test_search);
            List<SearchVerseRecord> search_result_list = new List<SearchVerseRecord>();
            String book = "";
            int chapter = -1;
            int verse = -1;
            int rank = -1;
            int testament= -1;
            String verse_ref = "";
            String book_id = "";
            foreach (SearchQueryResult result in results)
            {
                foreach (Match match in result.Matches)
                {
                    book_id = match.AttributesValues["book"].GetValue().ToString();
                    book = BibleHelper.getNameFromID(book_id);
                    chapter =  (int)match.AttributesValues["chapter"].GetValue();
                    verse = (int) match.AttributesValues["verse"].GetValue();
                    rank =  match.Weight;
                    testament = BibleHelper.getTestamentIDFromBookID(book_id);
                    Verse start_verse = BibleContainer.getInstance().getVerse(
                                    Int32.Parse(us.user_profile.getDefaultTranslationId()),
                                    testament,
                                    book,
                                    chapter,
                                    verse);
                    verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(start_verse, start_verse);
                    search_result_list.Add(new SearchVerseRecord(verse_ref, verse_ref, rank));
                }
            }
            us.search_results = search_result_list;
        }

        public const int MAX_MESSAGE_LENGTH = 100;


        public const String SEARCH_VERSE_VAR_NAME = "SearchResults.verse_from_search_results";

        public const String BOOK_SEARCH_VAR_NAME = "Search_Handler.book_name";

    }
}
