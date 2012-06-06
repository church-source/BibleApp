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
*/
using System;
using System.ServiceModel;

using MXit.Billing;
using MXit.Messaging;

using MXit.ExternalApp.ExternalAppAPI;


namespace MXit.ExternalApp
{
    /// <summary>
    /// A logic engine that is used by an <see cref="ExternalAppService{UserSessionType, LogicEngineType}"/> to process
    /// requests from MXit users.
    /// </summary>
    /// <typeparam name="UserSessionType">The class your ExternalApp uses to store session information for a user.</typeparam>
    public abstract class ExternalAppLogicEngine<UserSessionType>
        where UserSessionType : class, new()
    {
        #region Variables & Properties

        /// <summary>
        /// The ExternalApp service.
        /// </summary>
        public ExternalAppServiceBase<UserSessionType> Service { get; internal set; }

        /// <summary>
        /// The WCF communication interface to talk to the ExternalAppAPI.
        /// </summary>
        protected CommsClient ExternalAppApiComms
        {
            get
            {
                // Do some checks to make sure the ExternalAppAPI comms is available
                string commsUnavailableReason = null;
                try
                {
                    // API comms is not in the "opened" state
                    if (Service.ExternalAppApiComms.State != CommunicationState.Opened) commsUnavailableReason = "Your ExternalApp's connection is not in the " + CommunicationState.Opened + " state.\nExternalAppApiComms.State = " + Service.ExternalAppApiComms.State;
                }
                catch (Exception e)
                {
                    // Some arbitrary error occurred
                    commsUnavailableReason = "An error occurred while checking the ExternalAppAPI connection.\nException = " + e;
                }

                // If the ExternalAppAPI comms is not available
                if (commsUnavailableReason != null)
                {
                    throw new InvalidOperationException("ExternalAppAPI comms is not available: " + commsUnavailableReason);
                }

                // Return the ExternalAppAPI comms object
                return Service.ExternalAppApiComms;
            }
        }

        #endregion



        #region Event Handlers

        /// <summary>
        /// An event handler that will be invoked whenever a message is received from a MXit user.
        /// </summary>
        /// <param name="messageReceived">The message that was received from the MXit user.</param>
        /// <param name="userSession">The user's session object.</param>
        public abstract void OnMessageReceived(MessageReceived messageReceived, UserSessionType userSession);

        /// <summary>
        /// An event handler that will be invoked whenever a file is received from a MXit user.<br />
        /// <br />
        /// The default implementation ignores the file and pretends nothing happened.
        /// </summary>
        /// <param name="fileReceived">The file that was received from the MXit user.</param>
        /// <param name="userSession">The user's session object.</param>
        public virtual void OnFileReceived(FileReceived fileReceived, UserSessionType userSession)
        {
        }

        /// <summary>
        /// An event handler that will be invoked whenever a payment response is received from a MXit user.<br />
        /// <br />
        /// The default implementation ignores the payment response and pretends nothing happened.
        /// </summary>
        /// <param name="paymentResponse">The payment response that was received from the ExternalAppAPI.</param>
        /// <param name="userSession">The user's session object.</param>
        public virtual void OnPaymentResponseReceived(PaymentResponse paymentResponse, UserSessionType userSession)
        {
        }

        /// <summary>
        /// An event handler that will be invoked when the ExternalApp is started.<br />
        /// <br />
        /// The event handler will be invoked after a connection is established with the ExternalAppAPI, but before
        /// the service will start to process requests.<br />
        /// <br />
        /// You can override this function to perform start-up tasks, e.g. initializing your ExternalApp and
        /// registering image strips.
        /// </summary>
        public virtual void OnStart()
        {
        }

        /// <summary>
        /// An event handler that will be invoked when the ExternalApp is stopped.<br />
        /// <br />
        /// The event handler will be invoked after the connection to the ExternalAppAPI is terminated.
        /// </summary>
        public virtual void OnStop()
        {
        }

        /// <summary>
        /// An event handler that will be invoked when a lost connection to the ExternalAppAPI is re-connected.<br />
        /// <br />
        /// The event handler will be invoked after the connection is re-established with the ExternalAppAPI, but before
        /// the service will start to process requests.<br />
        /// <br />
        /// You can override this function to perform re-initialization tasks, e.g. registering image strips.
        /// </summary>
        public virtual void OnReconnect()
        {
        }

        #endregion
    }
}
