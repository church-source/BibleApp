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

using MXit.ExternalApp;

namespace MxitTestApp
{
    /// <summary>
    /// Class that represents a user's session on the application
    /// </summary>
    public class UserSession : ExternalAppUserSession
    {

        public UserProfile user_profile { get; set; }
        public string current_menu_loc { get; set; }
        public int current_menu_page { get; set; }
        public UserSessionScreenHistory screen_history { get; set; }
        public long session_id { get; private set; }
        public VerseHistory verse_history { get; private set; }
        public FavouriteVerseManager favourite_verses { get; private set; }
        public BookmarkManager bookmark_manager { get; private set; }
        public FriendManager friend_manager { get; private set; }
        public VerseMessagingManager verse_messaging_manager { get; private set; }
        private static Object thisLock = new Object();
        /*
         *we store all context variables per user in a Hash Table 
         *naive implementation and all context variables are string 
         */
        private Dictionary<String,Object> session_context_variables;
        public List<SearchVerseRecord> search_results { get; set; }

        public UserSession()
        {
            session_id = -1; //initialize so we can check if it has been set before.
            this.current_menu_loc = MenuDefinition.UNDEFINED_MENU_ID;

            screen_history = new UserSessionScreenHistory();//
            session_context_variables = new Dictionary<String, Object>(); 
        }

        /* this method checks if the the user profile has been loaded and if not loads it
         * and logs the start of the session
         */
        public void initializeUserSession(MessageReceived messageReceived, UserInfo user_info)
        {
            lock (thisLock)
            {
                //the user profile will be null when receiving the first message from the user in this session
                if (user_profile == null)
                {
                    user_profile = UserProfile.loadUserProfile(
                            messageReceived.From,
                            user_info);
                    logSessionStart();

                    //now we also get the Verse History list, but lest see if we can get this later rather. 
                    //but for now we keep it here. 
                    //the verse_history is loaded in the constructor.
                    verse_history = new VerseHistory(user_profile, this);


                    //now we also get the Favourite Verse list, but let see if we can get this later rather. 
                    //but for now we keep it here. 
                    //the favourite verses is loaded in the constructor.
                    favourite_verses = new FavouriteVerseManager(
                        user_profile, 
                        this);

                    //we initialize the current bookmark. if this is a new subscriber the bookmark stored inside will just be null. 
                    bookmark_manager = new BookmarkManager(user_profile, this);

                    friend_manager = new FriendManager(user_profile, this);

                    verse_messaging_manager = new VerseMessagingManager(this);
                    search_results = null;
                }
            }
        }

        public MessageToSend handleInput(MessageReceived message_received)
        {
            MessageToSend messageToSend = message_received.CreateReplyMessage();
            messageToSend.Clear();
            InputHandlerResult action = null;
            //new session so we append chat screen config
            if (current_menu_loc == MenuDefinition.UNDEFINED_MENU_ID)
            {
                AScreenOutputAdapter.appendInitialMessageConfig(this.user_profile,messageToSend);
            }
            if (current_menu_loc == MenuDefinition.UNDEFINED_MENU_ID && user_profile.user_profile_custom.user_name.StartsWith(GUEST_USER_NAME_PREFIX))
            {
                String guest_name = GUEST_USER_NAME_PREFIX + user_profile.id;
                setVariable(GUEST_USER_NAME_ASSIGNED, guest_name); 
            }
            else if (current_menu_loc == MenuDefinition.UNDEFINED_MENU_ID && user_profile.user_profile_custom.user_name.StartsWith(UserProfile.TEMP_USER_NAME))
            {
                String guest_name = GUEST_USER_NAME_PREFIX + user_profile.id;
                user_profile.setUserName(guest_name);
                setVariable(GUEST_USER_NAME_ASSIGNED, guest_name); 
            }
            bool is_suspended = SuspensionManager.getInstance().isSuspended(user_profile.id);
            if (is_suspended)
            {
                user_profile.is_suspended = true;
            }
            else
            {
                user_profile.is_suspended = false;
            }



            IInputHandler i_handler = InputHandlerFactory.getInputHandler(current_menu_loc);
            action = i_handler.handleInput(this, message_received);
            //special refer friend action.
            if (hasVariable(MainMenuHandler.REFER_A_FRIEND))
            {
                removeVariable(MainMenuHandler.REFER_A_FRIEND);
                return null;
            }
            handleAction(action);
            if (this.hasVariable(AScreenOutputAdapter.COLOUR_CHANGED))
            {
                this.removeVariable(AScreenOutputAdapter.COLOUR_CHANGED);
                AScreenOutputAdapter.appendInitialMessageConfig(this.user_profile, messageToSend);
            }
            MessageToSend output = MenuManager.getInstance().getScreenMessage(
                                                                    this,
                                                                    messageToSend,
                                                                    action);


            return output;
        }

        public MessageToSend handleError(MessageReceived message_received)
        {
            MessageToSend messageToSend = message_received.CreateReplyMessage();
            messageToSend.Clear();

            InputHandlerResult action = new InputHandlerResult(
                    InputHandlerResult.ROOT_MENU_ACTION,
                    MenuDefinition.ROOT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            handleAction(action);
            MessageToSend output = MenuManager.getInstance().getScreenMessage(
                                                                    this,
                                                                    messageToSend,
                                                                    action);

            return output;
        }

        /*this handles the action that is returned by a input handler*/
        public void handleAction(InputHandlerResult result)
        {
            if (InputHandlerResult.ROOT_MENU_ACTION.Equals(result.action))
            {
                screen_history.clear_history();
                //clear session
                //session_context_variables.Clear();
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                current_menu_loc = MenuDefinition.ROOT_MENU_ID;
                search_results = null;
            }
            else if (InputHandlerResult.BACK_MENU_ACTION.Equals(result.action))
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                if(current_menu_loc!=MenuDefinition.ROOT_MENU_ID)
                    current_menu_loc = getPrevScreenID();
                IInputHandler i_handler = InputHandlerFactory.getInputHandler(current_menu_loc);
                i_handler.init(this);

            }
            else if (InputHandlerResult.INVALID_MENU_ACTION.Equals(result.action))
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                current_menu_loc = current_menu_loc;
            }
            else if (InputHandlerResult.CHANGE_PAGE_ACTION.Equals(result.action))
            {
                //the page id was passed on in the result from the handler. 
                current_menu_page = result.page_id;
                current_menu_loc = result.menu_id;
            }
            else if (InputHandlerResult.NEXT_PAGE_ACTION.Equals(result.action))
            {
                //the page id was passed on in the result from the handler. 
                current_menu_page = result.page_id;
                current_menu_loc = result.menu_id;
            }
            else if (InputHandlerResult.PREV_PAGE_ACTION.Equals(result.action))
            {
                current_menu_page = result.page_id;
                current_menu_loc = result.menu_id;
            }
            else if (InputHandlerResult.CONF_PAGE_ACTION.Equals(result.action))
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                current_menu_loc = current_menu_loc;
            }
            else if (InputHandlerResult.FAVOURITE_ADDED_ACTION.Equals(result.action))
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                current_menu_loc = current_menu_loc;
            }
            else if (InputHandlerResult.DO_NOTHING_ACTION.Equals(result.action))
            {
                current_menu_page = current_menu_page;
                current_menu_loc = current_menu_loc;
            }
            else if (InputHandlerResult.BACK_WITHOUT_INIT_MENU_ACTION.Equals(result.action))
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                current_menu_loc = getPrevScreenID();
            }
            else if (InputHandlerResult.DISPLAY_MESSAGE.Equals(result.action))
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                current_menu_loc = current_menu_loc; // we hardcode this now. 
            }
            else
            {
                current_menu_page = InputHandlerResult.DEFAULT_PAGE_ID;
                string prev_loc = screen_history.peekPreviousScreenID();
                if (null == prev_loc || !result.menu_id.Equals(current_menu_loc))
                {
                    screen_history.addPreviousScreenID(current_menu_loc);
                    current_menu_loc = result.menu_id;
                    IInputHandler i_handler = InputHandlerFactory.getInputHandler(current_menu_loc);
                    i_handler.init(this);
                }
                else
                {
                    current_menu_loc = result.menu_id;
                }
            }

            Console.WriteLine("User with ID: " + user_profile.id +" is in current menu with ID: " + current_menu_loc);
        }

        public string getPrevScreenID()
        {
            if (screen_history.peekPreviousScreenID() != null)
                return screen_history.getPreviousScreenID();
            else
                return null;

        }

        public string peekPrevScreenID()
        {
            return screen_history.peekPreviousScreenID();
        }

        //set variable, if not exists create it
        public void setVariable(String name, Object val)
        {
            if (session_context_variables.ContainsKey(name))
                session_context_variables.Remove(name);
           
            session_context_variables.Add(name, val);

        }

        //get variable, if not exists return null
        public string getVariable(String name)
        {
            if(session_context_variables.ContainsKey(name))
                return (string)session_context_variables[name];

            return null;
        }

        public Boolean hasVariable(String name)
        {
            return session_context_variables.ContainsKey(name);
        }

        //get variable, if not exists return null
        public object getVariableObject(String name)
        {
            if(session_context_variables.ContainsKey(name))
                return session_context_variables[name];
            
            return null;
        }

        //remove and return
        public object removeVariable(String name)
        {
            try
            {
                if (session_context_variables.ContainsKey(name))
                {
                    Object o = getVariableObject(name);
                    deleteVariable(name);
                    return o;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /*make sure to call this only at the start of a session. */
        public void logSessionStart()
        {
            //TODO: need to investigate why sometimes a session is not ended. i.e. more than
            //one session id is present for the same session. However we fix it here by
            //making sure we always close the session if there is an open session already. 
            if (session_id != -1)
                logSessionEnd();

            session_id = UserProfileDBManager.logSessionStart(user_profile.id);
        }

        /*make sure to call this only at the start of a session. */
        public override void logSessionEnd()
        {
            friend_manager.removeFriendMapFromSession();
            UserProfileDBManager.logSessionEnd(user_profile.id, session_id);
        }

        //get variable, if not exists return null
        public void deleteVariable(String name)
        {
            session_context_variables.Remove(name);
        }

        /*
         * records history of verse requests
         */
        public void recordVerseSelection(Verse start, Verse end)
        {
            lock (thisLock)
            {
                if (verse_history != null)
                {
                    verse_history.saveVerseRequest(
                        this,
                        user_profile,
                        start,
                        end);
                }
            }
        }

        /*
         * records history of verse requests
         */
        public int recordFavouriteSelection(Verse start, Verse end)
        {
            lock (thisLock)
            {
                if (favourite_verses != null)
                {
                    return favourite_verses.saveFavouriteVerse(
                        this,
                        user_profile,
                        start,
                        end);
                }
            }
            return 2; //change this to constant
        }

        /*
         * saves or updates the bookmark
         */
        public int saveBookmarkVerse(Verse start, Verse end)
        {
            lock (thisLock)
            {
                if (bookmark_manager != null)
                {
                    return bookmark_manager.saveOrUpdateBookmark(
                        this,
                        user_profile,
                        start,
                        end);
                }
            }
            return 2; //change this to constant
        }

        /*
         * records history of verse requests
         */
        public int deleteFavouriteSelection(long favourite_verse_id)
        {
            lock (thisLock)
            {
                if (favourite_verses != null)
                {
                    return favourite_verses.deleteFavouriteVerse(
                        this, 
                        favourite_verse_id);
                }
            }
            return 2; //change this to constant
        }

        public bool hasNewFriendRequest()
        {
            try
            {
                if (friend_manager.getFriendRequests().Count() > 0)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("WARNING!!! CAUGHT EXCEPTION: " + e.Message + "\r\n" + e.StackTrace);
                return false;
            }
            return false;
        }

        public bool hasNewMessageEvent()
        {
            try
            {
                if (verse_messaging_manager.isAThreadUpdatedSinceLastAccess())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("WARNING!!! CAUGHT EXCEPTION: " + e.Message + "\r\n" + e.StackTrace);
                return false;
            }
            return false;
        }
        public bool hasNewEvent()
        {
            if (hasNewFriendRequest())
            {
                return true;
            }
            if (hasNewMessageEvent())
            {
                return true;
            }
            return false; 
        }

        public const String GUEST_USER_NAME_PREFIX = "Guest";
        public const String GUEST_USER_NAME_ASSIGNED = "GUEST";
    }

    

}
