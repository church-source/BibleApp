using System;
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
    public class NetBible : Bible
    {

        public NetBible(Translation translation)
            : base(translation)
        {
        }

        public override void parseAndAppendBibleText(
           List<Verse> list,
           MessageToSend ms)
        {
            string tmp = "";
            Boolean is_last_verse_in_section = false;
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
                    

                    //   verse_end_id = list[i].verse_id;
                    is_last_verse_in_section = i == (list.Count - 1);
                    if (i == 0)
                    {
                        appendText(ms, list[i], true, is_last_verse_in_section);
                    }
                    else
                    {
                        
                        appendText(ms, list[i], false, is_last_verse_in_section);
                    }
                }
                current_chapter = list[i].chapter.chapter_id;
            }
            // section += verse_end_id.ToString();

            ms.Append(tmp + "\r\n");
        }

        protected void appendText(
            MessageToSend ms,
            Verse verse,
            Boolean is_first_verse,
            Boolean is_last_verse)
        {
            //String text = (verse.text).Replace("<lb>", "");
            parseText(ms, verse, is_first_verse, is_last_verse);
           // ms.Append(text, ms);
        }


        /*protected String parseSCTags(
            String text)
        {
            int b_index = text.IndexOf("scstart-");
            String sc_text;
            string message_text = "";
            if (b_index != -1)
            {
                int e_index = text.IndexOf("-scend");
                string variable_name = text.Substring(b_index + 1, e_index - b_index - 1);
                sc_text = text.Substring(start_title_index, end_title_index - start_title_index );
                message_text = parseMessage(us, message.Replace('[' + variable_name + ']', us.getVariable(variable_name)));
               
            }
        }*/

        /*replace info tags and div tags*/
        protected void parseText(
            MessageToSend ms,
            Verse verse,
            Boolean is_first_verse,
            Boolean is_last_verse)
        {

            String tag_contents = "";
            int start_title_index = -1;
            int end_title_index = -1;
            String paragraph_title = "";
            Boolean start_of_verse = true;

            String  message = parseSCTags(verse.text);
            if (is_first_verse)
            {
                Verse prev_verse = verse.getPreviousVerse();
                if (prev_verse != null)
                {
                    String initial_title = getParagaphTitleFromVerse(prev_verse);
                    if (initial_title != null)
                    {
                        ms.Append("\r\n");
                        ms.Append(initial_title, new TextMarkup[] { TextMarkup.Bold });
                        ms.Append("\r\n");
                    }
                }
            }
            int b_index = message.IndexOf('<');
            if (b_index == -1)
            {
                ms.Append(message);
                return;
            }
            while (b_index != -1)
            {
                int e_index = message.IndexOf('>');
                if (e_index == -1)
                {
                    Console.WriteLine("NO CLOSING TAG FOUND FOR A VERSE: " + message);
                    return;
                }
                tag_contents = message.Substring(b_index + 1, e_index - b_index - 1);
                if (tag_contents.Contains(PARAGRAPH_TITLE_TAG))
                {
                    end_title_index = message.IndexOf("<lb>", e_index);
                    start_title_index = message.IndexOf("<head>") + 6; // move past head tag also
                    paragraph_title = message.Substring(start_title_index, end_title_index - start_title_index /*- 1*/);
                    if (!is_last_verse && paragraph_title != null && !paragraph_title.Trim().Equals(""))
                    {
                        ms.Append("\r\n");
                        ms.Append(paragraph_title.Trim(), new TextMarkup[] { TextMarkup.Bold });
                        ms.Append("\r\n");
                        ms.Append("\r\n");
                    }
                    message = message.Remove(b_index, end_title_index - b_index + 4);
                }
                else if (tag_contents.Contains(PARAGRAPH_TAG))
                {
                    if (start_of_verse)
                    {
                        ms.Append(" (" + verse.verse_id + ") ", TextMarkup.Bold);
                        start_of_verse = false;
                    }
                    ms.Append(removeTags(message.Substring(0, b_index)));
                    if(!(verse.chapter.chapter_id == 1 && verse.verse_id == 1))
                    {
                        ms.Append("\r\n");
                        ms.Append("\r\n");
                    }
                    message = message.Remove(0, e_index+1);
                }
               /* else if (tag_contents.Equals(ITALIC_TAG))
                {
                    if (start_of_verse)
                    {
                        ms.Append(" (" + verse.verse_id + ") ", TextMarkup.Bold);
                        start_of_verse = false;
                    }
                    ms.Append(message.Substring(0, b_index));
                    message = message.Remove(0, e_index + 1); //remove opening tag and previous text
                    int e_italic_index = message.IndexOf("<" + END_ITALIC_TAG + ">");
                    ms.Append(message.Substring(0, e_italic_index), TextMarkup.Italics);
                    message = message.Remove(0, e_italic_index+ 4); //remove closing tag and previous text
                }
                else if (tag_contents.Equals(BOLD_TAG))
                {
                    if (start_of_verse)
                    {
                        ms.Append(" (" + verse.verse_id + ") ", TextMarkup.Bold);
                        start_of_verse = false;
                    }
                    ms.Append(message.Substring(0, b_index));
                    message = message.Remove(0, e_index + 1); //remove opening tag and previous text
                    int e_italic_index = message.IndexOf("<" + END_BOLD_TAG + ">");
                    ms.Append(message.Substring(0, e_italic_index), TextMarkup.Bold);
                    message = message.Remove(0, e_italic_index + 4); //remove closing tag and previous text
                }*/
                else
                {
                    message = message.Remove(b_index, e_index - b_index+1);
                }
                b_index = message.IndexOf('<');
            }
            if(start_of_verse)
                ms.Append(" (" + verse.verse_id + ") ", TextMarkup.Bold);

            ms.Append(parseSCTags(removeTags(message)));

        }

        public String removeTags(String text)
        {
            int b_index = text.IndexOf('<');
            while (b_index != -1)
            {
                int e_index = text.IndexOf('>');
                if (e_index == -1)
                {
                    Console.WriteLine("NO CLOSING TAG FOUND FOR A VERSE: " + text);
                    return text;
                }
                else
                {
                    text = text.Remove(b_index, e_index - b_index+1);
                }
                b_index = text.IndexOf('<');
            }
            return text;
        }

        public String parseSCTags(String text)
        {
            int sc_start_index = -1;
            int sc_end_index = -1;
            String text_to_upper = "";
            String message = (String)text.Clone();
            sc_start_index = message.IndexOf(SC_START_TAG);
            int start_text_index = -1;
            while (sc_start_index != -1)
            {
                sc_end_index = message.IndexOf(SC_END_TAG);
                if (sc_end_index == -1)
                {
                    Console.WriteLine("NO CLOSING SC TAG FOUND IN A VERSE: " + message);
                    return message;
                }
                start_text_index = sc_start_index + SC_START_TAG.Length;
                text_to_upper = message.Substring(start_text_index, sc_end_index - start_text_index);
                message = message.Remove(sc_start_index, (sc_end_index - sc_start_index) + SC_END_TAG.Length);
                message = message.Insert(sc_start_index, text_to_upper.ToUpper());

                sc_start_index = message.IndexOf(SC_START_TAG);
            }
            return message;
        }

        public String getParagaphTitleFromVerse(Verse verse)
        {
            String message = verse.text;
            int b_index = message.IndexOf('<');
            String tag_contents = "";
            int start_title_index = -1;
            int end_title_index = -1;
            String paragraph_title = null;

            if (b_index == -1)
            {
                return null;
            }
            while (b_index != -1)
            {
                int e_index = message.IndexOf('>');
                if (e_index == -1)
                {
                    Console.WriteLine("NO CLOSING TAG FOUND FOR A VERSE: " + message);
                    return paragraph_title;
                }
                tag_contents = message.Substring(b_index + 1, e_index - b_index - 1);
                if (tag_contents.Contains(PARAGRAPH_TITLE_TAG))
                {
                    end_title_index = message.IndexOf("<lb>", e_index);
                    start_title_index = message.IndexOf("<head>") + 6; // move past head tag also
                    paragraph_title = message.Substring(start_title_index, end_title_index - start_title_index /*- 1*/);
                    message = message.Remove(b_index, end_title_index - b_index + 4);
                }
                else
                {
                    message = message.Remove(b_index, e_index - b_index + 1);
                }
                b_index = message.IndexOf('<');
            }
            return paragraph_title;
        }

        public const String ITALIC_TAG = "i";
        public const String END_ITALIC_TAG = "/i";
        public const String BOLD_TAG = "b";
        public const String END_BOLD_TAG = "/b";
        public const String PARAGRAPH_TAG = "/p";
        public const String PARAGRAPH_TITLE_TAG = "paragraphtitle";
        public const String SC_START_TAG = "scstart-";
        public const String SC_END_TAG = "-scend";
    }
}

