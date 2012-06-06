using System;
using System.Collections;
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
    class UserSessionManager
    {
        private Hashtable active_user_sessions;

        public UserSessionManager()
        {
            active_user_sessions = new Hashtable();
        }

        /*private UserSession getUserSession(string user_id)
        {
            lock(this.active_user_sessions)
            {
                if (active_user_sessions.ContainsKey(user_id))
                {
                    return (UserSession)active_user_sessions[user_id];
                }
                else
                {
                    UserSession us = new UserSession(user_id);
                    active_user_sessions.Add(user_id, us);
                    return us;
                }
            }

        }*/

        /*private Boolean closeUserSession(string user_id)
        {
            lock (this.active_user_sessions)
            {
                if (active_user_sessions.ContainsKey(user_id))
                {
                    active_user_sessions.Remove(user_id);
                    return true;
                }
                return false;
            }
        }
        */
        /*convert the string messages to Message objects*/
        /*public string handle_message(String user_id, String message)
        {
            UserSession us = getUserSession(user_id);
            IInputHandler i_handler = InputHandlerFactory.getInputHandler(us.current_menu_loc);

            //InputHandlerResult new_loc = i_handler.handleInput(us, message);
           // us.handleAction(new_loc);
           // string output = MenuManager.getInstance().getScreen(new_loc);
           // return output;
        }*/

        /*convert the string messages to Message objects*/
        /*public IMessageToSend handle_message(MessageReceived message_received)
        {
            string user_id = message_received.From;
            UserSession us = getUserSession(user_id);

            string message = message_received.Body.ToString();
           
            IMessageToSend output = us.handleInput(message_received);
            return output;
        }*/

        /*public IMessageToSend handle_error(MessageReceived message_received)
        {
            string user_id = message_received.From;
            UserSession us = getUserSession(user_id);

            IMessageToSend output = us.handleError(message_received);
            return output;
        }

        public IMessageToSend handle_presence(Presence presence)
        {
            string user_id = presence.UserId;
            if (!presence.IsOnline)
            {
                closeUserSession(user_id);
                return null;
            }
            return null;
        }*/
    }
}
