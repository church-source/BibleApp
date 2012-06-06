using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class SuspendedRecord
    {
        public long user_id { get; private set; }
        public DateTime datetime_end {get; private set;}

        public SuspendedRecord(
            long user_id,
            DateTime datetime_end)
        {
            this.user_id = user_id;
            this.datetime_end = datetime_end;
        }
    }
}
