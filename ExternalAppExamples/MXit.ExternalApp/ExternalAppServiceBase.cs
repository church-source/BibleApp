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
using System.Text;
using System.Threading;

using MXit.Common;

using MXit.ExternalApp.ExternalAppAPI;


namespace MXit.ExternalApp
{

#if DotNet4
    // DotNet4 has a concurrent dictionary implementation
#else
    #region ConcurrentDictionary Implementation
    
    /// <summary>
    /// Thread safe class used for a dictionary.
    /// </summary>
    public class ConcurrentDictionary <TKey, TValue>
    {
        /// <summary>
        /// Lock object to enable concurrent access to the <see cref="ConcurrentDictionary"/>.
        /// </summary>
        private ReaderWriterLockSlim _dictionaryLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// The base Dictionary object that is wrapped by the <see cref="ConcurrentDictionary"/>.
        /// </summary>
        private Dictionary<TKey, TValue> _baseDictionary;

        /// <summary>
        /// Default constructer.
        /// </summary>
        public ConcurrentDictionary ()
        {
            _baseDictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Clears the <see cref="ConcurrentDictionary"/> of all items.
        /// </summary>
        public void Clear()
        {
            _dictionaryLock.EnterWriteLock();
            try
            {
                _baseDictionary.Clear();
            }
            finally
            {
                _dictionaryLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// The number of key/value pairs contained in the <see cref="ConcurrentDictionary"/>.
        /// </summary>
        public int Count
        {
            get
            {
                _dictionaryLock.EnterReadLock();
                try
                {
                    return _baseDictionary.Count;
                }
                finally
                {
                    _dictionaryLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Attempts to remove and return the the value with the specified key from the <see cref="ConcurrentDictionary"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        /// When this method returns, value contains the object removed from the <see cref="ConcurrentDictionary"/>
        /// or the default value of if the operation failed.
        /// </param>
        /// <returns><c>true</c> if an object was removed successfully; otherwise, <c>false</c>.</returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            _dictionaryLock.EnterWriteLock();
            try
            {
                if (!_baseDictionary.TryGetValue(key, out value))
                {
                    throw new Exception("Unable to get value from dictionary");
                }
                _baseDictionary.Remove(key);
                return true;
            }
            catch (Exception)
            {
                value = default(TValue);
                return false;
            }
            finally
            {
                _dictionaryLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary"/> if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in 
        /// the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.
        /// </returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            _dictionaryLock.EnterUpgradeableReadLock();
            try
            {
                if (_baseDictionary.ContainsKey(key))
                {
                    return _baseDictionary[key];
                }

                TValue value = valueFactory.Invoke(key);

                _dictionaryLock.EnterWriteLock();

                _baseDictionary.Add(key, value);

                return value;
            }
            finally
            {
                if (_dictionaryLock.IsWriteLockHeld) _dictionaryLock.ExitWriteLock();
                if (_dictionaryLock.IsUpgradeableReadLockHeld) _dictionaryLock.ExitUpgradeableReadLock();
            }
        }
    }

    #endregion
#endif

    /// <summary>
    /// A generic ExternalApp service.<br />
    /// <br />
    /// This class handles all the technical bits for an ExternalApp (connect, disconnect, user session management, etc.).<br />
    /// <br />
    /// The business logic for the ExternalApp is handled by the service's <see cref="LogicEngine"/>.
    /// </summary>
    /// <typeparam name="UserSessionType">The class your ExternalApp uses to store session information for a user.</typeparam>
    public abstract class ExternalAppServiceBase<UserSessionType> : WithExpandedToString
        where UserSessionType : class, new()
    {
        #region Variables & Properties

        /// <summary>
        /// This ExternalApp's user session cache.
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, UserSessionType>> _userSessionCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, UserSessionType>>();

        /// <summary>
        /// The WCF communication interface this ExternalApp uses to talk to the ExternalAppAPI.
        /// </summary>
        internal CommsClient ExternalAppApiComms { get; set; }

        /// <summary>
        /// This ExternalApp's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This ExternalApp's connection password.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// The service's current status.
        /// </summary>
        public ExternalAppServiceStatus Status { get; protected set; }

        #endregion



        #region User Session Cache

        /// <summary>
        /// Creates a new user session object for the given contact name.
        /// </summary>
        /// <param name="contactName">The contact name (i.e. the context) within which the user is accessing the ExternalApp.</param>
        /// <returns>A new user session object.</returns>
        private UserSessionType CreateUserSessionInstance(string contactName)
        {
            // Create a new user session instance
            UserSessionType userSession = new UserSessionType();

            // If the type inherits from our base type
            if (userSession is ExternalAppUserSession)
            {
                // Cast and set the known members
                ExternalAppUserSession userSessionAsExternalAppUserSession = userSession as ExternalAppUserSession;
                userSessionAsExternalAppUserSession.ContactName = contactName;
            }

            // Return the new user session object
            return userSession;
        }

        /// <summary>
        /// Creates a new dictionary to store the user sessions for a MXit user (one for each context within which
        /// the user is accessing the ExternalApp).
        /// </summary>
        /// <param name="userId">The MXit user's UserId.</param>
        /// <returns>A new dictionary of user sessions.</returns>
        private ConcurrentDictionary<string, UserSessionType> CreateMulitContextUserSession(string userId)
        {
            return new ConcurrentDictionary<string, UserSessionType>();
        }

        /// <summary>
        /// Returns a user session object for the given UserId and contact name (i.e. context) from this ExternalApp's
        /// user session cache.<br />
        /// <br />
        /// If no such user session is cached, a new user session object will be created and added to the cache
        /// before it is returned. 
        /// </summary>
        /// <param name="userId">The MXit user's UserId.</param>
        /// <param name="contactName">The contact name (i.e. the context) within which the user is accessing the ExternalApp.</param>
        /// <returns>A user session object for the given UserId and contact name.</returns>
        protected UserSessionType GetOrCreateUserSession(string userId, string contactName)
        {
            // Get/create the multi-context user session
            ConcurrentDictionary<string, UserSessionType> userSessions =
                _userSessionCache.GetOrAdd(
                    userId,
                    new Func<string, ConcurrentDictionary<string, UserSessionType>>(CreateMulitContextUserSession)
                    );

            // Get/create the user session for the given context
            return userSessions.GetOrAdd(contactName, new Func<string, UserSessionType>(CreateUserSessionInstance));
        }

        /// <summary>
        /// Removes the user session for the given UserId from this ExternalApp's user session cache.
        /// </summary>
        /// <param name="userId">The MXit user's UserId.</param>
        /// <returns>The user session object that was removed from the cache, or NULL if no user session object was cached for the given UserId.</returns>
        protected ConcurrentDictionary<string, UserSessionType> RemoveFromUserSessionCache(string userId)
        {
            ConcurrentDictionary<string, UserSessionType> userSessions = null;
            _userSessionCache.TryRemove(userId, out userSessions);

            return userSessions;
        }

        /// <summary>
        /// Remove all entries from the user session cache.
        /// </summary>
        protected void ClearUserSessionCache()
        {
            _userSessionCache.Clear();
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
            sb.AppendFormat(itemFormat, "Name", Name);
            sb.AppendFormat(itemFormat, "Password", Password);
            sb.AppendFormat(itemFormat, "ExternalAppSdkVersion", SDK.Instance.Version);
            if (Status != ExternalAppServiceStatus.Stopped) sb.AppendFormat(itemFormat, "UserSessionCacheCount", _userSessionCache.Count);
        }

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAppServiceBase"/> class.
        /// </summary>
        /// <param name="name">The ExternalApp's name.</param>
        /// <param name="password">The ExternalApp's connection password.</param>
        protected ExternalAppServiceBase(string name, string password)
            : base(22)
        {
            Name = name;
            Password = password;
        }

        #endregion

    }
}
