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
    class Verse_Handler : AInputHandler
    {
        public override void init(UserSession us)
        {
            if (us.getVariableObject("Browse.verse_section") != null)
            {
                us.deleteVariable("Browse.verse_section");
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

            //now handle input
            input = input.Trim();
            if (input.Equals(""))
            {
                return new InputHandlerResult(
            "Invalid entry...Please enter a valid input (e.g. 'John 3:16' or '1 John 1:9' or read the help section for more information"); //invalid choice
            }
            input = input.Replace('.', ':');
            Verse start_verse = null;
            Verse end_verse = null;
            if (input.Contains('-'))
            {
                String[] start_end = input.Split('-');

                start_verse = getStartingVerse(user_session.user_profile.getDefaultTranslationId(), start_end[0]);
                if (start_verse == null)
                {
                    return new InputHandlerResult(
                                "Invalid entry...Please enter a valid input (e.g. 'John 3:16' or '1 John 1:9'"); //invalid choice
                }
                if (start_end.Count() >= 2)
                {
                    end_verse = getEndingVerse(user_session.user_profile.getDefaultTranslationId(), start_verse, start_end[1]);
                }
            }
            else
            {
                start_verse = getStartingVerse(user_session.user_profile.getDefaultTranslationId(), input);
            }
            user_session.deleteVariable("Browse.verse_section");

            try
            {
                if (end_verse == null)
                    end_verse = start_verse;
                VerseSection vs = new VerseSection(start_verse, end_verse);
                user_session.setVariable("Browse.verse_section", vs);
                //now this is one big hack
                user_session.setVariable("Browse.directSelect", true);
            }
            catch (XInvalidVerseSection e)
            {
                return new InputHandlerResult(e.Message); //invalid choice
            }

            return new InputHandlerResult(
                     InputHandlerResult.NEW_MENU_ACTION,
                     vmp.input_item.target_page,
                     InputHandlerResult.DEFAULT_PAGE_ID);

        }

        public static VerseSection getVerseSection(
            UserSession us,
            String verse_section,
            String current_book_name,
            String current_chapter)
        {
            verse_section = verse_section.Trim();
            Verse start_verse = null;
            Verse end_verse = null;
            String book = getBookName(verse_section).Trim();
            book = BibleHelper.getFullBookName(book);
            if (book == null || (book != null && !isValidBook(us.user_profile.getDefaultTranslationId(), book)))//TODO fix this, throw Exception if book is invalid. otherwise we going to give user wrong section 
            {
                if (verse_section.Contains('-'))
                {
                    String[] start_end = verse_section.Split('-');
                    String start = start_end[0];
                    if (!start.Contains(':'))
                        verse_section = current_chapter + ":" + start;
                    if (start_end.Count() > 1)
                        verse_section = verse_section + "-" + start_end[1];
                }
                verse_section = current_book_name + " " + verse_section;
            }
            else
            {
                if (verse_section.Contains('-'))
                {
                    String[] start_end = verse_section.Split('-');
                    String start = start_end[0];
                    if (!start.Contains(':'))
                        verse_section = current_chapter + ":" + start;
                    if(start_end.Count() > 1)
                            verse_section = verse_section + "-" + start_end[1];
                }
            }

            if (verse_section.Contains('-'))
            {
                String[] s_e = verse_section.Split('-');

                start_verse = getStartingVerse(us.user_profile.getDefaultTranslationId(), s_e[0]);
                if (start_verse == null)
                {
                    return null;
                }
                if (s_e.Count() >= 2)
                {
                    end_verse = getEndingVerse(us.user_profile.getDefaultTranslationId(), start_verse, s_e[1]);
                }
            }
            else
            {
                start_verse = getStartingVerse(us.user_profile.getDefaultTranslationId(), verse_section);
            }

            if (end_verse == null)
                end_verse = start_verse;
            VerseSection vs = new VerseSection(start_verse, end_verse);
            return vs;

        }

        public static Verse getStartingVerse(String translation, String verse)
        {
            if (verse == null)
            {
                return null;
            }
            string[] tokens = verse.Trim().Split(' ');
            if (tokens.Count() > 0)
            {
                int num;
                string testament = "";
                string book = "";
                string chapter = "";
                string start_verse = DEFAULT_VAL;

                book = getBookName(verse).Trim();
                //check if this is a short abbr for the book. 
                book = BibleHelper.getFullBookName(book);
                //now we check book
                //be sure not to actually use these objects for displaying verse since they not tied to translation yet
                if (!isValidBook(translation, book))
                {
                    return null;
                }
                testament = getTestament(translation, book);

                String chap_ver = getVerseAndChapter(verse);

                if (chap_ver == null || (chap_ver != null && chap_ver == ""))
                {
                    chap_ver = "1:1";
                }

                if (chap_ver.Contains(":"))
                {
                    string[] chapter_verse = chap_ver.Split(':');
                    if (chapter_verse.Count() >= 2)//dont care whats after the first 
                    {
                        chapter = chapter_verse[0];

                        if (!isValidChapterInBook(translation, chapter, book))
                        {
                            return null;
                        }

                        string verse_loc = chapter_verse[1];

                        if (isValidVerseInChapter(translation, verse_loc, chapter, book))
                        {
                            start_verse = verse_loc;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    chapter = chap_ver;

                    if (!isValidChapterInBook(translation, chapter, book))
                    {
                        return null;
                    }
                    start_verse = "1";
                }


                Verse start_v = BibleContainer.getInstance().getVerse(
                        Int32.Parse(translation),
                        Int32.Parse(testament),
                        book,
                        Int32.Parse(chapter),
                        Int32.Parse(start_verse));
                return start_v;
            }
            else
            {
                return null;
            }
        }

        /*public static Verse getStartingVerse(String verse)
        {
            return getStartingVerse( ,verse);
        }*/


        public static Verse getEndingVerse(String translation, Verse start_verse, String verse_loc)
        {
            verse_loc = verse_loc.Trim();
            if (verse_loc.Contains(":"))
            {
                String[] chap_verse = verse_loc.Split(':');
                if (chap_verse.Count() < 2 || chap_verse.Count() > 2)
                {
                    return null;
                }
                if (isValidChapterInBook(
                    translation, 
                    chap_verse[0],
                    start_verse.book.name))
                {
                    if (isValidVerseInChapter(
                    translation,
                    chap_verse[1],
                    chap_verse[0],
                    start_verse.book.name))
                    {
                        int verse;
                        if (!Int32.TryParse(chap_verse[1], out verse))
                        {
                            return null;
                        }
                        int chap;
                        if (!Int32.TryParse(chap_verse[0], out chap))
                        {
                            return null;
                        }
                        return start_verse.book.getChapter(chap).getVerse(verse);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (isValidVerseInChapter(
                    translation,
                    verse_loc,
                    (start_verse.chapter.chapter_id).ToString(),
                    start_verse.book.name))
                {
                    int verse;
                    if (!Int32.TryParse(verse_loc, out verse)) //entry is like e.g. 1 John or 2 Corinthians
                    {
                        return null;
                    }
                    return start_verse.book.getChapter(start_verse.chapter.chapter_id).getVerse(verse);
                }
                else
                {
                    return null;
                }
            }
        }

        //validate against NET
        public static bool validateVerseReference(String input)
        {

            //now handle input
            input = input.Trim();
            if (input.Equals(""))
            {
                return false;
            }
            input = input.Replace('.', ':');
            Verse start_verse = null;
            Verse end_verse = null;
            
            if (input.Contains('-'))
            {
                String[] start_end = input.Split('-');

                start_verse = getStartingVerse("2", start_end[0]);
                if (start_verse == null)
                {
                    return false;
                }
                if (start_end.Count() >= 2)
                {
                    end_verse = getEndingVerse("2", start_verse, start_end[1]);
                }
            }
            else
            {
                start_verse = getStartingVerse("2", input);
            }
            try
            {
                if (end_verse == null)
                    end_verse = start_verse;
                VerseSection vs = new VerseSection(start_verse, end_verse);
            }
            catch (XInvalidVerseSection e)
            {
                return false;
            }
            return true;
        }

        private static Boolean isValidBook(String translation,string book)
        {
            Book book_from_any_translation = BibleContainer.getBookId(translation, book);
            if (book_from_any_translation == null)
            {
                return false; //book not valis
            }
            return true;
        }

        private static string getTestament(String translation, string book)
        {
            Book book_from_any_translation = BibleContainer.getBookId(translation, book);
            return book_from_any_translation.testament.testament_id.ToString();
        }

        private static Boolean isValidChapterInBook(String translation, string chapter, string b)
        {
            int chap_num;
            if (!Int32.TryParse(chapter, out chap_num)) //entry is like e.g. 1 John or 2 Corinthians
            {
                return false;
            }
            Book book_from_any_translation = BibleContainer.getBookId(translation, b);
            if (book_from_any_translation == null)
            {
                return false; //book not valis
            }
            Chapter chapter_from_any_translation = BibleContainer.getInstance().getChapter(
                ref book_from_any_translation,
                chap_num);
            if (chapter_from_any_translation == null)
            {
                return false;
            }
            return true;
        }



        private static Boolean isValidVerseInChapter(String translation, string verse, string c, string b)
        {
            Book book_from_any_translation = BibleContainer.getBookId(translation, b);
            if (book_from_any_translation == null)
            {
                return false; //book not valis
            }
            int chap_num;
            if (!Int32.TryParse(c, out chap_num)) //entry is like e.g. 1 John or 2 Corinthians
            {
                return false;
            }
            Chapter chapter_from_any_translation = BibleContainer.getInstance().getChapter(
                ref book_from_any_translation,
                chap_num);
            if (chapter_from_any_translation == null)
            {
                return false;
            }
            int verse_num;
            if (!Int32.TryParse(verse, out verse_num)) //entry is like e.g. 1 John or 2 Corinthians
            {
                return false;
            }
            Verse verse_from_any_translation = BibleContainer.getInstance().getVerse(
                ref chapter_from_any_translation,
                verse_num);
            if (verse_from_any_translation == null)
            {
                return false;
            }
            return true;

        }

        public static int firstIntIndex(String s)
        {
            int min_index = Int32.MaxValue;
            for (int i = 0; i < 10; i++)
            {
                if (s.IndexOf(i.ToString()) != -1 &&
                    s.IndexOf(i.ToString()) < min_index)
                    min_index = s.IndexOf(i.ToString());
            }

            if (min_index == Int32.MaxValue)
                return -1;
            else
                return min_index;
        }

        public static int firstVerseIndex(String s)
        {
            int index = firstIntIndex(s);
            int count = 0;
            while (index == 0)
            {
                count++;
                index = firstIntIndex(s.Substring(count));
            }
            if (index != -1)
            {
                return index + count;
            }
            else
            {
                return index;
            }
        }

        public static String getBookName(String b)
        {
            if (b != null && b != "")
            {
                int index = firstVerseIndex(b);
                if (index != -1)
                    return b.Substring(0, index).Trim();
                else
                    return b;
            }
            else
            {
                return null;
            }
        }

        public static String getVerseAndChapter(String b)
        {
            String book_name = getBookName(b);
            return (b.Replace(book_name, "")).Trim();
        }


        public const int TRANSLATION = 1;
        public const string DEFAULT_VAL = "-1";
    }
}


