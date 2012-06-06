using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class TopFavouriteVerseRecord : VerseRecord
    {
        public long verse_count { get; private set; }

        public TopFavouriteVerseRecord(
            String start_verse,
            String end_verse,
            long verse_count
            ) : base(start_verse, end_verse)
        {
            this.verse_count = verse_count;
        }

    }
}
