using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace MxitTestApp
{
    public class Category
    {
        public int category_id { get; set; }
        public string name { get; set; }
        public List<Topic> topics {get;set;}
        
        public Category(int id, String name)
        {
            this.category_id = id;
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
