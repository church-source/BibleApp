using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    abstract class MenuPage
    {
        public string menu_id {  get; private set; }
        public string title { get; private set; }
        public string message { get; private set; }
        public string screen_adapter { get; private set; }
        public string input_handler {get; private set; }
        
        public string help_page_id { get; private set; }
        
        private Boolean back_link_enabled = true;
        private Boolean main_link_enabled = true;

        public MenuPage(
            string menu_id, 
            string title, 
            string message, 
            string screen_adapter,
            string input_handler,
            string help_page_id)
        {
            this.menu_id = menu_id;
            this.title = title;
            this.message = message;
            this.screen_adapter = screen_adapter;
            this.input_handler = input_handler;
            this.help_page_id = help_page_id;
        }

        public Boolean hasHelpPage()
        {
            if (help_page_id == "")
                return false;
            else
                return true;
        }

        public Boolean isBackLinkEnabled()
        {
            return back_link_enabled;
        }

        public void setBackLinkEnabled(Boolean b)
        {
            this.back_link_enabled = b;
        }


        public Boolean isMainLinkEnabled()
        {
            return main_link_enabled;
        }

        public void setMainLinkEnabled(Boolean b)
        {
            this.main_link_enabled = b;
        }


        public override string ToString()
        {
            return "ID: " + menu_id + "\r\nTitle: " + title + " \r\nMessage: " + message +" \r\nInputHandler: " + input_handler;
        }

        public abstract List<MenuOptionItem> getOptionList(UserSession us);
    }
}
