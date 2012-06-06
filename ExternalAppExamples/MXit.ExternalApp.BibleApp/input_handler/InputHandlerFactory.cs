using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class InputHandlerFactory
    {

        public static IInputHandler getInputHandler(string curr_user_page)
        {
            //we get the input handler of the current page 
            MenuManager mm = MenuManager.getInstance();
            MenuPage mp;
            if (MenuDefinition.UNDEFINED_MENU_ID.Equals(curr_user_page))
                mp =  (MenuPage)mm.menu_def.getMenuPage(MenuDefinition.ROOT_MENU_ID);//return root menu handler 
            else
                mp = (MenuPage)mm.menu_def.getMenuPage(curr_user_page);
            string ih = mp.input_handler;
            //then use reflection to create an instance of it.
            // the string name must be fully qualified for GetType to work
            string objName = ih;

            IInputHandler obj = (IInputHandler)Activator.CreateInstance(Type.GetType(objName));
            return obj;
        }
    }
}
