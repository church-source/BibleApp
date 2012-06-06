using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    //this class contains the custom user profile data. i.e. not the data in user info. also will be responsible for updating the user profile entry when the user changes these values.
    public class UserProfileCustomData
    {
        public long id { get; private set; } //this is just a "foreign key" reference
        public String user_code { get; private set; }
        public String translation { get; private set; } //this should of been an int. 
        public String user_name { get; private set; }
        public int colour_theme { get; private set; }
        public Boolean is_subscribed_to_dv { get; private set; }

        public UserProfileCustomData(
            long id,
            String user_name,
            String translation,
            String user_code,
            int colour_theme,
            Boolean is_subscribed_to_dv)
        {
            this.id = id;
            this.user_code = user_code;
            this.translation = translation;
            this.user_name = user_name;
            this.colour_theme = colour_theme;
            this.is_subscribed_to_dv = is_subscribed_to_dv;
        }

        public void setTranslationID(String translation, Boolean updateDB)
        {
            this.translation = translation;
            if(updateDB)
                UserProfileDBManager.updateDefaultTranslation(id, translation);
        }

        public void setColourTheme(int colour_theme)
        {
            this.colour_theme = colour_theme;
            UserProfileDBManager.updateColourTheme(id, colour_theme);
        }

        public void setUserName(String user_name)
        {
            this.user_name = user_name;
        }

        public void setIsSubscribedToDailyVerse(Boolean is_subscribed)
        {
            this.is_subscribed_to_dv = is_subscribed;
        }
    }
}
