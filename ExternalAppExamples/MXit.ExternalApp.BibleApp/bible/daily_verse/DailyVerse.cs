using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class DailyVerse
    {
        public long id { get; set; }
        public DateTime datetime { get; set; }
        public String verse_ref { get; set; }
        public String verse_text { get; set; }
        public Boolean is_sent { get; set; }
        public DateTime sent_datetime { get; set; }

        public DailyVerse(
            long id, 
            DateTime datetime,
            String verse_ref,
            String verse_text)
        {
            this.id = id;
            this.datetime = datetime;
            this.verse_ref = verse_ref;
            this.verse_text = verse_text;
            this.sent_datetime = DateTime.MinValue;
        }

        public override String ToString()
        {
            return verse_ref + ": " + verse_text;
        }
    }
}
