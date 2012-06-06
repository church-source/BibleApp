using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class SearchVerseRecord : VerseRecord
    {
        public int searh_rank { get; private set; }

        public SearchVerseRecord(
            String start_verse,
            String end_verse,
            int searh_rank
            ) : base(start_verse, end_verse)
        {
            this.searh_rank = searh_rank;
        }

    }
}
