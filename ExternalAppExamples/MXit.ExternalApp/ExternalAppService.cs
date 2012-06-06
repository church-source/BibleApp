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

/* ----------------------------------------------------------------------------
 * Remove this comment if you are using .NET Framework 4 or higher. Remember
 * to also change the project's target framework.

 * ---------------------------------------------------------------------------- */
#define DotNet4
using System;
using System.Collections;
#if DotNet4
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;

using MXit.Billing;
using MXit.Log;
using MXit.Messaging;
using MXit.User;

using MXit.ExternalApp.ExternalAppAPI;

namespace MXit.ExternalApp
{
    /// <summary>
    /// A generic ExternalApp service.<br/>
    /// 	<br/>
    /// This class handles all the technical bits for an ExternalApp (connect, disconnect, user session management, etc.).<br/>
    /// 	<br/>
    /// The business logic for the ExternalApp is handled by the service's <see cref="LogicEngine"/>.
    /// </summary>
    /// <typeparam name="UserSessionType">The class your ExternalApp uses to store session information for a user.</typeparam>
    /// <typeparam name="LogicEngineType">The class that contains your ExternalApp's business logic.</typeparam>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class ExternalAppService<UserSessionType, LogicEngineType> : ExternalAppServiceBase<UserSessionType>, CommsCallback
        where UserSessionType : class, new()
        where LogicEngineType : ExternalAppLogicEngine<UserSessionType>
    {
        #region Variables & Properties

        /// <summary>
        /// The default number worker threads to process requests from an ExternalApp's request queue.
        /// </summary>
        public const int DefaultRequestQueueThreads = 10;

        /// <summary>
        /// The default maximum number of requests allowed in an ExternalApp's request queue.
        /// </summary>
        public const int DefaultRequestQueueMaxCount = 50;

        /// <summary>
        /// An object to lock on to control access to this service's state.
        /// </summary>
        private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// A keep-alive timer to periodically "ping" the ExternalAppAPI to prevent an active WCF
        /// connection from being closed due to a timeout.
        /// </summary>
        private Timer _keepAliveTimer = null;

        /// <summary>
        /// The logic engine this ExternalApp uses to process requests from MXit users.
        /// </summary>
        public LogicEngineType LogicEngine { get; private set; }

        /// <summary>
        /// Timestamp when this service was started.
        /// </summary>
        public DateTime? StartTimestamp { get; private set; }

        /// <summary>
        /// The service's current uptime.
        /// </summary>
        public TimeSpan? Uptime
        {
            get
            {
                // Acquire the lock on the service's state
                _stateLock.EnterReadLock();
                try
                {
                    if (StartTimestamp == null)
                    {
                        return null;
                    }
                    else
                    {
                        return DateTime.Now - StartTimestamp.Value;
                    }
                }
                finally
                {
                    // Release the lock on the service's state
                    _stateLock.ExitReadLock();
                }
            }
        }

        #region Request Queue

        /// <summary>
        /// A queue of requests (messages, files, payment responses, etc.) received from the ExternalAppAPI that are
        /// waiting to be processed.
        /// </summary>
        private readonly Queue<Object> _requestQueue = new Queue<Object>();

        /// <summary>
        /// Lock to synchronize access to <see cref="_requestQueue"/>.
        /// </summary>
        private object _requestQueueLock { get { return ((ICollection)_requestQueue).SyncRoot; } }

        /// <summary>
        /// The number of dedicated threads that are currently busy processing requests from the request queue.
        /// </summary>
        public int RequestQueueThreads { get; private set; }

        /// <summary>
        /// The current number of requests in the request queue.<br />
        /// <br />
        /// This is the number of requests (messages, files, payment responses, etc.) that are waiting to be processed.
        /// </summary>
        public int RequestQueueCount { get { return _requestQueue.Count; } }

        /// <summary>
        /// The maximum number of requests that are allowed in the service's request queue.
        /// </summary>
        public int RequestQueueMaxCount { get; private set; }

        /// <summary>
        /// The number of dedicated threads that to keep alive while the service is running.
        /// </summary>
        private int _requestQueueThreadsRequired = DefaultRequestQueueThreads;

        /// <summary>
        /// Indicates if a request queue is enabled for this service.
        /// </summary>
        public bool RequestQueueIsEnabled
        {
            get { return _requestQueueThreadsRequired > 0; }
        }

        #endregion


        #endregion



        #region ExternalAppAPI Callback Functions

        #region Private Functions

        /// <summary>
        /// Determines whether the service is ready to process a request received from a MXit user.<br/>
        /// 	<br/>
        /// If the service is in maintenance mode, this function will also try to send a nice message to the user.
        /// </summary>
        /// <param name="contactName">The contact name within which the ExternalApp is processing the request.</param>
        /// <param name="userId">The MXit user's UserId.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The function returns <c>true</c> if it is ready to process request; otherwise, <c>false</c>.
        /// </returns>
        private bool IsReadyToProcessRequest(string contactName, string userId, object request)
        {
            // Acquire the lock on the service's state
            _stateLock.EnterReadLock();
            try
            {
                // If the service is running, we can process the request
                if (Status == ExternalAppServiceStatus.Running)
                {
                    return true;
                }

                // If the service is paused
                else if (Status == ExternalAppServiceStatus.Paused)
                {
                    // Log some info
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Service is paused, dropping request.\nStatus = " + Status + "\nRequest = " + request, Level.Info);

                    try
                    {
                        // Create a nice reply message
                        MessageToSend maintenanceMessage = MessageBuilder.CreateMessageToSend(contactName, userId);
                        maintenanceMessage.Append(MessageBuilder.Elements.CreateClearScreen());
                        maintenanceMessage.AppendLine();
                        maintenanceMessage.AppendLine("We're currently performing system maintenance. Please try again in 5 minutes.");

                        // Send the reply
                        ExternalAppApiComms.SendMessage(maintenanceMessage);
                    }
                    catch (Exception e)
                    {
                        // Log some info
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while sending maintenance message.\nException = " + e, Level.Error);
                    }
                }

                // The service is not running
                else
                {
                    // Log some info
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Service is not running, dropping request.\nStatus = " + Status + "\nRequest = " + request, Level.Warning);
                }
            }
            finally
            {
                // Release the lock on the service's state
                _stateLock.ExitReadLock();
            }

            // Nothing left to do
            return false;
        }

        /// <summary>
        /// Start a new worker thread to process messages from the message queue.
        /// </summary>
        private void StartRequestQueueWorkerThread()
        {
            // Increment the worker thread count
            RequestQueueThreads++;

            // Start a new worker thread
            Thread thread = new Thread(new ParameterizedThreadStart(ProcessFromQueue));
            thread.Start(RequestQueueThreads);
        }

        /// <summary>
        /// Continually process messages from the message queue.
        /// </summary>
        /// <param name="threadId">The thread's integer ID.</param>
        private void ProcessFromQueue(object threadId)
        {
            // Extract the thread ID
            int MyThreadId = (int)threadId;

            // Set the thread's name
            Thread.CurrentThread.Name = String.Format("{0}.{1:000}", Name, MyThreadId);

            // Log an info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Starting worker thread " + Thread.CurrentThread.Name, Level.Info);

            // While the service is not stopping and the service is not stopped
            while (Status != ExternalAppServiceStatus.Stopping && Status != ExternalAppServiceStatus.Stopped)
            {
                // Acquire a lock on the request queue
                lock (_requestQueueLock)
                {
                    // While the service is
                    // 1. not in an error state,
                    // 2. not stopping,
                    // 3. not stopped, and
                    // 4. there are no objects to process in the queue
                    while (
                        Status != ExternalAppServiceStatus.Error &&
                        Status != ExternalAppServiceStatus.Stopping &&
                        Status != ExternalAppServiceStatus.Stopped &&
                        RequestQueueCount == 0)
                    {
                        // Release the lock on the request queue and wait to be signalled
                        Monitor.Wait(_requestQueueLock);
                    }
                }

                // Acquire the lock on the service's state
                _stateLock.EnterReadLock();
                try
                {
                    // If the service is in an error state, stopping or stopped
                    if (
                        Status == ExternalAppServiceStatus.Error &&
                        Status == ExternalAppServiceStatus.Stopping ||
                        Status == ExternalAppServiceStatus.Stopped
                        )
                    {
                        // Commit suicide
                        break;
                    }

                    // While we have messages to process
                    while (_requestQueue.Count > 0)
                    {
                        // Create a variable to store the object in the queue
                        Object receivedObject = null;

                        // Acquire a lock on the request queue
                        lock (_requestQueueLock)
                        {
                            // Read the next object from the queue
                            if (_requestQueue.Count > 0)
                            {
                                receivedObject = _requestQueue.Dequeue();
                            }
                        }

                        // Invoke correct event handler on the logic engine.
                        if (receivedObject == null)
                        {
                            continue;
                        }
                        else if (receivedObject is MessageReceived)
                        {
                            // Message
                            MessageReceived messageReceived = (MessageReceived)receivedObject;

                            // Get the user's session object
                            UserSessionType userSession = GetOrCreateUserSession(messageReceived.From, messageReceived.To);

                            // Invoke the logic engine's event handler
                            LogicEngine.OnMessageReceived(messageReceived, userSession);
                        }
                        else if (receivedObject is FileReceived)
                        {
                            // File
                            FileReceived fileReceived = (FileReceived)receivedObject;

                            // Get the user's session object
                            UserSessionType userSession = GetOrCreateUserSession(fileReceived.From, fileReceived.To);

                            // Invoke the logic engine's event handler
                            LogicEngine.OnFileReceived(fileReceived, userSession);
                        }
                        else if (receivedObject is PaymentResponse)
                        {
                            // Payment response
                            PaymentResponse paymentResponse = (PaymentResponse)receivedObject;

                            // Get the user's session object
                            UserSessionType userSession = GetOrCreateUserSession(paymentResponse.UserId, paymentResponse.ContactName);

                            // Invoke the logic engine's event handler
                            LogicEngine.OnPaymentResponseReceived(paymentResponse, userSession);
                        }
                        else
                        {
                            // There was a non null object in the queue with a type we do not provision for.
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Unknown request type in queue.\nType = " + receivedObject.GetType(), Level.Warning);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Log some info
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while processing the request.\nException = " + e, Level.Error);
                }
                finally
                {
                    // Release the lock on the service's state
                    _stateLock.ExitReadLock();
                }
            }

            // Decrement the worker thread count
            RequestQueueThreads--;

            // Log an info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Stopping worker thread " + Thread.CurrentThread.Name, Level.Info);
        }
        #endregion

        /// <summary>
        /// Callback function to receive messages from MXit users.<br/>
        /// <br/>
        /// Implement this function to handle messages that MXit users send to your ExternalApp.<br/>
        /// <br/>
        /// <br/>
        /// <em>Note on user sessions:</em><br/>
        /// <br/>
        /// It is a good idea to create and store session objects for all the MXit users who are
        /// currently accessing your ExternalApp.<br/>
        /// <br/>
        /// E.g. you can use the session object to store the user's current location in your
        /// application (viewing main-menu, viewing leader-board, etc.), so that you will know how to
        /// interpret the next message you receive from the user.<br/>
        /// <br/>
        /// When you receive a message from a user for whom you do not have a session, create and
        /// store a new session object for the user and direct the user to the default or "home"
        /// location in your application.<br/>
        /// <br/>
        /// When you receive a notification that a user has gone offline (via the
        /// <see cref="OnPresenceReceived"/> callback), it is a good time to destroy the session
        /// object you've been keeping for the user, so that the next time the user accesses your
        /// application, a new session object will be created.
        /// </summary>
        /// <param name="messageReceived">The message that was sent by the user.</param>
        public void OnMessageReceived(MessageReceived messageReceived)
        {
            // Log some info
           // ConsoleLogger.Log(MethodBase.GetCurrentMethod(), messageReceived.ToString(), Level.Debug);

            // Acquire the lock on the service's state
            _stateLock.EnterReadLock();
            try
            {
                // If the service is not ready to process the request
                if (IsReadyToProcessRequest(messageReceived.To, messageReceived.From, messageReceived) == false)
                {
                    // Drop the request
                    return;
                }

                bool requestQueueIsFull = false;

                // Acquire a lock on the request queue
                lock (_requestQueueLock)
                {
                    // If the request queue is enabled
                    if (RequestQueueIsEnabled)
                    {
                        // If the request queue is not full
                        if (_requestQueue.Count < RequestQueueMaxCount)
                        {
                            // Add the message to the request queue
                            _requestQueue.Enqueue(messageReceived);

                            // Notify a worker thread that a message is ready
                            Monitor.Pulse(_requestQueueLock);

                            // We have nothing more to do, so return
                            return;
                        }

                        // If the request queue is full
                        else
                        {
                            // Set our flag
                            requestQueueIsFull = true;
                        }
                    }
                }

                // If the request queue is enabled, but it was full
                if (requestQueueIsFull)
                {
                    // Drop the request
                    return;
                }

                // The request queue is NOT enabled, process the message directly

                // Get the user's session object
                UserSessionType userSession = GetOrCreateUserSession(messageReceived.From, messageReceived.To);

                // Invoke the logic engine's event handler
                LogicEngine.OnMessageReceived(messageReceived, userSession);
            }
            catch (Exception e)
            {
                // Log some info
                ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while handling the event.\nException = " + e, Level.Error);
            }
            finally
            {
                // Release the lock on the service's state
                _stateLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Callback function to receive files from MXit users.<br/>
        /// <br/>
        /// Implement this function to handle files that MXit users send to your ExternalApp.
        /// </summary>
        /// <param name="fileReceived">The file that was sent by the user.</param>
        public void OnFileReceived(FileReceived fileReceived)
        {
            // Log some info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), fileReceived.ToString(), Level.Debug);

            // Acquire the lock on the service's state
            _stateLock.EnterReadLock();
            try
            {
                // If the service is not ready to process the request
                if (IsReadyToProcessRequest(fileReceived.To, fileReceived.From, fileReceived) == false)
                {
                    // Drop the request
                    return;
                }

                bool requestQueueIsFull = false;

                // Acquire a lock on the request queue
                lock (_requestQueueLock)
                {
                    // If the request queue is enabled
                    if (RequestQueueIsEnabled)
                    {
                        // If the request queue is not full
                        if (_requestQueue.Count < RequestQueueMaxCount)
                        {
                            // Add the file to the request queue
                            _requestQueue.Enqueue(fileReceived);

                            // Notify a worker thread that a message is ready
                            Monitor.Pulse(_requestQueueLock);

                            // We have nothing more to do, so return
                            return;
                        }

                        // If the request queue is full
                        else
                        {
                            // Set our flag
                            requestQueueIsFull = true;
                        }
                    }
                }

                // If the request queue is enabled, but it was full
                if (requestQueueIsFull)
                {
                    // Drop the request
                    return;
                }

                // The request queue is NOT enabled, process the file directly

                // Get the user's session object
                UserSessionType userSession = GetOrCreateUserSession(fileReceived.From, fileReceived.To);

                // Invoke the logic engine's event handler
                LogicEngine.OnFileReceived(fileReceived, userSession);
            }
            catch (Exception e)
            {
                // Log some info
                ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while handling the event.\nException = " + e, Level.Error);
            }
            finally
            {
                // Release the lock on the service's state
                _stateLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Callback function to receive updates about a user's presence information.<br/>
        /// <br/>
        /// Implement this function to be notified when a MXit user goes offline (e.g. to free
        /// resources allocated for the user).<br/>
        /// <br/>
        /// <br/>
        /// <em>Note on user sessions:</em><br/>
        /// <br/>
        /// If you store session objects for all the MXit users who are currently accessing your
        /// ExternalApp, and you receive a notification via this function that a user has gone
        /// offline, it is a good time to destroy the session object you've been keeping for the
        /// user.
        /// </summary>
        /// <param name="userPresence">The user's presence information.</param>
        public void OnPresenceReceived(Presence userPresence)
        {
            // Log some info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), userPresence.ToString(), Level.Debug);

            // If we've received an offline presence notification
            if (userPresence.IsOnline == false)
            {
                // Remove the user's session from cache
                ConcurrentDictionary<string, UserSessionType> userSessions = RemoveFromUserSessionCache(userPresence.UserId);

                if (userSessions != null)
                {
                    if (userSessions.Count > 0)
                    {
                        UserSessionType userSession = userSessions[Name];
                        // If the type inherits from our base type
                        if (userSession is ExternalAppUserSession)
                        {
                            // Cast and set the known members
                            ExternalAppUserSession userSessionAsExternalAppUserSession = userSession as ExternalAppUserSession;
                            userSessionAsExternalAppUserSession.logSessionEnd();
                        }
                    }
                    // Log some info
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Removed user session from cache.\nUserId = " + userPresence.UserId, Level.Debug);
                }
            }
        }

        /// <summary>
        /// Callback function to receive payment responses.<br/>
        /// <br/>
        /// Implement this function to be notified of payment responses. The ExternalAppAPI will send a payment
        /// response to your ExternalApp when a transaction result is ready for a payment request that your ExternalApp
        /// submitted earlier on.<br/>
        /// <br/>
        /// You can use the <see cref="Comms.RequestPayment"/> ExternalAppAPI function to submit a payment request.
        /// </summary>
        /// <param name="paymentResponse">The payment response.</param>
        public void OnPaymentResponseReceived(PaymentResponse paymentResponse)
        {
            // Log some info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), paymentResponse.ToString(), Level.Debug);

            // Acquire the lock on the service's state
            _stateLock.EnterReadLock();
            try
            {
                // If the service is not ready to process the request
                if (IsReadyToProcessRequest(paymentResponse.ContactName, paymentResponse.UserId, paymentResponse) == false)
                {
                    // Drop the request
                    return;
                }


                // Acquire a lock on the request queue
                lock (_requestQueueLock)
                {
                    // If the request queue is enabled
                    if (RequestQueueIsEnabled)
                    {
                        // Add the payment response to the request queue, regardless of the queue's size
                        _requestQueue.Enqueue(paymentResponse);

                        // Notify a worker thread that a message is ready
                        Monitor.Pulse(_requestQueueLock);

                        // We have nothing more to do, so return
                        return;
                    }
                }

                // The request queue is NOT enabled, process the message directly

                // Get the user's session object
                UserSessionType userSession = GetOrCreateUserSession(paymentResponse.UserId, paymentResponse.ContactName);

                // Invoke the logic engine's event handler
                LogicEngine.OnPaymentResponseReceived(paymentResponse, userSession);
            }
            catch (Exception e)
            {
                // Log some info
                ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while handling the event.\nException = " + e, Level.Error);
            }
            finally
            {
                // Release the lock on the service's state
                _stateLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Callback function to receive notification messages from the ExternalAppAPI.<br/>
        /// 	<br/>
        /// Implement this function to receive debug, info, warning, error, etc. messages, that are
        /// generated for your ExternalApp, while it is connected to MXit's platform via the
        /// ExternalAppAPI.<br/>
        /// <br/>
        /// You should consider any messages with a level of <see cref="MXit.Log.Level.Warning"/>
        /// or higher to be serious.<br/>
        /// <br/>
        /// For example, MXit may send warning notifications to your ExternalApp to notify it of
        /// upgrades or changes to the ExternalAppAPI, that may require changes or upgrades to your
        /// ExternalApp for it to continue working.<br/>
        /// <br/>
        /// If your ExternalApp is a critical application to your business, you should consider
        /// forwarding notifications received from this function to your operations team. 
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <param name="level">Notification level.</param>
        public void OnNotificationReceived(string message, Level level)
        {
            // Log some info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), message, level);
        }

        /// <summary>
        /// Callback function to receive a notification of important ExternalAppAPI server events
        /// that may affect your ExternalApp.<br/>
        /// 	<br/>
        /// Implement this function to take an appropriate action when an ExternalAppAPI server
        /// event occurs.<br/>
        /// 	<br/>
        /// E.g. you can implement this function to gracefully stop your application after
        /// receiving a <see cref="ServerEventType.ServerIsShuttingDown"/> event notification.
        /// </summary>
        /// <param name="serverEventType">The ExternalAppAPI server event type.</param>
        public void OnServerEvent(ServerEventType serverEventType)
        {
            // Log some info
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), serverEventType.ToString(), Level.Debug);

            // If we're receiving a shutdown event from the ExternalAppAPI
            if (serverEventType == ServerEventType.ServerIsShuttingDown)
            {
                // Log a warning
                ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "ExternalAppAPI server is about to go down for maintenance!", Level.Warning);

                // Acquire the lock on the service's state
                _stateLock.EnterWriteLock();
                try
                {
                    // Set the status
                    Status = ExternalAppServiceStatus.ExternalAppApiConnectionLost;

                    try
                    {
                        // Log an info
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Closing connection to the ExternalAppAPI...", Level.Info);

                        // Disconnect from the ExternalAppAPI
                        ExternalAppApiComms.Disconnect();

                        // Close the WCF connection
                        ExternalAppApiComms.Close();

                        // Log an info
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "ExternalAppAPI connection closed", Level.Info);
                    }
                    catch (Exception e)
                    {
                        // Log an error
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while disconnecting and closing ExternalAppAPI connection.\nException = " + e, Level.Warning);
                    }

                    // Disregard the closed WCF communication interface
                    ExternalAppApiComms = null;
                }
                finally
                {
                    // Release the lock on the service's state
                    _stateLock.ExitWriteLock();
                }

            }
        }

        #endregion



        #region KeepAlive

        /// <summary>
        /// Keep-alive method to maintain the connection to MXit's ExternalAppAPI.<br />
        /// <br />
        /// This method should be called periodically to prevent an active WCF connection from
        /// being closed due to a timeout, or establish a new connection in-case of a communication
        /// failure.
        /// </summary>
        /// <param name="stateInfo">Timer state info (ignored).</param>
        private void KeepAlive(object stateInfo)
        {
            // Log a message
            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Keep-alive triggered.", Level.Debug);


            // Acquire the lock on the service's state
            _stateLock.EnterUpgradeableReadLock();
            try
            {
                // If the service is currently running
                if (Status == ExternalAppServiceStatus.Running)
                {
                    try
                    {
                        // Fire off a keep-alive to prevent the connection from being closed due to a timeout
                        ExternalAppApiComms.KeepAlive();
                    }
                    catch (Exception e)
                    {
                        // Acquire a write-lock on the service's state
                        _stateLock.EnterWriteLock();

                        // Set the status
                        Status = ExternalAppServiceStatus.ExternalAppApiConnectionLost;

                        // If something went wrong with our ExternalAppAPI comms (probably a network connection failure)
                        if (e is CommunicationObjectFaultedException)
                        {
                            // Log a warning
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "ExternalAppAPI communication failure!\nService = " + this + "\nException = " + e, Level.Warning);
                        }

                        // If something else went wrong while calling our keep alive code
                        else
                        {
                            // Log an error
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while executing keep-alive!\nService = " + this + "\nException = " + e, Level.Error);
                        }
                    }
                }


                // If the ExternalAppAPI connection was lost
                if (Status == ExternalAppServiceStatus.ExternalAppApiConnectionLost)
                {
                    // Acquire a write-lock on the service's state
                    if (!_stateLock.IsWriteLockHeld) _stateLock.EnterWriteLock();
                    try
                    {
                        // Set the status
                        Status = ExternalAppServiceStatus.Reconnecting;

                        // Log some info
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Attempting to re-establish connection to the ExternalAppAPI...", Level.Info);

                        // Initialize our WCF instance 
                        InstanceContext context = new InstanceContext(this);

                        // Create a WCF communications interface to talk to the ExternalAppAPI
                        ExternalAppApiComms = new CommsClient(context);

                        // Connect to the ExternalAppAPI
                        ExternalAppApiComms.Connect(Name, Password, SDK.Instance);

                        try
                        {
                            // Log some info
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "ExternalAppAPI connection re-established, invoking reconnect event handler...", Level.Info);

                            // Invoke the logic engine's event handler
                            LogicEngine.OnReconnect();

                            // Set the status
                            Status = ExternalAppServiceStatus.Running;

                            // Connection established
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "ExternalAppAPI comms restored.\nService = " + this, Level.Info);
                        }
                        catch (Exception eventHandlerEx)
                        {
                            // Set the status
                            Status = ExternalAppServiceStatus.Error;

                            // Log an error
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while executing the OnReconnect event handler.\nException = " + eventHandlerEx, Level.Error);
                        }
                    }
                    catch (Exception reconnectEx)
                    {
                        // Set the status
                        Status = ExternalAppServiceStatus.ExternalAppApiConnectionLost;

                        // Re-connect failed
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Reconnect failed!\nService = " + this + "\nException = " + reconnectEx, Level.Error);
                    }
                }
            }
            finally
            {
                // Release the write-lock on the service's state
                if (_stateLock.IsWriteLockHeld) _stateLock.ExitWriteLock();

                // Release the read-lock on the service's state
                if (_stateLock.IsUpgradeableReadLockHeld) _stateLock.ExitUpgradeableReadLock();
                
                // If the ExternalApp is in an error state
                if (Status == ExternalAppServiceStatus.Error)
                {
                    try
                    {
                        // Stop the ExternalApp
                        Stop();
                    }
                    catch (Exception e)
                    {
                        // Log an error
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Could not stop ExternalApp in error state!\nService = " + this + "\nException = " + e, Level.Error);
                    }
                }
            }
        }

        #endregion



        #region Start & Stop

        /// <summary>
        /// Start this ExternalApp.
        /// </summary>
        public void Start()
        {
            lock (_requestQueueLock)
            {
                // Acquire the lock on the service's state
                _stateLock.EnterWriteLock();
                try
                {
                    // If the service is not stopped
                    if (Status != ExternalAppServiceStatus.Stopped) throw new InvalidOperationException("To start a service, it has to be in the " + ExternalAppServiceStatus.Stopped + " state.");

                    // Set the status
                    Status = ExternalAppServiceStatus.Starting;

                    // Start the request processing threads
                    for (int i = 1; i <= _requestQueueThreadsRequired; i++)
                    {
                        StartRequestQueueWorkerThread();
                    }

                    try
                    {
                        // Initialize our WCF instance 
                        InstanceContext context = new InstanceContext(this);

                        // Create a WCF communications interface to talk to the ExternalAppAPI
                        ExternalAppApiComms = new CommsClient(context);
                    }
                    catch (Exception e)
                    {
                        // Set the status
                        Status = ExternalAppServiceStatus.Error;

                        // Re-throw the exception
                        throw new SystemException("An error occurred while establishing WCF connection to the ExternalAppAPI.", e);
                    }

                    try
                    {
                        // Connect to the ExternalAppAPI
                        ExternalAppApiComms.Connect(Name, Password, SDK.Instance);
                    }
                    catch (Exception e)
                    {
                        // Set the status
                        Status = ExternalAppServiceStatus.Error;

                        // Re-throw the exception
                        throw new SystemException("An error occurred while connecting the ExternalApp to MXit's platform.", e);
                    }

                    // Mark the start-up timestamp
                    StartTimestamp = DateTime.Now;

                    try
                    {
                        // Invoke the logic engine's event handler
                        LogicEngine.OnStart();
                    }
                    catch (Exception e)
                    {
                        // Set the status
                        Status = ExternalAppServiceStatus.Error;

                        // Re-throw the exception
                        throw new SystemException("An error occurred while executing the OnStart event handler.", e);
                    }

                    // Set the status
                    Status = ExternalAppServiceStatus.Running;

                    // Create a new keep-alive timer
                    int keepAliveMilliseconds = 60 * 1000;    // 1 minute
                    _keepAliveTimer = new Timer(new TimerCallback(KeepAlive), null, keepAliveMilliseconds, keepAliveMilliseconds);

                    // Started!
                    string logMessage = string.Format(
                        "{0}{1}{2}{3}{4}{5}",
                        "\n--------------------------------------------------------------------------------------------------------------",
                        "\nExternalApp started.",
                        "\n--------------------------------------------------------------------------------------------------------------",
                        "\n", this,
                        "\n--------------------------------------------------------------------------------------------------------------");
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), logMessage, Level.Info);
                }
                finally
                {
                    // Release the lock on the service's state
                    _stateLock.ExitWriteLock();

                    // If the ExternalApp is in an error state
                    if (Status == ExternalAppServiceStatus.Error)
                    {
                        try
                        {
                            // Stop the ExternalApp
                            Stop();
                        }
                        catch (Exception e)
                        {
                            // Log an error
                            ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Could not stop ExternalApp in error state!\nService = " + this + "\nException = " + e, Level.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stop this ExternalApp.
        /// </summary>
        public void Stop()
        {
            lock (_requestQueueLock)
            {
                // Acquire the lock on the service's state
                _stateLock.EnterWriteLock();
                try
                {
                    // If the service is not running
                    if (Status == ExternalAppServiceStatus.Stopping || Status == ExternalAppServiceStatus.Stopped) throw new InvalidOperationException("To start a service, it has to be in the " + ExternalAppServiceStatus.Running + " state.");

                    // Set the status
                    Status = ExternalAppServiceStatus.Stopping;

                    // Linger to allow all threads some time to finish processing
                    Thread.Sleep(100);

                    // Dispose of threads waiting for messages
                    Monitor.PulseAll(_requestQueueLock);

                    try
                    {
                        // If a keep-alive timer exists
                        if (_keepAliveTimer != null)
                        {
                            // Dispose the keep-alive timer
                            _keepAliveTimer.Dispose();
                            _keepAliveTimer = null;
                        }
                    }
                    catch (Exception e)
                    {
                        // Log an error
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while disposing keep-alive timer.\nException = " + e, Level.Error);
                    }

                    try
                    {
                        // Disconnect from the ExternalAppAPI
                        ExternalAppApiComms.Disconnect();
                    }
                    catch (Exception e)
                    {
                        // Log an error
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while disconnecting the ExternalApp from MXit's framework.\nException = " + e, Level.Warning);
                    }

                    try
                    {
                        // Close the WCF connection
                        ExternalAppApiComms.Close();
                    }
                    catch (Exception e)
                    {
                        // Log an error
                        ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred while closing the ExternalAppAPI connection.\nException = " + e, Level.Warning);
                    }

                    // Disregard the closed WCF communication interface
                    ExternalAppApiComms = null;

                    // Clear the user session cache
                    ClearUserSessionCache();

                    try
                    {
                        // Invoke the logic engine's event handler
                        LogicEngine.OnStop();
                    }
                    catch (Exception e)
                    {
                        // Set the status
                        Status = ExternalAppServiceStatus.Error;

                        // Re-throw the exception
                        throw new SystemException("An error occurred while executing the OnStop event handler.", e);
                    }

                    // Clear the start-up timestamp
                    StartTimestamp = null;

                    // Set the status
                    Status = ExternalAppServiceStatus.Stopped;

                    // Stopped!
                    string logMessage = string.Format(
                        "{0}{1}{2}{3}{4}{5}",
                        "\n--------------------------------------------------------------------------------------------------------------",
                        "\nExternalApp stopped.",
                        "\n--------------------------------------------------------------------------------------------------------------",
                        "\n", this,
                        "\n--------------------------------------------------------------------------------------------------------------");
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), logMessage, Level.Info);
                }
                finally
                {
                    // Release the lock on the service's state
                    _stateLock.ExitWriteLock();
                }
            }
        }

        #endregion



        #region ToString

        /// <summary>
        /// Adds key-value items to the output produced by a call to <see cref="M:MXit.Common.WithExpandedToString.ToString"/>.
        /// </summary>
        /// <param name="sb">The string builder to add the key-value items to.</param>
        /// <param name="itemFormat">The key-value item format.</param>
        /// <param name="indent">Number of indentations.</param>
        /// <param name="spacer">String to use as indentation spacer.</param>
        /// <param name="equals">String to use a equals sign.</param>
        /// <param name="preText">The pre-text before each entry per line.</param>
        /// <example>
        /// 	<code>
        /// // This function will typically contain a series of name/value entries, e.g.
        /// sb.AppendFormat(itemFormat, "Name", Name);
        /// sb.AppendFormat(itemFormat, "Port", Port);
        /// sb.AppendFormat(itemFormat, "Host", System.Net.Dns.GetHostName());
        /// </code>
        /// </example>
        public override void ToStringAddKeyValueItems(StringBuilder sb, string itemFormat, int indent, string spacer, string equals, string preText)
        {
            // Acquire the lock on the service's state
            _stateLock.EnterReadLock();
            try
            {
                base.ToStringAddKeyValueItems(sb, itemFormat, indent, spacer, equals, preText);
                sb.AppendFormat(itemFormat, "Status", Status);
                if (Status != ExternalAppServiceStatus.Stopped)
                {
                    sb.AppendFormat(itemFormat, "StartTimestamp", StartTimestamp.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                    sb.AppendFormat(itemFormat, "Uptime", Uptime.Value);
                }
                sb.AppendFormat(itemFormat, "ProcessName", Process.GetCurrentProcess().ProcessName);
                sb.AppendFormat(itemFormat, "ProcessStartTime", Process.GetCurrentProcess().StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendFormat(itemFormat, "ProcessThreadCount", Process.GetCurrentProcess().Threads.Count);
                sb.AppendFormat(itemFormat, "MemoryUsage (RAM)", string.Format("{0:F3} MB", Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0)));
                sb.AppendFormat(itemFormat, "MemoryUsage (Virtual)", string.Format("{0:F3} MB", Process.GetCurrentProcess().PrivateMemorySize64 / (1024.0 * 1024.0)));
                sb.AppendFormat(itemFormat, "LogicEngine", LogicEngine.GetType());
                if (RequestQueueIsEnabled)
                {
                    sb.AppendFormat(itemFormat, "QueueWorkerThreads", RequestQueueCount + (Status != ExternalAppServiceStatus.Stopped && RequestQueueCount != _requestQueueThreadsRequired ? "(Required = " + _requestQueueThreadsRequired + ")" : ""));
                    sb.AppendFormat(itemFormat, "RequestQueueCount", RequestQueueCount + "(Max = " + RequestQueueMaxCount + ")");
                }
            }
            finally
            {
                // Release the lock on the service's state
                _stateLock.ExitReadLock();
            }
        }

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAppService"/> class.
        /// </summary>
        /// <param name="logicEngine">The ExternalApp's logic engine.</param>
        /// <param name="name">The ExternalApp's name.</param>
        /// <param name="password">The ExternalApp's connection password.</param>
        /// <param name="requestQueueThreads">The number of threads to start to process requests from the service's request queue.</param>
        /// <param name="requestQueueMaxCount">The maximum number of requests allowed in the service's request queue before the service will start dropping new requests.</param>
        public ExternalAppService(LogicEngineType logicEngine, string name, string password, int requestQueueThreads = DefaultRequestQueueThreads, int requestQueueMaxCount = DefaultRequestQueueMaxCount)
            : base(name, password)
        {
            if (requestQueueThreads < 0) throw new ArgumentException("requestQueueThreads must be an integer greater to or equal to 0");
            if (requestQueueMaxCount < 1) throw new ArgumentException("requestQueueMaxCount must be an integer greater to or equal to 1");
            if (logicEngine.Service != null) throw new ArgumentException("This logic engine is already associated with another service (" + logicEngine.Service.Name + ")");

            _requestQueueThreadsRequired = requestQueueThreads;
            RequestQueueMaxCount = requestQueueMaxCount;

            Status = ExternalAppServiceStatus.Stopped;

            LogicEngine = logicEngine;
            LogicEngine.Service = this;
        }

        #endregion

    }
}
