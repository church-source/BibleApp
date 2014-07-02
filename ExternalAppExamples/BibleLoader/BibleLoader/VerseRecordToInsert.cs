using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BibleLoader
{
    public class VerseRecordToInsert
    {
        public int translation_id { get; set; }
        public  String book_name { get; set; }
        public  int chapter_id { get; set; }
        public int verse_id { get; set; }
        public String verse_text { get; set; }

        public VerseRecordToInsert(
            int translation_id,
            String book_name,
            int chapter_id,
            int verse_id,
            String verse_text)
        {
            this.translation_id = translation_id;
            this.book_name = book_name;
            this.chapter_id = chapter_id;
            this.verse_id = verse_id;
            this.verse_text = verse_text;

        }
    }
}
