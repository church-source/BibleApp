using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class BibleTopicManager
    {
        private static BibleTopicManager instance = null;
        private static Dictionary<int,Category> topic_categories;
        private static Object lockObject = new Object();
        static BibleTopicManager()
        {

            topic_categories = new Dictionary<int,Category>();
            string sqlQuery = "SELECT id, category FROM bibletopiccategories";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                int cat_id = -1;
                String category = "";
                Category a_cat = null;
                while (rdr.Read())
                {
                    cat_id = Int32.Parse((rdr[0]).ToString());
                    category = (rdr[1]).ToString();
                    a_cat = new Category(cat_id, category);
                    topic_categories.Add(cat_id, a_cat);
                    loadTopicForCategory(a_cat);        
                }
                rdr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static BibleTopicManager getInstance()
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new BibleTopicManager();
                    }
                }
            }
            return instance;
        }

        public static void loadTopicForCategory(Category category)
        {
            string sqlQuery = "SELECT topic_id, topic, verse_ref FROM bibletopics WHERE category_id = '"+category.category_id+"'";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                int topic_id = -1;
                String topic_name = "";
                String topic_verse_ref = "";
                Topic topic = null;
                while (rdr.Read())
                {
                    topic_id = Int32.Parse((rdr[0]).ToString());
                    topic_name = (rdr[1]).ToString();
                    topic_verse_ref = (rdr[2]).ToString();
                    topic = new Topic(topic_id, topic_name, topic_verse_ref);
                    category.topics.Add(topic);
                }
                rdr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                conn.Close();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public Category getCategory(int category_id)
        {
            if(topic_categories.ContainsKey(category_id))
                return topic_categories[category_id];

            return null;
        }

        public List<Category> getListOfCategories()
        {
            return ListUtils.convertTopicCategoryDictionaryToList(topic_categories);
            //return topic_categories.ToList<>;
        }
    }
}
