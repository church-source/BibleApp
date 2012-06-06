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
    interface IScreenOutputAdapter
    {
        string getOutputScreen(MenuPage mp);
        MessageToSend getOutputScreenMessage(UserSession us, MenuPage mp, MessageToSend ms, InputHandlerResult ihr);
    }
}
