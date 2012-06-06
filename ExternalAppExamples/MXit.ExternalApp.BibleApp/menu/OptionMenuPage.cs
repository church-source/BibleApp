using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class OptionMenuPage : MenuPage
    {
        public List<MenuOptionItem> options { get; private set; }
        public OptionMenuPage(
            string menu_id, 
            string title, 
            string message, 
            string screen_adapter, 
            string input_handler, 
            string help_page_id, 
            List<MenuOptionItem> options)
                : base(menu_id, title, message,screen_adapter, input_handler, help_page_id)
        {
            this.options = options;
        }

        public override string ToString()
        {
            string options_string = "Options \r\n";
            foreach(var option in options){
                options_string+= option.ToString() +"\r\n";
            }
            return base.ToString() + "\r\n" + options_string;

        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            return options;
        }
    }
}
