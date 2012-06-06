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
    interface IInputHandler
    {
        /*
          the framework isolates each menu interaction with an input handler and screen generator/adaptor. 
          the handling should be entirely in the handler, but this is broken on the first entry, because
          the previous screen handler results in this screen being displayed.
        */
        void init(UserSession user_session);
        /*
         the handleInput is the method that needs to be implemented to handle the input and return an
         InputHandlerResult. Depending on the action of this result, it might get sent to the output
         adapter which would need to generate a screen based on the result
        */
        InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved);
    }
}
