using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class VerseMenuPage : OptionMenuPage
    {
        public MenuInputItem input_item { get; private set; }
        public VerseMenuPage(
            string menu_id, 
            string title, 
            string message, 
            string screen_adapter, 
            string input_handler,
            string help_page_id,
            List<MenuOptionItem> options, 
            MenuInputItem input_item)
            : base(
                menu_id, 
                title, 
                message, 
                screen_adapter, 
                input_handler, 
                help_page_id, 
                options)
        {
            this.input_item = input_item;
        }

        public override string ToString()
        {
            string options_string = "Options \r\n";
            foreach(var option in options){
                options_string+= option.ToString() +"\r\n";
            }
            return base.ToString() + "\r\n" + options_string;

        }
    }
}
