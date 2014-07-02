using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace BibleLoader
{
    public class BibleHelper
    {
        //abbrs mapped to the books, many to one
        public static Dictionary<string, string> abbr_book_map = new Dictionary<string, string>();
        //books mapped to their abbrs, book, with comma delimited string. change this in future. 
        public static Dictionary<string, string> book_abbr_map = new Dictionary<string, string>();
        static BibleHelper()
        {
            string sqlQuery = "SELECT book_name, abbrs FROM book_abbr"; 
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                String book_name = "";
                String book_abbrs = "";
                String abbr = "";
                String[] abbrs = null;
                while (rdr.Read())
                {
                    book_name = (rdr[0]).ToString();
                    book_abbrs = (rdr[1]).ToString();
                    book_abbr_map.Add(book_name.ToUpper(), book_abbrs);
                    abbrs = book_abbrs.Split(',');
                    for(int i=0; i < abbrs.Count(); i++)
                    {
                        abbr = abbrs[i].Trim();
                        if (!abbr_book_map.ContainsKey(abbr))
                        {
                            abbr_book_map.Add(abbr.ToUpper(), book_name);
                        }
                        else
                        {
                            //do nothing, we found two books with same abbr in table, 
                        }
                        
                    }
                }
                rdr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
            }
        }

        //returns full name of book if it is an abbreviation
        public static String getFullBookName(String book_abbr)
        {
            if (abbr_book_map.ContainsKey(book_abbr.ToUpper()))
            {
                return abbr_book_map[book_abbr.ToUpper()];
            }
            else
            {
                return book_abbr;
            }
        }

        //returns comma delimited string list of short codes for given book. 
        public static String getShortCodeStringList (String book)
        {
            if (book_abbr_map.ContainsKey(book.ToUpper()))
            {
                return book_abbr_map[book.ToUpper()];
            }
            else
            {
                return "";
            }
        }

        /*add finally blocks*/
        public static List<Book> getListOfBooks()
        {
            string sqlQuery = "Select id, abbr, testament, name From books Order by id";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                List<Book> list = new List<Book>();
                Testament old_testament = new Testament(0, Testament.OLD_TESTAMENT_NAME);
                Testament new_testament = new Testament(1, Testament.NEW_TESTAMENT_NAME);
                while (rdr.Read())
                {
                    //quick hack
                    int testament_id = Int32.Parse(rdr[2].ToString());
                    if (testament_id == 0)
                    {
  
                        list.Add(
                            new Book(
                                Int32.Parse((rdr[0]).ToString()),
                                rdr[3].ToString(),
                                ((rdr[1]).ToString()),
                                ref old_testament));
                    }
                    else
                    {

                        list.Add(
                            new Book(
                                Int32.Parse((rdr[0]).ToString()),
                                rdr[3].ToString(),
                                ((rdr[1]).ToString()),
                                ref new_testament));
                    }
                }
                rdr.Close();
                conn.Close();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
                return null;
            }
        }

        public static List<String> getListOfChapters(string book_id)
        {
            string sqlQuery = "SELECT DISTINCT chapter FROM bible WHERE book = " 
                + book_id; 
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                List<String> list = new List<String>();
                while (rdr.Read())
                {
                    list.Add((rdr[0]).ToString());
                }
                rdr.Close();
                conn.Close();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
                return null;
            }
        }

        /**/

        public static Hashtable getListOfChapters()
        {
            string sqlQuery = "SELECT name, count(DISTINCT Chapter)  as chapter_count" 
            + " FROM bible INNER JOIN books ON (books.id = bible.Book) WHERE translation = 1 GROUP BY book, translation";

            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                Hashtable book_chapters = new Hashtable();
                while (rdr.Read())
                {
                    book_chapters.Add((rdr[0]).ToString(), (rdr[1]).ToString());
                }
                rdr.Close();
                conn.Close();
                return book_chapters;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
                return null;
            }
        }

        public static List<Testament> getTestaments()
        {
            string sqlQuery = "SELECT * FROM testament";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                List<Testament> testaments = new List<Testament>();
                while (rdr.Read())
                {
                    testaments.Add(new Testament(Int32.Parse((rdr[0]).ToString()),
                                                               (rdr[1]).ToString()));
                }
                rdr.Close();
                return testaments;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                conn.Close();
            }
        }

        /*this adds the books to the testament*/
        public static void addTestamentBooks(ref Testament testament)
        {
            string sqlQuery = "Select id, abbr, testament, name From books WHERE "
            +" testament ='"+ testament.testament_id+"' Order by id";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                Book prev_book = null;
                Book next_book = null;
                if (rdr.HasRows)
                {
                    rdr.Read();
                    Book curr_book = new Book(
                                  Int32.Parse((rdr[0]).ToString()),
                                  rdr[3].ToString(),
                                  ((rdr[1]).ToString()),
                                  ref testament);
                    while (curr_book != null)
                    {
                        //curr_chapter.prev_chapter = prev_chapter;
                        if (rdr.Read())
                        {
                            next_book = new Book(
                                                  Int32.Parse((rdr[0]).ToString()),
                                                  rdr[3].ToString(),
                                                  ((rdr[1]).ToString()),
                                                  ref testament);
                            curr_book.prev_book = prev_book;
                            curr_book.next_book = next_book;
                            testament.addBook(ref curr_book);
                            prev_book = curr_book;
                            curr_book = next_book;
                        }
                        else
                        {
                            curr_book.prev_book = prev_book;
                            curr_book.next_book = null;
                            testament.addBook(ref curr_book);
                            break;
                        }

                    }
                }
                /*while (rdr.Read())
                {
                    Book aBook = new Book(
                                Int32.Parse((rdr[0]).ToString()),
                                rdr[3].ToString(),
                                ((rdr[1]).ToString()),
                                ref testament);

                     testament.addBook(ref aBook);
                }*/
                rdr.Close();
                conn.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();

            }
        }

        /*this adds the chapters to the book*/
        public static void addBookChapters(ref Book book)
        {
            string sqlQuery = "SELECT DISTINCT Chapter"
            + " FROM bible WHERE translation = 1 AND book ='" + book.id + "'";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                Chapter prev_chapter = null;
                Chapter next_chapter = null;
                if (rdr.HasRows)
                {
                    rdr.Read();
                    Testament book_test = book.testament;
                    Chapter curr_chapter = new Chapter(
                                    Int32.Parse((rdr[0]).ToString()),
                                    ref book_test,
                                    ref book);
                    while (curr_chapter != null)
                    {
                        //curr_chapter.prev_chapter = prev_chapter;
                        if (rdr.Read())
                        {
                            next_chapter = new Chapter(
                                                    Int32.Parse((rdr[0]).ToString()),
                                                    ref book_test,
                                                    ref book);
                            curr_chapter.prev_chapter = prev_chapter;
                            curr_chapter.next_chapter = next_chapter;
                            book.addChapter(ref curr_chapter);
                            prev_chapter = curr_chapter;
                            curr_chapter = next_chapter;
                        }
                        else
                        {
                            curr_chapter.prev_chapter = prev_chapter;
                            curr_chapter.next_chapter = null;
                            book.addChapter(ref curr_chapter);
                            break;
                        }

                    }
                }
                rdr.Close();
                conn.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();

            }
        }

        /*this adds the verses to the chapter*/
        public static void addChapterVerses(ref Chapter chapter, ref Translation translation)
        {
            Testament book_test = chapter.testament;
            Book book = chapter.book;
            string sqlQuery = "SELECT Verse, VerseText"
            + " FROM bible WHERE translation = '" + translation.translation_id+ "' AND book ='" + book.id + "'" 
            + " AND Chapter = '"+chapter.chapter_id+"' ORDER BY Verse";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                Verse prev_verse = null;
                Verse next_verse = null;
                if (rdr.HasRows)
                {
                    rdr.Read();

                    Verse curr_verse = new Verse(
                                            Int32.Parse((rdr[0]).ToString()), 
                                            (rdr[1]).ToString(),
                                            ref book_test,
                                            ref book,
                                            ref chapter,
                                            ref translation);

                    while (curr_verse != null)
                    {
                        //curr_chapter.prev_chapter = prev_chapter;
                        if (rdr.Read())
                        {

                            next_verse = new Verse(
                                            Int32.Parse((rdr[0]).ToString()), 
                                            (rdr[1]).ToString(),
                                            ref book_test,
                                            ref book,
                                            ref chapter,
                                            ref translation
                                            );
                            curr_verse.prev_verse = prev_verse;
                            curr_verse.next_verse = next_verse;
                            chapter.addVerse(ref curr_verse);
                            prev_verse = curr_verse;
                            curr_verse = next_verse;
                        }
                        else
                        {
                            curr_verse.prev_verse = prev_verse;
                            curr_verse.next_verse = null;
                            curr_verse.is_last_verse_of_chapter = true;
                            chapter.addVerse(ref curr_verse);
                            break;
                        }

                    }
                }
                rdr.Close();
                conn.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();

            }
        }

        /*the start and end for now cant span more than one book*/
        public static String getVerseSectionReference(Verse start_verse, Verse end_verse)
        {

            if (start_verse != null)
            {
                return getVerseSectionReferenceWithoutTranslation(start_verse, end_verse) + " (" + start_verse.translation.name + ")";
            }
            else
            {
                return "";
            }
        }

        /*the start and end for now cant span more than one book*/
        public static String getVerseSectionReferenceWithoutTranslation(Verse start_verse, Verse end_verse)
        {
            string section = "";
            if (start_verse != null)
            {

                section = start_verse.book.name + " "
                    + start_verse.chapter.chapter_id + ":" + start_verse.verse_id;
                if (end_verse == null || (end_verse != null
                                                && start_verse.getVerseReference() == end_verse.getVerseReference()))
                {
                    //dont do anything
                }
                else if (start_verse.chapter == end_verse.chapter)
                {
                    section += "-" + end_verse.verse_id;
                }
                else if (start_verse.chapter == end_verse.chapter)
                {
                    section += "-" + end_verse.chapter + ":" + end_verse.verse_id;
                }
            }
            return section;
        }
    }
}
