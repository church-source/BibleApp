using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseMessageParticipant
    {
        public long participant_row_id { get; set; }
        public long thread_id { get; set; }
        public long user_id { get; set; }
        public DateTime datetime_joined { get; set; }
        public DateTime datetime_last_read { get; set; } 

        public VerseMessageParticipant(
            long participant_id,
            long thread_id,
            long user_id,
            DateTime datetime_joined,
            DateTime datetime_last_read)
        {
            this.participant_row_id = participant_row_id;
            this.thread_id = thread_id;
            this.user_id = user_id;
            this.datetime_joined = datetime_joined;
            this.datetime_last_read = datetime_last_read;
        }

        public void updateDateTimeLastRead(DateTime datetime)
        {
            this.datetime_last_read = datetime;
        }
    }
}
