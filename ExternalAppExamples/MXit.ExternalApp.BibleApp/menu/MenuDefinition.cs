using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    
    class MenuDefinition
    {
        private Hashtable menu_pages {  get; set; }

        public MenuDefinition(List<MenuPage> menu_pages_to_add)
        {
            this.menu_pages = new Hashtable();
            foreach (MenuPage menu_page in menu_pages_to_add)
            {
                this.menu_pages.Add(menu_page.menu_id, menu_page);
            }
        }

        public MenuPage getMenuPage(string menu_page_id)
        {
            return (MenuPage)menu_pages[menu_page_id];
        }

        public const string UNDEFINED_MENU_ID = "-1";
        public const string ROOT_MENU_ID = "1";
        public const int PAGE_ITEM_COUNT = 8;
    }
}
