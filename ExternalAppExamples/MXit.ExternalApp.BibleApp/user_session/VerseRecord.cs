using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseRecord
    {
        
        public String start_verse { get; private set; }
        public String end_verse { get; private set; }

        public VerseRecord(
            String start_verse,
            String end_verse
            )
        {
            this.start_verse = start_verse;
            this.end_verse = end_verse;
        }

    }
}
