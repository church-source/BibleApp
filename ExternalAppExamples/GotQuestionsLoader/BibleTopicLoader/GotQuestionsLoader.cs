using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace GotQuestionsLoader
{
    public class GotQuestionsLoader
    {
        private static List<Category> topic_categories;

        public static void loadTopics(String filePath)
        {
            Dictionary<String, String> topics = new Dictionary<String, String>();

            topic_categories = new List<Category>();
            //Directory.GetFiles(filePath, "*.sgm");

            Console.WriteLine("reading topics");
            XDocument xmlDoc = XDocument.Load(filePath);

            var q = from qas in xmlDoc.Descendants("QuestionsAndAnswers")
                    from qa in qas.Elements("QuestionAndAnswer")
                    select qa.Element("topic");
                    //from qa in qas.Descendants("QuestionsAndAnswer")
                    //select qa.Element("topic");
                    //from topic in qa.Elements("topic")
                    //select topic.Element("topic");


            foreach (var item in q)
            {
                String key = item.Value.Trim();
                if(!topics.ContainsKey(key))
                {
                    topics.Add(key,key);   
                }
            }

            foreach (String val in topics.Values)
            {
                Console.WriteLine(val);
            }

 //           XElement po = xmlDoc.Root.Element().Element("topic");

            // LINQ to XML query
   /*         IEnumerable<XElement> list1 = po.Elements();
            foreach (XElement el in list1)
            {
                Console.WriteLine(el);
            }*/
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
