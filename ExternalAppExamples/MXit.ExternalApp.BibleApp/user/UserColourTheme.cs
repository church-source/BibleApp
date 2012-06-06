using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MxitTestApp
{
    public class UserColourTheme : IComparable
    {
        public int colour_theme = 0;
        public string theme_name { get; private set; }
        public Color background_colour { get; private set; }
        public Color foreground_colour { get; private set; }
        public Color link_colour { get; private set; }
        public Color tip_colour { get; private set; }
        public Color bible_text_colour { get; private set; }

        public static Dictionary<int, UserColourTheme> colour_themes = new Dictionary<int, UserColourTheme>();

        static UserColourTheme()
        {
            XDocument xmlDoc = XDocument.Load(THEME_FILE_NAME);
            parseThemes(xmlDoc);
        }

        //parse xDoc and generate menu definition
        private static void parseThemes(XDocument xdoc) 
        {
            var themes = xdoc.Descendants("Theme");
            foreach (var theme in themes)
            {
                string id = theme.Attribute("theme_id").Value;
                string name = theme.Attribute("name").Value;
                string bg_colour = theme.Attribute("background_colour").Value;
                string fg_colour = theme.Attribute("foreground_colour").Value;
                string l_colour  = theme.Attribute("link_colour").Value;
                string t_colour  = theme.Attribute("tip_colour").Value;
                string bt_colour = theme.Attribute("bible_text_colour").Value;

                Color background_colour = System.Drawing.ColorTranslator.FromHtml(bg_colour);
                Color forefround_colour = System.Drawing.ColorTranslator.FromHtml(fg_colour);
                Color link_colour = System.Drawing.ColorTranslator.FromHtml(l_colour);
                Color tip_colour = System.Drawing.ColorTranslator.FromHtml(t_colour);
                Color bible_text_colour = System.Drawing.ColorTranslator.FromHtml(bt_colour);
                
                int t_id_int = Int32.Parse(id);
                colour_themes.Add(
                    t_id_int,
                    new UserColourTheme(
                        t_id_int,
                        name,
                        background_colour,
                        forefround_colour,
                        link_colour,
                        tip_colour,
                        bible_text_colour));
            }
        }

        public UserColourTheme(int theme)
        {
            this.colour_theme = theme;
        }

        public UserColourTheme(
            int theme,
            String theme_name,
            Color background,
            Color foreground,
            Color link,
            Color tip,
            Color bible_text)
        {
            this.colour_theme = theme;
            this.theme_name = theme_name;
            this.background_colour = background;
            this.foreground_colour = foreground;
            this.link_colour = link;
            this.tip_colour = tip;
            this.bible_text_colour = bible_text;
        }

        public static Dictionary<int, UserColourTheme> getColourThemes()
        {
            return colour_themes;
        }

        public static bool isColourThemeValid(int colour_theme)
        {
            if (colour_themes.ContainsKey(colour_theme))
                return true;
            else
                return false;
        }

        public String getThemeName()
        {
            return theme_name;
        }

        public static UserColourTheme getColourTheme(int colour_theme)
        {
            if (colour_themes.ContainsKey(colour_theme))
                return colour_themes[colour_theme];

            return null;
        }

        public bool isSpecialTheme()
        {
            if (colour_theme == NO_THEME)
                return true;

            return false;
        }

        public Color getBackGroundColour()
        {
            return  background_colour;
        }

        public Color getForeGroundColour()
        {
            return foreground_colour;
        }

        public Color getLinkColour()
        {
            return link_colour;
        }

        public Color getTipTextColour()
        {
            return tip_colour;
        }

        public Color getBibleTextColour()
        {
            return bible_text_colour;
        }

        int IComparable.CompareTo(Object obj)
        {
            if (obj == null)
                return -1;
            else
            {
                UserColourTheme uct = (UserColourTheme)obj;
                if (this.colour_theme < uct.colour_theme)
                    return -1;
                if (this.colour_theme == uct.colour_theme)
                    return 0;
                else
                    return 1;
            }
        }


        public const int NO_THEME = 0;
        public const String THEME_FILE_NAME = "ColourThemes.xml";

    }
}
