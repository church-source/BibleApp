/*
Software Copyright Notice

Copyright © 2004-2010 MXit Lifestyle Development Company (Pty) Ltd.
All rights are reserved

Copyright exists in this computer program and it is protected by
copyright law and by international treaties. The unauthorised use,
reproduction or distribution of this computer program constitute
acts of copyright infringement and may result in civil and criminal
penalties. Any infringement will be prosecuted to the maximum extent
possible.

MXit Lifestyle Development Company (Pty) Ltd chooses the following
address for delivery of all legal proceedings and notices:
  Riesling House,
  Brandwacht Office Park,
  Trumali Road,
  Stellenbosch,
  7600,
  South Africa.

The following computer programs, or portions thereof, are used in
this computer program under licenses obtained from third parties.
You are obliged to familiarise yourself with the contents of those
licenses.

Third-party software used:
*/

namespace MXit.ExternalApp
{
    /// <summary>
    /// Possible ExternalApp service states.
    /// </summary>
    public enum ExternalAppServiceStatus
    {
        /// <summary>
        /// The service is currently stopped.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// The service is busy starting up.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// The service is currently running.
        /// </summary>
        Running = 2,

        /// <summary>
        /// The service is paused and not processing any requests.
        /// </summary>
        Paused = 3,

        /// <summary>
        /// The service is in the process of stopping.
        /// </summary>
        Stopping = 4,

        /// <summary>
        /// The service is in the process of re-connecting to the ExternalAppAPI.
        /// </summary>
        Reconnecting = 5,

        /// <summary>
        /// The service is in an error state.
        /// </summary>
        Error = -1,

        /// <summary>
        /// The service's connection to the ExternalAppAPI has been lost.
        /// </summary>
        ExternalAppApiConnectionLost = -2
    }
}
