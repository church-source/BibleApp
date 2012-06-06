using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class MenuOptionItem
    {
        public string menu_option_id { get; private set; }
        public string select_action { get; private set; }
        public string display_text { get; private set; }
        public string link_val { get; private set; }

        public Boolean is_valid { get; set; }
        public MenuOptionItem(string menu_option_id, string link_val, string select_action, string display_text)
        {
            this.menu_option_id = menu_option_id;
            this.link_val = link_val;
            this.select_action = select_action;
            this.display_text = display_text;

            is_valid = true;
        }

        public override string ToString()
        {
            return "ID: " + menu_option_id + "\r\nAction: " + select_action + " \r\nDisplay Text: " + display_text;
        }
    }
}
