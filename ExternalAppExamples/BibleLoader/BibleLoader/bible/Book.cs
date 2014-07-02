using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BibleLoader
{
    public class Book
    {
        public int id { get; private set; }
        public string name { get; private set; }
        public string abbr { get; private set; } //change this to list in future. 
        public Testament testament { get; private set; }
        public Hashtable chapters { get; private set; }
        
        public Book prev_book { get; set; }
        public Book next_book { get; set; }


        public Book(
            int id,
            string name,
            string abbr,
            ref Testament testament)
        {
            this.id = id;
            this.name = name;
            this.testament = testament;
            this.abbr = abbr;
            chapters = new Hashtable();
        }

        public void addChapter(ref Chapter chapter)
        {
            chapters.Add(chapter.chapter_id, chapter);
        }

        public Chapter getChapter(int chapter_id)
        {
            return (Chapter)chapters[chapter_id];
        }

    }
}
