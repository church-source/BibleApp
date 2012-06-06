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
using MXit.ExternalApp;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace MxitTestApp
{
    public class UserProfile
    {
        public long id { get; private set; }
        public string user_id { get; private set; }
        public UserInfo user_info { get; set; }
        public UserProfileCustomData user_profile_custom{ get; private set; }
        public bool is_suspended { get; set; }
        public bool is_admin { get; set; }


        public UserProfile(long id, string user_id, UserInfo user_info, UserProfileCustomData user_profile_custom)
        {
            this.id = id;
            this.user_id = user_id;
            this.user_info = user_info;
            this.user_profile_custom = user_profile_custom;
            is_admin = UserRoleManager.getInstance().isUserAdmin(this);
        }

        /*update memory and db*/
        public void setDefaultTranslationId(String translation_id)
        {
            this.user_profile_custom.setTranslationID(translation_id, true);
        }

        public String getDefaultTranslationId()
        {
            return this.user_profile_custom.translation;
        }

        /*update memory and db*/
        public void setUserName(String user_name)
        {
            String old_user_name = this.user_profile_custom.user_name;
            UserNameManager.getInstance().saveUserNameToDBProfile(id, user_name);
            this.user_profile_custom.setUserName(user_name);
        }

        public String geUserName()
        {
            return this.user_profile_custom.user_name;
        }

        public void setIsSubscribedToDailyVerseAndUpdateDB(Boolean is_subscribed)
        {
            this.user_profile_custom.setIsSubscribedToDailyVerse(is_subscribed);
            UserProfileDBManager.updateDailyVerseSubscrtipion(id,is_subscribed);
        }

        public static UserProfile loadUserProfile(string user_id, UserInfo user_info)
        {

            //check if user exists and get id to create new user profile session. 
            UserProfileCustomData upcd = UserProfileDBManager.loadCustomUserProfileData(user_id);
            long id = -1;
            if (upcd != null)
            {
                id = upcd.id;
                return new UserProfile(upcd.id, user_id, user_info, upcd);
            }
            else
            {
                String randomCode = BibleUserCodeCreator.getInstance().generateUniqueANRandomCode(BibleUserCodeCreator.CODE_LENGTH);
                id = UserProfileDBManager.addUser(user_id, user_info, TEMP_USER_NAME + user_id, randomCode);
                upcd = new UserProfileCustomData(id, TEMP_USER_NAME + user_id, DEFAULT_TRANSLATION, randomCode,0,true);
                if (upcd != null && upcd.id != -1)
                {
                    return new UserProfile(id, user_id, user_info, upcd);
                }
                else
                {
                    throw new Exception("Could not add new user to DB");
                }
            }
        }
        public const String TEMP_USER_NAME = "TEMP_USER_NAME_";
        public const String BIBLE_APP_USER_NAME = "BibleApp";
        public const string DEFAULT_TRANSLATION = "2";
    }
}
