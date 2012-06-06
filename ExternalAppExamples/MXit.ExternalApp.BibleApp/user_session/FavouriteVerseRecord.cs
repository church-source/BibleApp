using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class FavouriteVerseRecord : VerseRecord
    {
        public long id { get; private set; }
        public long user_id { get; private set; }
        public long session_id { get; private set; }
        public DateTime datetime { get; private set; }

        public FavouriteVerseRecord(
            long id,
            long user_id,
            long session_id,
            DateTime datetime,
            String start_verse,
            String end_verse
            ) : base(start_verse, end_verse)
        {
            this.id = id;
            this.user_id = user_id;
            this.session_id = session_id;
            this.datetime = datetime;
        }

        /*test if the given verse is equal to this verse record*/
        public Boolean isEqual(FavouriteVerseRecord fvr_2)
        {
            if (fvr_2 != null)
            {
                String start_verse_2 = fvr_2.start_verse;
                String end_verse_2 = fvr_2.end_verse;
                if (start_verse == start_verse_2)
                {
                    if (end_verse == end_verse_2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*test if the given verse is equal to this verse record*/
        public Boolean isEqual(String start_verse_2, String end_verse_2)
        {
            if (start_verse == start_verse_2)
            {
                if (end_verse == end_verse_2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
