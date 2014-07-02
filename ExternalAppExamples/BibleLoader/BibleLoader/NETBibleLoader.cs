using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace BibleLoader
{
    public class NETBibleLoader
    {
        private static List<VerseRecordToInsert> record_list;
        public static void loadNetBible(String filePath)
        {
            //Directory.GetFiles(filePath, "*.sgm");
            Dictionary<String, String> booksToLoad = new Dictionary<String, String>();
            String line = "";
            int counter = 0;
            Console.WriteLine("reading filesToLoad");
            System.IO.StreamReader file = new System.IO.StreamReader(@filePath + "filesToLoad.lst");
            try
            {
                while ((line = file.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("#"))
                        continue;
                    String[] tokens = line.Split(':');
                    String file_book = "";
                    String fileName = "";
                    String book = "";
                    if (tokens.Count() >= 2)
                    {
                        if (LOAD_FILE_TAG.Equals(tokens[0]))
                        {
                            file_book = tokens[1];
                            tokens = file_book.Split('-');
                            if (tokens.Count() >= 2)
                            {
                                fileName = tokens[0] + ".fix";
                                book = tokens[1];
                                booksToLoad.Add(book, filePath + fileName);
                                Console.WriteLine(book + "|" + fileName);
                            }
                        }
                    }
                }
            }
            finally
            {
                file.Close();
            }

            Console.WriteLine("filesToLoad read complete");
            List<Book> book_list = BibleHelper.getListOfBooks();

            Boolean do_load = false;
            long rows_added = 0;
            long rows_read = 0;
            long total_rows_added = 0;
            long total_rows_read = 0;
            try
            {
                String book_name = "";
                String verse_to_load = "";
                int chapter_id = -1;
                int verse_id = -1;
                String first_line = "";
                String chap_ver = "";
                Console.WriteLine("loading books");
                for (int i = 0; i < book_list.Count(); i++)
                {
                    book_name = book_list[i].name;

                    if (booksToLoad.ContainsKey(book_name))
                    {
                        record_list = new List<VerseRecordToInsert>();
                        String file_name = (String)booksToLoad[book_name];
                        Console.WriteLine("Reading File: " + file_name);
                        file = new System.IO.StreamReader(file_name);
                        while ((line = file.ReadLine()) != null)
                        {
                            if (counter == 0)
                            {
                                first_line = line;
                            }
                            else
                            {

                                chap_ver = (line.Split(' '))[0];
                                if (chap_ver.Contains('<'))
                                {
                                    line = line.Insert(line.IndexOf('<'), " ");
                                    chap_ver = (line.Split(' '))[0];
                                } 
                                if (chap_ver.Contains("scstart"))
                                {
                                    line = line.Insert(line.IndexOf("scstart"), " ");
                                    chap_ver = (line.Split(' '))[0];
                                } 
                                chapter_id = Int32.Parse(chap_ver.Split(':')[0]);
                                verse_id = Int32.Parse(chap_ver.Split(':')[1]);
                                verse_to_load = line.Substring(line.IndexOf(' ') + 1);//(line.Split(' '))[1];
                                if (counter == 1)
                                {
                                    verse_to_load = first_line + "</p>" + verse_to_load;
                                }
                                /*else
                                {
                                    verse_to_load = verse_to_load
                                }*/
                                recordForInsert(NET_TRANSLATION_ID, book_name, chapter_id, verse_id, verse_to_load);
                                rows_read++;
                                if (rows_added > 2)
                                {
                                    Console.WriteLine("Error in load of verse: " + book_name + "|" + chapter_id + "|" + verse_id + "|" + verse_to_load);
                                    Console.ReadLine();
                                }
                                Console.WriteLine(chapter_id + ":" + verse_id);
                            }
                            counter++;
                        }
                        counter = 0;
                        Console.WriteLine(rows_read + " rows read for the book: " + book_name);
                        total_rows_read += rows_read;
                        rows_added = insertRecordsFromList();
                        record_list = null;
                        total_rows_added += rows_added;
                        rows_read = 0;
                        rows_added = 0; 
                        if(total_rows_added != total_rows_read)
                        {
                            Console.WriteLine("WARNING!!! total added not equal to total read. total read = " + total_rows_read + " AND total added = " + total_rows_added);
                        }
                    }
                    
                }
                do_load = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                do_load = false;
            }
            finally
            {
                file.Close();
            }
            Console.WriteLine("loading books completed");
            if (do_load)
            {
               
            }
            else
            {
                Console.WriteLine("There was an error loading a file so the data will not be uploaded");
            }
        }

        public static void recordForInsert(
            int translation,
            String book,
            int chapter,
            int verse,
            String text)
        {
            VerseRecordToInsert verse_record = new VerseRecordToInsert(
                translation,
                book,
                chapter,
                verse,
                text);

            record_list.Add(verse_record);
        }

        public static long insertRecordsFromList()
        {
            long rows_added = 0;
            long total_rows_added = 0;
            MySqlConnection conn = DBManager.getConnection();
            conn.Open();
            try
            {
                foreach (var record in record_list)
                {
                    rows_added = insertVerse(record, conn);
                    total_rows_added += rows_added;
                }
                Console.WriteLine("Total verses loaded = " + total_rows_added);
                return total_rows_added;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        /**
        * returns session ID
        * 
        **/
        public static long insertVerse(
            VerseRecordToInsert record,
            MySqlConnection conn
            )
        {
            
            try
            {
                

                string sqlQuery =
                    "INSERT INTO bible SELECT NULL as key_id, " + record.translation_id + ", b.id, " + record.chapter_id+ "," +record.verse_id +",@verse_text "+
                    "FROM books b WHERE b.name = '"+ record.book_name+ "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);

                cmd.Parameters.Add("@verse_text", MySql.Data.MySqlClient.MySqlDbType.Text);
                cmd.Parameters["@verse_text"].Value = record.verse_text;
                int output = cmd.ExecuteNonQuery();
                return output;
                //return 1;

                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                
            }
            return -1;
        }

        public const int NET_TRANSLATION_ID = 2;
        public const String LOAD_FILE_TAG = "file";
    }
}
