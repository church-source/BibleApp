using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BibleLoader
{
    public abstract class ABible
    {
        public abstract void loadBible();
        public abstract Testament getTestament(string t_name);
        public abstract Testament getTestament(int index);

       /* public virtual void parseAndAppendBibleText(
            List<Verse> list,
            MessageToSend ms)
        {
            string tmp = "";
            //int verse_end_id = 0;
            int current_chapter = -1;
            if (list.Count > 0)
                current_chapter = list[0].chapter.chapter_id;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    if (list[i].chapter.chapter_id != current_chapter)
                    {
                        ms.Append("\r\n" + list[i].book.name + " " + list[i].chapter.chapter_id + "\r\n", TextMarkup.Bold);
                    }
                    ms.Append(" (" + list[i].verse_id + ") ", TextMarkup.Bold);
                    ms.Append(list[i].text);
                    //   verse_end_id = list[i].verse_id;
                }
                current_chapter = list[i].chapter.chapter_id;
            }
            // section += verse_end_id.ToString();

            ms.Append(tmp + "\r\n");
        }*/
    }
}
