using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;
namespace MxitTestApp
{
    class MenuManager
    {
        private static MenuManager instance;
        public MenuDefinition menu_def {get; private set;}

        static MenuManager()
        {
            instance = new MenuManager();
            getInstance();
        }

        public MenuManager()
        {
            XMLMenuHandler xml_menu = new XMLMenuHandler(MENU_DEF_FILE_NAME);
            MenuDefinition md = new MenuDefinition(xml_menu.getMenuPages());
            this.menu_def = md;
        }

        public static MenuManager getInstance()
        {
            if (instance != null)
                return instance;
            else
            {
                instance = new MenuManager();
                return instance;
            }
        }

        public string getScreen(string id)
        {
            return ScreenOutputAdapterFactory.getScreenOutputAdapter(id).getOutputScreen(menu_def.getMenuPage(id));
        }

        public MessageToSend getScreenMessage(
            UserSession us,
            MessageToSend ms,
            InputHandlerResult ihr)
        {
            string page_id = us.current_menu_loc;
            return ScreenOutputAdapterFactory.getScreenOutputAdapter(page_id).getOutputScreenMessage(
                us, 
                menu_def.getMenuPage(page_id),
                ms,
                ihr);
        }
        public static string MENU_DEF_FILE_NAME = "MenuDefinition.xml"; //should get this from setting rather
    }
}
