using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{


    public class VerseMenuOptionItem : MenuOptionItem
    {
        public VerseRecord fvr { get; private set; }
        public VerseMenuOptionItem(
            string menu_option_id,
            string link_val,
            string select_action,
            string display_text,
            VerseRecord fvr)
            : base(menu_option_id, link_val, select_action, display_text)
        {
            this.fvr = fvr;
        }

        public override string ToString()
        {
            return "ID: " + menu_option_id + "\r\nAction: " + select_action + " \r\nDisplay Text: " + display_text;
        }
    }
}
