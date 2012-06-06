using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class Verse
    {
        public String text { get; private set; }
        public int verse_id { get; private set; }
        public Translation translation { get; set; }
        public Testament testament { get; private set; }
        public Book book { get; private set; }
        public Chapter chapter { get; private set; }
        public Verse prev_verse { get; set; }
        public Verse next_verse { get; set; }
        public Boolean is_last_verse_of_chapter { get; set; }

        public Verse(
            int verse_id,
            string text,
            ref Testament testament,
            ref Book book,
            ref Chapter chapter,
            ref Translation translation
            )
        {
            this.verse_id = verse_id;
            this.text = text;
            this.book = book;
            this.chapter = chapter;
            this.next_verse = null;
            this.prev_verse = null;
            this.translation = translation;
            is_last_verse_of_chapter = false;
        }

        public string getVerseReference()
        {
            return this.book.name + " " + this.chapter.chapter_id + ":" + this.verse_id;
        }

        public Verse getPreviousVerse()
        {

            if (this.prev_verse != null)
            {
                return this.prev_verse;
            }
            else if (this.chapter.prev_chapter != null)
            {
                return this.chapter.prev_chapter.getLastVerseOfChapter();
            }
            else
            {
                return null;
            }

        }
    }
}
