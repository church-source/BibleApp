using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{


    public class TaggedVerseMenuOptionItem : MenuOptionItem
    {
        public VerseTag verse_tag { get; private set; }
        public TaggedVerseMenuOptionItem(
            string menu_option_id,
            string link_val,
            string select_action,
            string display_text,
            VerseTag verse_tag)
            : base(menu_option_id, link_val, select_action, display_text)
        {
            this.verse_tag = verse_tag;
        }

        public override string ToString()
        {
            return "ID: " + menu_option_id + "\r\nAction: " + select_action + " \r\nDisplay Text: " + display_text;
        }
    }
}
