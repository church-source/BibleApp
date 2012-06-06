using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{


    public class MessageThreadMenuOptionItem : MenuOptionItem
    {
        public VerseMessageThread vmt { get; private set; }
        public MessageThreadMenuOptionItem(
            string menu_option_id,
            string link_val,
            string select_action,
            string display_text,
            VerseMessageThread vmt)
            : base(menu_option_id, link_val, select_action, display_text)
        {
            this.vmt = vmt;
        }

        public override string ToString()
        {
            return "ID: " + menu_option_id + "\r\nAction: " + select_action + " \r\nDisplay Text: " + display_text;
        }
    }
}
