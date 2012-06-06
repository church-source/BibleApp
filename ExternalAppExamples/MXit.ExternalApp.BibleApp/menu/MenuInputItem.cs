using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class MenuInputItem
    {
        public string menu_input_id { get; private set; }
        public string target_page { get; private set; }
        public string display_text { get; private set; }
        public MenuInputItem(string menu_input_id, string target_page, string display_text)
        {
            this.menu_input_id = menu_input_id;
            this.target_page = target_page;
            this.display_text = display_text;
        }

        public override string ToString()
        {
            return "ID: " + menu_input_id + "\r\nAction: " + target_page + " \r\nDisplay Text: " + display_text;
        }

    }
}
