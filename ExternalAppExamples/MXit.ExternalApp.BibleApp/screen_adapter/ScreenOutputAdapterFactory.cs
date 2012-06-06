using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class ScreenOutputAdapterFactory
    {
        public static IScreenOutputAdapter getScreenOutputAdapter(string page_id)
        {
            /*if (mp.GetType().FullName.Equals("MxitTestApp.OptionMenuPage"))//TODO: Should be constant
            {
                return new OptionScreenOutputAdapter();
            }
            else
            {
                return null;
            }*/
            //we get the screen adapter of the current page 
            MenuManager mm = MenuManager.getInstance();
            MenuPage mp;
            if (MenuDefinition.UNDEFINED_MENU_ID.Equals(page_id))
                mp = (MenuPage)mm.menu_def.getMenuPage(MenuDefinition.ROOT_MENU_ID);//return root menu handler 
            else
                mp = (MenuPage)mm.menu_def.getMenuPage(page_id);
            string ih = mp.screen_adapter;
            //then use reflection to create an instance of it.
            // the string name must be fully qualified for GetType to work
            string objName = ih;

            IScreenOutputAdapter obj = (IScreenOutputAdapter)Activator.CreateInstance(Type.GetType(objName));
            return obj;
        }
    }
}
