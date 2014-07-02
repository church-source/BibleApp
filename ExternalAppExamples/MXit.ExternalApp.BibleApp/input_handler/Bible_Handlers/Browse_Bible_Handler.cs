using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;


namespace MxitTestApp
{
    class Browse_Bible_Handler : AInputHandler
    {

        public override void init(UserSession us)
        {
            //Console.WriteLine("Init Browse Interaction");
            //first we need a way to know if the screen should be cleared. 
            us.setVariable(BROWSE_CLEAR_SCREEN, true);
            //now this is one big hack
            Boolean direct_select;
            try
            {
                Object o = us.removeVariable("Browse.directSelect");
                if(o==null)
                    direct_select = false;
                else
                    direct_select = (Boolean)o;
            }catch(Exception e)
            {
                direct_select = false;
            }

            int verse_history_index = getVerseHistoryIndex(us);
           

            //Verse was selected from history of verses
            if (verse_history_index > -1)
            {
                ReadOnlyCollection<VerseHistoryRecord> history_list = us.verse_history.getHistoryListForDisplay();
                VerseHistoryRecord vhr = history_list[verse_history_index];
                Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), vhr.start_verse);
                Verse end_verse;
                if (vhr.end_verse == null || vhr.start_verse.Equals(vhr.end_verse))
                    end_verse = start_verse;
                else if ("NULL".Equals(vhr.end_verse))
                    end_verse = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start_verse);
                else
                    end_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), vhr.end_verse);

                VerseSection vs = new VerseSection(start_verse, end_verse);
                us.setVariable("Browse.verse_section", vs);
            }
            else
            {
                String top_fav_verse = getTopFavouriteSelectedVerse(us);
                if (top_fav_verse != null)
                {
                    VerseSection vs = Verse_Handler.getVerseSection(us, top_fav_verse, null, null);
                    if (vs != null)
                    {
                        us.setVariable("Browse.verse_section", vs);
                        us.recordVerseSelection(vs.start_verse, vs.end_verse);
                    }
                }
                else
                {
                    int fav_verse_index = getFavouriteVerseIndex(us);

                    //Verse was selected from history of verses
                    if (fav_verse_index > -1)
                    {
                        ReadOnlyCollection<FavouriteVerseRecord> favourite_list = us.favourite_verses.getFavouriteListForDisplay();
                        FavouriteVerseRecord fvr = favourite_list[fav_verse_index];
                        Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), fvr.start_verse);
                        Verse end_verse;
                        if (fvr.end_verse == null || fvr.start_verse.Equals(fvr.end_verse))
                            end_verse = start_verse;
                        else if ("NULL".Equals(fvr.end_verse))
                            end_verse = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start_verse);
                        else
                            end_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), fvr.end_verse);

                        VerseSection vs = new VerseSection(start_verse, end_verse);
                        us.setVariable("Browse.verse_section", vs);
                        us.recordVerseSelection(vs.start_verse, vs.end_verse);
                    }
                    else{
                        String bookmark_verse = getBookmarkVerse(us);
                        if (bookmark_verse != null)
                        {
                            VerseSection vs = Verse_Handler.getVerseSection(us, bookmark_verse, null, null);
                            if (vs != null)
                            {
                                us.setVariable("Browse.verse_section", vs);
                                us.recordVerseSelection(vs.start_verse, vs.end_verse);
                            }
                        }
                        else
                        {
                            String daily_verse = getDailyVerseSelected(us);
                            if (daily_verse != null)
                            {
                                VerseSection vs = Verse_Handler.getVerseSection(us, daily_verse, null, null);
                                if (vs != null)
                                {
                                    us.setVariable("Browse.verse_section", vs);
                                    us.recordVerseSelection(vs.start_verse, vs.end_verse);
                                }
                            }
                            else
                            {
                                String topic_verse = getTopicVerse(us);
                                if (topic_verse != null)
                                {
                                    VerseSection vs = Verse_Handler.getVerseSection(us, topic_verse, null, null);
                                    if (vs != null)
                                    {
                                        us.setVariable("Browse.verse_section", vs);
                                        us.recordVerseSelection(vs.start_verse, vs.end_verse);
                                    }
                                }
                                else
                                {
                                    String search_verse = getSearchVerse(us);
                                    if (search_verse != null)
                                    {
                                        int search_verse_index = Int32.Parse(search_verse) - 1;
                                        SearchVerseRecord svr = us.search_results[search_verse_index];
                                        Verse start_verse = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), svr.start_verse);
                                        VerseSection vs = new VerseSection(start_verse, start_verse);
                                        if (vs != null)
                                        {
                                            us.setVariable("Browse.verse_section", vs);
                                            us.recordVerseSelection(start_verse, start_verse);
                                        }
                                    }
                                    //verse was selected from direct select
                                    else if (direct_select == false)
                                    {

                                        Verse start_verse = BibleContainer.getInstance().getVerse(
                                                Int32.Parse(us.user_profile.getDefaultTranslationId()),
                                                Int32.Parse(us.getVariable("Testament_Handler.testament_id")),
                                                us.getVariable("BookOptionSet.book_id"),
                                                Int32.Parse(us.getVariable("ChapterOptionSet.chapter_id")),
                                                1);
                                        VerseSection vs = new VerseSection(start_verse, null);
                                        us.setVariable("Browse.verse_section", vs);
                                        us.recordVerseSelection(start_verse, null);
                                    }
                                    //Verse was selected by browsing
                                    else
                                    {
                                        VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
                                        if (vs == null)
                                        {
                                            Console.WriteLine("Warning...verse_section context var was not found");
                                            throw new Exception("Expected Browse.verse_section var present but not found");
                                        }
                                        Verse start_verse = vs.start_verse;
                                        Verse end_verse = vs.end_verse;
                                        us.recordVerseSelection(start_verse, end_verse);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /*returns -1 if there history option wasnt chosen*/
        public int getVerseHistoryIndex(UserSession us)
        {

            // we assume that if the session variable MxitTestApp.VerseHistoryOptionSet exists then history
            // verse was selected. 
            try
            {
                Object o = us.removeVariable("VerseHistoryOptionSet.verse_from_history");
                if (o == null)
                    return -1;
                int verse_history_index = Int32.Parse((String)o);
                return verse_history_index - 1;
            }
            catch (Exception e)
            {
                return -1;

            }
        }

        /*returns -1 if there favourite option wasnt chosen*/
        public int getFavouriteVerseIndex(UserSession us)
        {

            try
            {
                Object o = us.removeVariable("FavouriteVersesOptionSet.verse_from_favourites");
                if (o == null)
                    return -1;
                int fav_verse_index = Int32.Parse((String)o);
                return fav_verse_index - 1;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        /*returns null if there favourite option wasnt chosen*/
        public String getTopFavouriteSelectedVerse(UserSession us)
        {

            try
            {
                Object o = us.removeVariable("TopFavouriteVersesOptionSet.verse_from_top_favourites");
                if (o == null)
                    return null;
                String top_fav_verse = (String)o;
                return top_fav_verse;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /*returns null if the bookmark option wasnt chosen*/
        public String getBookmarkVerse(UserSession us)
        {

            try
            {
                Object o = us.removeVariable(MainMenuHandler.BOOKMARK_VERSE_VAR_NAME);
                if (o == null)
                    return null;
                String bookmark_verse = (String)o;
                return bookmark_verse;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /*returns null if the bookmark option wasnt chosen*/
        public String getTopicVerse(UserSession us)
        {

            try
            {
                Object o = us.removeVariable("Bible_Topic.topic_id");
                if (o == null)
                    return null;
                String verse_ref = (String)o;
                return verse_ref;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /*returns null if the bookmark option wasnt chosen*/
        public String getSearchVerse(UserSession us)
        {

            try
            {
                Object o = us.removeVariable(SearchHandler.SEARCH_VERSE_VAR_NAME);
                if (o == null)
                    return null;
                String search_verse = (String)o;
                return search_verse;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /*returns null if the bookmark option wasnt chosen*/
        public String getDailyVerseSelected(UserSession us)
        {

            try
            {
                Object o = us.removeVariable("DailyVerseOptionSet.daily_verse_selected");
                if (o == null)
                    return null;
                String search_verse = (String)o;
                return search_verse;
            }
            catch (Exception e)
            {
                return null;
            }
        }

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

            output = handleBrowseLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleDirectVerseInput(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleFavouriteLinks(user_session, input);
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


        protected InputHandlerResult handleBrowseLinks(
            UserSession us,
            string input)
        {
            VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
            if (vs == null)
            {
                Console.WriteLine("Expected Browse.verse_section present, but not found");
                return new InputHandlerResult(
                   InputHandlerResult.UNDEFINED_MENU_ACTION,
                   InputHandlerResult.DEFAULT_MENU_ID,
                   InputHandlerResult.DEFAULT_PAGE_ID);
            }
            Verse start_verse = vs.start_verse;
            Verse end_verse = vs.end_verse;
            if (DISPLAY_MORE.Equals(input.Trim().ToUpper()))
            {
                if (end_verse != null && end_verse.next_verse != null)
                {
                    start_verse = end_verse.next_verse;
                    //end_verse = getDefaultEndVerse(start_verse);
                    vs = new VerseSection(start_verse, null);
                    us.setVariable("Browse.verse_section", vs); 
                    return new InputHandlerResult(InputHandlerResult.NEW_MENU_ACTION,
                                us.current_menu_loc,
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
            else if (DISPLAY_NEXT_CHAPTER.Equals(input.Trim().ToUpper()))
            {
                if (start_verse != null && 
                    start_verse.chapter != null  && 
                    start_verse.chapter.next_chapter != null)
                {
                    start_verse = start_verse.chapter.next_chapter.getVerse(1);
                    //end_verse = getDefaultEndVerse(start_verse);
                    vs = new VerseSection(start_verse, null);
                    us.setVariable("Browse.verse_section", vs);
                    return new InputHandlerResult(InputHandlerResult.NEW_MENU_ACTION,
                                us.current_menu_loc,
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
            else if (DISPLAY_PREV_CHAPTER.Equals(input.Trim().ToUpper()))
            {
                if (start_verse != null &&
                    start_verse.chapter != null &&
                    start_verse.chapter.prev_chapter != null)
                {
                    start_verse = start_verse.chapter.prev_chapter.getVerse(1);
                    //end_verse = getDefaultEndVerse(start_verse);
                    vs = new VerseSection(start_verse, null);
                    us.setVariable("Browse.verse_section", vs);
                    return new InputHandlerResult(InputHandlerResult.NEW_MENU_ACTION,
                                us.current_menu_loc,
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
            else
            {
                return new InputHandlerResult(
                          InputHandlerResult.UNDEFINED_MENU_ACTION,
                          InputHandlerResult.DEFAULT_MENU_ID,
                          InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        protected InputHandlerResult handleFavouriteLinks(
            UserSession us,
            string input)
        {
            if (ADD_TO_FAV.Equals(input.Trim().ToUpper()))
            {
                VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
                if (vs == null)
                {
                    return new InputHandlerResult(
                             InputHandlerResult.UNDEFINED_MENU_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
                }
                Verse start_verse = vs.start_verse;
                if (start_verse == null)
                {
                    return new InputHandlerResult(
                             InputHandlerResult.UNDEFINED_MENU_ACTION,
                             InputHandlerResult.DEFAULT_MENU_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
                }
                Verse end_verse = vs.end_verse;
                if (us.favourite_verses.isFavouriteListFull())
                {
                    return new InputHandlerResult(
                                    "Your favourite list is full. Please first delete an existing favourite verse"); //invalid choice
                }
                int output = us.recordFavouriteSelection(start_verse, end_verse);
                if(output == FavouriteVerseManager.FAVOURITE_VERSE_ADDED_SUCCCESS)
                {
                    //end verse should never be null
                    String verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(start_verse, end_verse);
                    return new InputHandlerResult(
                                   InputHandlerResult.FAVOURITE_ADDED_ACTION,
                                   us.current_menu_loc,
                                   verse_ref + " has been added to your favourites"); //invalid choice
                }else if(output == FavouriteVerseManager.FAVOURITE_ALREADY_ADDED)
                {
                       return new InputHandlerResult(
                                   "This is already a favourite. You can't add duplicate favourites."); //invalid choice
                
                }
            }
            else if (SEND_TO_BUDDY.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.SEND_VERSE_MESSAGE_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (TAG_VERSE.Equals(input.Trim().ToUpper()))
            {
                return new InputHandlerResult(
                             InputHandlerResult.NEW_MENU_ACTION,
                             MenuIDConstants.TAG_VERSE_ID,
                             InputHandlerResult.DEFAULT_PAGE_ID);
            }
            else if (".".Equals(input.Trim().ToUpper()))
            {
                VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
                if (vs == null)
                {
                    Console.WriteLine("Expected Browse.verse_section present, but not found");
                    return new InputHandlerResult(
                       InputHandlerResult.UNDEFINED_MENU_ACTION,
                       InputHandlerResult.DEFAULT_MENU_ID,
                       InputHandlerResult.DEFAULT_PAGE_ID);
                }
                String verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(vs.start_verse, vs.end_verse);
                return handleDirectVerseInput(us, verse_ref);
            }
            return new InputHandlerResult(
                         InputHandlerResult.UNDEFINED_MENU_ACTION,
                         InputHandlerResult.DEFAULT_MENU_ID,
                         InputHandlerResult.DEFAULT_PAGE_ID);
            
        }

        protected InputHandlerResult handleDirectVerseInput(
            UserSession us,
            string input)
        {
            try
            {
                int verse_id = -1;
                VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
                if (vs == null)
                {
                    Console.WriteLine("Expected Browse.verse_section present, but not found");
                    return new InputHandlerResult(
                       InputHandlerResult.UNDEFINED_MENU_ACTION,
                       InputHandlerResult.DEFAULT_MENU_ID,
                       InputHandlerResult.DEFAULT_PAGE_ID);
                }
                if (Int32.TryParse(input, out verse_id))
                {
                    Verse curr_start_verse = vs.start_verse;
                    Verse end_verse = vs.end_verse;
                    Verse start_verse = curr_start_verse.chapter.getVerse(verse_id);

                    if (start_verse != null)
                    {
                        //end_verse = getDefaultEndVerse(start_verse);
                        end_verse = start_verse;
                        vs = new VerseSection(start_verse, end_verse);//we set end verse to distinguish browsing from direct input
                        us.setVariable("Browse.verse_section", vs);
                        us.recordVerseSelection(start_verse, end_verse); 
                        return new InputHandlerResult(InputHandlerResult.NEW_MENU_ACTION,
                                    us.current_menu_loc,
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
                else
                {
                    VerseSection vs1 = (VerseSection)us.getVariableObject("Browse.verse_section");
                    if (vs1 == null)
                    {
                        Console.WriteLine("Expected Browse.verse_section present, but not found");
                        return new InputHandlerResult(
                           InputHandlerResult.UNDEFINED_MENU_ACTION,
                           InputHandlerResult.DEFAULT_MENU_ID,
                           InputHandlerResult.DEFAULT_PAGE_ID);
                    }
                    String current_book = "";
                    String current_chapter = "";
                    input = input.Replace(".", ":");
                    if(vs1 != null && vs1.start_verse != null)
                    {
                        current_book = vs1.start_verse.book.name;
                        current_chapter = vs1.start_verse.chapter.chapter_id.ToString(); // TODO: check the taking of chapter from start verse and not end verse
                    }
                    VerseSection vsection = Verse_Handler.getVerseSection(us, input, current_book, current_chapter);
                    if (vsection != null)
                    {
                        us.setVariable("Browse.verse_section", vsection);
                        Verse start_verse = vsection.start_verse;
                        Verse end_verse = vsection.end_verse;
                        us.recordVerseSelection(start_verse, end_verse); 
                        return new InputHandlerResult(
                            InputHandlerResult.NEW_MENU_ACTION,
                            us.current_menu_loc,
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

                
            }
            catch (Exception e)
            {
                return new InputHandlerResult(
                          InputHandlerResult.UNDEFINED_MENU_ACTION,
                          InputHandlerResult.DEFAULT_MENU_ID,
                          InputHandlerResult.DEFAULT_PAGE_ID);
            }
   
        }


        public const string DISPLAY_MORE = "MORE";
        public const string DISPLAY_NEXT_CHAPTER = "NXT_CHAPTER";
        public const string DISPLAY_PREV_CHAPTER = "PRV_CHAPTER";

        public const string ADD_TO_FAV = "ADD_TO_FAV";
        public const string SEND_TO_BUDDY = "SEND";
        public const string TAG_VERSE = "TAG_VERSE";

        public const string BROWSE_CLEAR_SCREEN= "Browse.clear_screen";
    }


}
