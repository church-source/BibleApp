using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;

namespace MxitTestApp
{
    public abstract class ABible
    {
        public abstract void loadBible();
        public abstract Testament getTestament(string t_name);
        public abstract Testament getTestament(int index);

        public virtual void parseAndAppendBibleText(
            List<Verse> list,
            MessageToSend ms,
            UserColourTheme uct)
        {
            Color color = Color.Empty;
            if (uct != null)
                color = uct.getBibleTextColour();
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
                        if(uct!=null)
                            ms.Append("\r\n" + list[i].book.name + " " + list[i].chapter.chapter_id + "\r\n", color, TextMarkup.Bold);
                        else
                            ms.Append("\r\n" + list[i].book.name + " " + list[i].chapter.chapter_id + "\r\n", TextMarkup.Bold);
                    }
                    if (uct != null)
                    {
                        ms.Append(" (" + list[i].verse_id + ") ", color, TextMarkup.Bold);
                        ms.Append(list[i].text, color);
                    }
                    else
                    {
                        ms.Append(" (" + list[i].verse_id + ") ", TextMarkup.Bold);
                        ms.Append(list[i].text, color);
                    }
                    //   verse_end_id = list[i].verse_id;
                }
                current_chapter = list[i].chapter.chapter_id;
            }
            // section += verse_end_id.ToString();

            ms.Append(tmp + "\r\n");
        }
    }
}
