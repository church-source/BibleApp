using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    abstract class AMenuDynamicOptionSet
    {
        //public abstract AMenuDynamicOptionSet(String target_page);
        private String extra_commands;
        private String list_empty_message; 
        public abstract void init();
        public abstract List<MenuOptionItem> getOptionList(UserSession us);
        public abstract string parseInput(String input, UserSession us);
        public virtual InputHandlerResult handleExtraCommandInput(UserSession us, String input)
        {
            return new InputHandlerResult(
                   InputHandlerResult.UNDEFINED_MENU_ACTION,
                   InputHandlerResult.DEFAULT_MENU_ID,
                   InputHandlerResult.DEFAULT_PAGE_ID);
        }

        public string getExtraCommandString()
        {
            return extra_commands;
        }

        public void setExtraCommandString(String extra_commands)
        {
            this.extra_commands = extra_commands;
        }

        public string getListEmptyMessage()
        {
            return list_empty_message;
        }

        public void setListEmptyMessage(String message)
        {
            this.list_empty_message= message;
        }
    }
}
