using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace GotQuestionsLoader
{
    public class Category
    {
        public string name { get; set; }
        public List<Topic> topics {get;set;}
        
        public Category(String name)
        {
            this.name = name;
            topics = new List<Topic>();
        }

        public String ToString()
        {
            String output = name + "\r\n";
            foreach (var topic in topics)
            {
                output = output + topic.topic + " ==> " + topic.verse_ref + "\r\n";
            }
            return output;
        }
    }
}
