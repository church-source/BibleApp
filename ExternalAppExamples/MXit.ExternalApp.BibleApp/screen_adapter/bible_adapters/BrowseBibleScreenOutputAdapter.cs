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
    class BrowseBibleScreenOutputAdapter : AScreenOutputAdapter
    {
        public override MessageToSend getOutputScreenMessage(
            UserSession us,
            MenuPage mp,
            MessageToSend ms,
            InputHandlerResult ihr)
        {
            if (!mp.GetType().FullName.Equals("MxitTestApp.OptionMenuPage"))//TODO: Should be constant
                throw new Exception("Invalid menu page passed into getScreen method ");

            OptionMenuPage omp = (OptionMenuPage)mp;
            ms.Append(omp.title + "\r\n", TextMarkup.Bold);
            if (ihr.error != null && ihr.action == InputHandlerResult.INVALID_MENU_ACTION)
            {
                ms.Append((string)ihr.error + "\r\n");
                //dont clear screen after this. 
                appendMessageConfig(false, ms);
            }
            else
            {
                try
                {
                    VerseSection vs = (VerseSection)us.getVariableObject("Browse.verse_section");
                    if (vs == null)
                    {
                        Console.WriteLine("Expected Browse.verse_section present, but not found");
                    }
                    Verse start_verse = vs.start_verse;
                    Verse end_verse = vs.end_verse;
                    if (end_verse == null)
                    {
                        end_verse = getDefaultEndVerse(start_verse);
                    }
                    vs = new VerseSection(start_verse, end_verse);
                    us.setVariable("Browse.verse_section", vs);
                    //this is not going to work when user goes from verse direct menu
                    Boolean clear_screen = false;
                    try
                    {
                        
                        if (us.hasVariable(Browse_Bible_Handler.BROWSE_CLEAR_SCREEN))
                        {
                            Object o = us.removeVariable(Browse_Bible_Handler.BROWSE_CLEAR_SCREEN);
                            clear_screen = (Boolean)o;
                        } 
                        
                    }
                    catch (Exception e)
                    {
                        clear_screen = false;
                    }
                    if (clear_screen)
                    {
                        ms.Append(MessageBuilder.Elements.CreateClearScreen());
                    }



                    List<Verse> list = getVerseList(start_verse, end_verse);
                    if (ihr.message != null && ihr.action == InputHandlerResult.FAVOURITE_ADDED_ACTION)
                    {
                        ms.Append((string)ihr.message + "\r\n");
                        //dont clear screen after this. 
                        appendPaginateLinks(list, ms);

                    }
                    else
                    {
                        
                        string section = BibleHelper.getVerseSectionReference(start_verse, end_verse);
                        UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
                        if(uct!= null)
                            ms.Append(section, uct.getBibleTextColour(),TextMarkup.Bold);
                        else
                            ms.Append(section, TextMarkup.Bold);
                        BibleContainer.getInstance().getBible(
                        start_verse.translation.translation_id).parseAndAppendBibleText(
                            list,
                            ms,
                            uct);
                        us.saveBookmarkVerse(start_verse, end_verse);
                        appendPaginateLinks(list, ms);
                        appendFavouriteLink(us, ms);
                    }

                    if (clear_screen)
                    {
                        appendMessageConfig(true, ms);
                    }
                    else
                    {
                        //dont clear screen after this. 
                        appendMessageConfig(false, ms);
                    }

                }
                catch (Exception e)
                {

                    ms.Append("An error has occured, please try a different book/chapter \r\n");
                    appendBackMainLinks(us,  ms);
                    return ms;
                }
            }

            /*List<MenuOptionItem> options = omp.options;
            int count =1 ;
            foreach (MenuOptionItem option in options)
            {
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", option.link_val));
                ms.Append(option.display_text + "\r\n");
                count++;
            }*/

            appendBackMainLinks(us, ms);
            return ms;
            //return output;
        }

        public static List<Verse> getVerseList(Verse start_verse, Verse end_verse)
        {
            List<Verse> list = new List<Verse>();

            Verse curr_verse = start_verse;
            list.Add(curr_verse);

            if (end_verse == null)
                return list;

            int count = 0;
            while (null == curr_verse || (curr_verse.getVerseReference() != end_verse.getVerseReference()))
            {

                if (curr_verse != null && curr_verse.next_verse != null)
                {
                    curr_verse = curr_verse.next_verse;
                }
                else if (curr_verse != null && curr_verse.next_verse == null)
                {
                    curr_verse = (Verse)curr_verse.chapter.next_chapter.verses[1];
                }

                if (curr_verse != null)
                    list.Add(curr_verse);

                count++;
                if (count > 50) //for safety 
                {
                    break;
                }
            }
            return list;
        }



        //this adds pagination links depending on the count passed into it and the current page the user
        //is on
        public void appendPaginateLinks(
            List<Verse> verses,
            MessageToSend ms)
        {
            bool more_link = false;
            bool next_c_link = false;
            //bool prev_c_link = false;
            if (verses != null &&
                verses[verses.Count - 1] != null &&
                verses[verses.Count - 1].next_verse != null)
            {

                ms.Append(createMessageLink(MENU_LINK_NAME, "More", Browse_Bible_Handler.DISPLAY_MORE));
                more_link = true;
            }
            if (verses != null && verses[0] != null)
            {
                if (verses[0].chapter.next_chapter != null)
                {
                    //just to add pipe character
                    if (more_link)
                    {
                        ms.Append(" | ");
                    }
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Nxt Chapt", Browse_Bible_Handler.DISPLAY_NEXT_CHAPTER));
                    next_c_link = true;
                }
                if (verses[0].chapter.prev_chapter != null)
                {
                    //just to add pipe character
                    if (next_c_link || more_link)
                    {
                        ms.Append(" | ");
                    }
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Prev Chapt", Browse_Bible_Handler.DISPLAY_PREV_CHAPTER));
                    //prev_c_link = true;
                }
            }
            ms.Append("\r\n");
        }

        //check if we must check if link is full. 
        public void appendFavouriteLink(
           UserSession us,
            MessageToSend ms)
        {
            //if (us.favourite_verses.isFavouriteListFull())
            ms.Append(createMessageLink(MENU_LINK_NAME, "Send", Browse_Bible_Handler.SEND_TO_BUDDY));
            ms.Append(" | ");
            ms.Append(createMessageLink(MENU_LINK_NAME, "Add to Favourites", Browse_Bible_Handler.ADD_TO_FAV));
            ms.Append(" | ");
            ms.Append(createMessageLink(MENU_LINK_NAME, "Tag", Browse_Bible_Handler.TAG_VERSE));
            ms.Append("\r\n");
        }

        public static Verse getDefaultEndVerse(Verse start_verse)
        {
            Verse tmp = null;
            Verse verse = start_verse;
            for (int i = 1; i < VERSE_DISPLAY_COUNT; i++)
            {
                if (verse != null && verse.next_verse != null)
                {
                    tmp = verse.next_verse;
                    verse = tmp;
                }
                else
                    continue; //some translation's might not have a particular verse


            }
            return verse;
        }

        public static int VERSE_DISPLAY_COUNT = 5;
    }
}
