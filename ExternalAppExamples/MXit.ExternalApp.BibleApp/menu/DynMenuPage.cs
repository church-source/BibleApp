using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class DynMenuPage : OptionMenuPage
    {
        public AMenuDynamicOptionSet dynamic_set {get; private set;}
        public string output_var { get; private set; }
        public DynMenuPage(
            string menu_id,
            string title,
            string message,
            string screen_adapter,
            string input_handler,
            string help_page_id,
            List<MenuOptionItem> options,
            AMenuDynamicOptionSet dynamic_set,
            string output_var)
            : base(menu_id, title, message, screen_adapter, input_handler, help_page_id, options)
        {
            this.dynamic_set = dynamic_set;
            this.output_var = output_var;
        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            return dynamic_set.getOptionList(us);
        }
    }
}
