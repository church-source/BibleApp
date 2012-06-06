using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class ColourThemeOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public ColourThemeOptionSet(String target_page) 
        {
            this.target_page = target_page;
            init();
        }

        public override void init()
        {
            Dictionary<int, UserColourTheme> colour_themes = UserColourTheme.getColourThemes();
            List<UserColourTheme> theme_list = ListUtils.convertColourThemeDictionaryToList(colour_themes);
            theme_list.Sort();
            UserColourTheme uct;
            if (theme_list != null)
            {
                for (int i = 0; i < theme_list.Count; i++)
                {
                    uct = theme_list[i];
                    list.Add(
                        new MenuOptionItem(
                            (i + 1).ToString(),
                            (uct.colour_theme).ToString(),
                            target_page,
                            uct.theme_name));
                }
            }
        }

        //we pass user session in case some people can only look at certain translations in future. 
        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            return list;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            return input;
        }
    }
}
