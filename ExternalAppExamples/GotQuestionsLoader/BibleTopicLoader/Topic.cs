using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GotQuestionsLoader
{
    public class Topic
    {
        public String topic { get; set; }
        public String verse_ref { get; set; }

        public Topic(String topic, String verse_ref)
        {
            this.topic = topic;
            this.verse_ref = verse_ref;
        }
    }
}
