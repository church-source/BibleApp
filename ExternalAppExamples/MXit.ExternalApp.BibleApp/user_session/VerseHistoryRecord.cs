using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseHistoryRecord : VerseRecord
    {
        public long id{ get;  set; }
        public long user_id{ get; private set; }
        public long session_id{ get; private set; }
        public DateTime datetime { get; private set; }

        public VerseHistoryRecord(
            long id,
            long user_id,
            long session_id,
            DateTime datetime,
            String start_verse,
            String end_verse
            ) : base(start_verse,end_verse)
        {
            this.id = id;
            this.user_id = user_id;
            this.session_id = session_id;
            this.datetime = datetime;
        }
    }
}
