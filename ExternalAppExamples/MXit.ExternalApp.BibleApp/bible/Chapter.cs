using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class Chapter
    {

        public int chapter_id { get; private set; }
        public Testament testament { get; private set; }
        //this should be a pointer to the book the chapter belongs to. 
        public Book book {get; private set;}
        //we should probably make this class immutable, but lets leave it for now
        public Hashtable verses { get; private set; }
        public Verse last_verse { get; private set; }
        public Chapter prev_chapter { get; set; }
        public Chapter next_chapter { get; set; }

        private int num_verses=0; //we set this once so we dont have to calculate it everytime
        
        public Chapter(
            int chapter_id,
            ref Testament testament,
            ref Book book)
        {
            this.chapter_id = chapter_id;
            this.testament = testament;
            this.book = book;
            verses = new Hashtable();
        }

        public void addVerse(ref Verse verse)
        {
            if (verse.is_last_verse_of_chapter)
                last_verse = verse;
            verses.Add(verse.verse_id, verse);
            num_verses++;
        }

        public Verse getVerse(int verse_id)
        {
            return (Verse)verses[verse_id];
        }

        public int getNumVersesInChapter()
        {
            return num_verses;
        }

        public Verse getLastVerseOfChapter()
        {
            return last_verse;
        }
    }
}
