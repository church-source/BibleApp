using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MXit.ExternalApp;
using System.ServiceModel;

using MXit.Billing;
using MXit.Messaging;

using MXit.ExternalApp.ExternalAppAPI;
using MXit.ExternalApp.Examples.Redirect;
namespace MxitTestApp
{
    class BatchJobData
    {
        public BibleAppEngine engine { get; set; }
        public List<MessageToSend> messages { get; set; }
    }
}
