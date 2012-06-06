using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class Topic
    {
        public int topic_id { get; set; }
        public String topic { get; set; }
        public String verse_ref { get; set; }

        public Topic(int id, String topic, String verse_ref)
        {
            this.topic_id = id;
            this.topic = topic;
            this.verse_ref = verse_ref;
        }
    }
}
