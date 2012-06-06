using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class BibleContainer
    {
        private static BibleContainer instance;
        public static Hashtable bibles = new Hashtable();
        
       

        public BibleContainer()
        {
            bibles.Add(0,new Bible(new Translation(0, "King James Version", "KJV")));
            bibles.Add(1,new Bible(new Translation(1, "World English Bible", "WEB")));
            bibles.Add(2, new NetBible(new Translation(2, "New English Translation", "NET")));
        }

        public static BibleContainer getInstance()
        {
            if (instance == null)
                instance = new BibleContainer();

            return instance;
        }

        //this looks totally WRONG!. 
        public static ArrayList getTranslations()
        {
            ArrayList list = new ArrayList(bibles.Values);
            return list;
        }

        public Bible getBible(int tran_id)
        {
            return (Bible)bibles[tran_id];
        }

        public static String getTranslationFullName(int tran_id)
        {
            return ((Bible)bibles[tran_id]).translation.full_name;
        }

        public Verse getVerse(
            int translation,
            int testament_id,
            string book,
            int chapter_id,
            int verse_id)
        {
            return getInstance().getBible(translation).getTestament(testament_id).getBook(book).getChapter(chapter_id).getVerse(verse_id);
        }

        //returns book of first translation (books should never change in different translations).
        public static Book getBookId(String book_name)
        {
            if (bibles.Count > 0)
            {
                if (((Bible)bibles[0]).testaments.Count == 2)
                {
                    Testament old_test = ((Bible)bibles[0]).getTestament(Testament.OLD_TESTAMENT);
                    Book tmp_book = old_test.getBook(book_name);
                    if (null != tmp_book)
                    {
                        return tmp_book;
                    }
                    else
                    {
                        Testament new_test = ((Bible)bibles[0]).getTestament(Testament.NEW_TESTAMENT);
                        tmp_book = new_test.getBook(book_name);
                        if (null != tmp_book)
                        {
                            return tmp_book;
                        }
                        else
                        {
                            return null;
                        }
                    }
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

        

        //use this method instead of the one that does not take on usersession
        public static Book getBookId(String translation_id, String book_name)
        {
            // String translation_id = us.user_profile.getDefaultTranslationId();
            int tran_id = Int32.Parse(translation_id);
            if (bibles.Count > 0)
            {
                if (((Bible)bibles[tran_id]).testaments.Count == 2)
                {
                    Testament old_test = ((Bible)bibles[tran_id]).getTestament(Testament.OLD_TESTAMENT);
                    Book tmp_book = old_test.getBook(book_name);
                    if (null != tmp_book)
                    {
                        return tmp_book;
                    }
                    else
                    {
                        Testament new_test = ((Bible)bibles[tran_id]).getTestament(Testament.NEW_TESTAMENT);
                        tmp_book = new_test.getBook(book_name);
                        if (null != tmp_book)
                        {
                            return tmp_book;
                        }
                        else
                        {
                            return null;
                        }
                    }
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


        public Chapter getChapter(ref Book tmp_book, int chapter)
        {
            return (Chapter) tmp_book.getChapter(chapter);
        }
        
        public Verse getVerse(ref Chapter tmp_chapter, int verse)
        {
            return (Verse)tmp_chapter.getVerse(verse);
        }

        public static String getSummaryOfVerse(Verse verse, int n)
        {
            //get Bible to check which translation it is. 
            if (verse.chapter.testament.translation.name.Equals(NET_ABBR_CODE))
            {
                String text = NetBible.getVerseTextOnly(verse);
                String summary = GetFirstNWords(text, n);
                return summary;
            }
            else
            {
                return GetFirstNWords(verse.text, n);
            }
            
        }


        public static string GetFirstNWords(string text, int maxWordCount)
        {
            int wordCounter = 0;
            int stringIndex = 0;
            char[] delimiters = new[] { '\n', ' ', ',', '.' };

            while (wordCounter < maxWordCount)
            {
                stringIndex = text.IndexOfAny(delimiters, stringIndex + 1);
                if (stringIndex == -1)
                    return text;

                ++wordCounter;
            }

            return text.Substring(0, stringIndex);
        }


        public const String KJV_FULL_NAME = "King James Version";
        public const String KJV_ABBR_CODE = "KJV";

        public const String WEB_FULL_NAME = "World English Bible";
        public const String WEB_ABBR_CODE = "WEB";

        public const String NET_FULL_NAME = "New English Translation";
        public const String NET_ABBR_CODE = "NET";
    }
}
