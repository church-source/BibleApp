using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    //bad design, should override this class for different functions like a conf page, etc. but ok leave it for now.:( 
    public class InputHandlerResult
    {
        public int action { get; set; }
        public string menu_id { get; private set; }
        public int page_id { get; private set; }
        public object error { get; private set; }
        public string message  {get;private set;}

        public InputHandlerResult()
        {
            this.action = DO_NOTHING_ACTION;
            this.menu_id = DEFAULT_MENU_ID;
            this.page_id = DEFAULT_PAGE_ID;
        }

        public InputHandlerResult(
            int action, 
            string menu_id,
            int page_id)
        {
            this.action = action;
            this.menu_id = menu_id;
            this.page_id = page_id;
        }

        public InputHandlerResult(
            int action,
            string menu_id,
            String message)
        {
            this.action = action;
            this.menu_id = menu_id;
            this.message = message;
        }

        public InputHandlerResult(
            int action,
            string menu_id)
        {
            this.action = action;
            this.menu_id = menu_id;
            this.page_id = DEFAULT_PAGE_ID;
        }

        public InputHandlerResult(
            object error)
        {
            this.action = INVALID_MENU_ACTION;
            this.menu_id = DEFAULT_MENU_ID;
            this.page_id = DEFAULT_PAGE_ID;
            this.error = error;
        }

        public const int INVALID_MENU_ACTION = 0xFF;
        public const int UNDEFINED_MENU_ACTION = 0x00;
        public const int NEW_MENU_ACTION = 0x01;
        public const int ROOT_MENU_ACTION = 0x02;
        public const int BACK_MENU_ACTION = 0x03;
        public const int NEXT_PAGE_ACTION = 0x04;
        public const int PREV_PAGE_ACTION = 0x05;
        public const int CONF_PAGE_ACTION = 0x06;
        public const int DO_NOTHING_ACTION = 0x07;
        public const int FAVOURITE_ADDED_ACTION = 0x08;
        public const int BACK_WITHOUT_INIT_MENU_ACTION = 0x09;
        public const int DISPLAY_MESSAGE = 0x10;
        public const int CHANGE_PAGE_ACTION = 0x11;

        public const int DEFAULT_PAGE_ID = 0;
        public const string DEFAULT_MENU_ID = "-1";
    }
}
