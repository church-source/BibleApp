using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace BibleLoader
{
    public class BibleContainer
    {
        private static BibleContainer instance;
        public static Hashtable bibles = new Hashtable();
        
       

        public BibleContainer()
        {
            bibles.Add(0,new Bible(new Translation(0, "King James Version", "KJV")));
            bibles.Add(1,new Bible(new Translation(1, "World English Bible", "WEB")));
            //bibles.Add(2, new NetBible(new Translation(2, "New English Translation", "NET")));
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
        public Book getBookId(String book_name)
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

        public Chapter getChapter(ref Book tmp_book, int chapter)
        {
            return (Chapter) tmp_book.getChapter(chapter);
        }
        
        public Verse getVerse(ref Chapter tmp_chapter, int verse)
        {
            return (Verse)tmp_chapter.getVerse(verse);
        }



    }
}
