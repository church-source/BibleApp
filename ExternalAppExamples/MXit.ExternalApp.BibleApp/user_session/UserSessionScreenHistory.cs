using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class UserSessionScreenHistory
    {
        private Stack<String> screen_history;
        public UserSessionScreenHistory()
        {
            screen_history = new Stack<String>();
            //always push the root menu on first.
            //screen_history.Push(MenuDefinition.ROOT_MENU_ID);
        }

        public String getPreviousScreenID()
        {
            if (screen_history.Peek() != null)
                return screen_history.Pop();

            return null;
        }

        public String peekPreviousScreenID()
        {
            if(screen_history.Count > 0)
                return screen_history.Peek();

            return null;
        }

        public void clear_history()
        {
            screen_history.Clear();
        }


        public void addPreviousScreenID(String screen_id)
        {
            //only add if not already previous screen id

            string existint_prev = "";
            if (screen_history.Count > 0)
                existint_prev = screen_history.Peek();
            if (!(existint_prev).Equals(screen_id))
                screen_history.Push(screen_id);
        }

        public string trailToString()
        {
            string output = "";
            for (int i = 0; i < screen_history.Count; i++)
            {
                output = output + screen_history.ElementAt(screen_history.Count - 1 - i) + "\r\n";
            }
            return output;
        }

    }
}
