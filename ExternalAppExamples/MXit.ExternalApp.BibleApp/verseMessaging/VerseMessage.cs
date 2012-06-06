using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseMessage
    {
        public long message_id { get; set; }
        public long thread_id { get; set; }
        public DateTime datetime_sent { get; set; }
        public String message_text { get; set; }
        public long sender_id { get; set; } //we dont have a to recipient ID because we use a corresponding participant list to find that out (so that we can send more than one person etc). 

        public VerseMessage(
            long message_id,
            long thread_id,
            DateTime datetime_sent,
            String message_text,
            long sender_id)
        {
            this.message_id = message_id;
            this.thread_id = thread_id;
            this.datetime_sent = datetime_sent;
            this.message_text = message_text;
            this.sender_id = sender_id;
        }
    }
}
