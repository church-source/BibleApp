using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace BibleTopicLoader
{
    public class BibleTopicLoader
    {
        private static List<Category> topic_categories;

        public static void loadTopics(String filePath)
        {
            topic_categories = new List<Category>();
            //Directory.GetFiles(filePath, "*.sgm");

            String line = "";
            int counter = 0;
            Console.WriteLine("reading topicsToLoad");
            System.IO.StreamReader file = new System.IO.StreamReader(@filePath + "topics.txt");
            try
            {
                Category cat = null;
                while ((line = file.ReadLine()) != null)
                {
                    line = line.Trim();
                    //check if new category
                    String category = "";
                    if (line.StartsWith("*"))
                    {
                        category = line.Substring(1);
                        cat = new Category(category);
                        topic_categories.Add(cat);
                    }
                    else
                    {
                        if (cat != null)
                        {
                            category = line.Substring(0);
                            String[] topic_and_verse = category.Split('~');
                            Topic topic = new Topic(
                                topic_and_verse[0].Trim(),
                                topic_and_verse[1].Trim());
                            cat.topics.Add(topic);
                        }
                    }
                }
            }
            finally
            {
                file.Close();
            }
            loadTopics();
        }

        public static void loadTopics()
        {
            MySqlConnection conn = DBManager.getConnection();
            conn.Open();
            try
            {
                foreach (var cat in topic_categories)
                {
                    string sqlQuery = "INSERT INTO bibletopiccategories VALUES(NULL,'" + cat.name + "')";
                    MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                    int output = cmd.ExecuteNonQuery();
                    long category_id = cmd.LastInsertedId;
                    List<Topic> topics = cat.topics;
                    foreach (var topic in topics)
                    {
                        sqlQuery = "INSERT INTO bibletopics VALUES(NULL,'" + category_id + "',@topic,@verse)";
                        cmd = new MySqlCommand(sqlQuery, conn);
                        
                        cmd.Parameters.Add("@topic", MySql.Data.MySqlClient.MySqlDbType.Text);
                        cmd.Parameters["@topic"].Value = topic.topic;

                        cmd.Parameters.Add("@verse", MySql.Data.MySqlClient.MySqlDbType.Text);
                        cmd.Parameters["@verse"].Value = topic.verse_ref;

                        cmd.ExecuteNonQuery();
                    }
                    //return 1;
                }
            }
            finally
            {
                conn.Close();
            }
        }


        public const int NET_TRANSLATION_ID = 2;
        public const String LOAD_FILE_TAG = "file";
    }
}
