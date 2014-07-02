using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;
namespace MxitTestApp
{
    abstract class AScreenOutputAdapter : IScreenOutputAdapter
    {



        public abstract MessageToSend getOutputScreenMessage(
            UserSession us, 
            MenuPage mp,
            MessageToSend ms,
            InputHandlerResult ihr);

        public string getOutputScreen(MenuPage mp)
        {
            if (!mp.GetType().FullName.Equals("MxitTestApp.OptionMenuPage"))//TODO: Should be constant
                throw new Exception("Invalid menu page passed into getScreen method ");

            OptionMenuPage omp = (OptionMenuPage)mp;
            string output = "";
            output += omp.title + "\r\n";
            output += omp.message + "\r\n";
            List<MenuOptionItem> options = omp.options;
            foreach (MenuOptionItem option in options)
            {
                output += option.menu_option_id + ") " + option.display_text + "\r\n";
            }
            output += "\r\n Main Menu | Back \r\n";
            return output;
        }

        //this is sort of hack, but the message functions should always have user session as first variable. 
        public String getTranslationName(UserSession us, string trans_id)
        {
            int tran_id = Int32.Parse(us.user_profile.getDefaultTranslationId());
            return BibleContainer.getTranslationFullName(tran_id);
        }

        //this is sort of hack, but the message functions should always have user session as first variable. 
        public String getUserName(UserSession us, string dummy)
        {
            return us.user_profile.user_profile_custom.user_name;
        }


        //this is sort of hack, but the message functions should always have user session as first variable. 
        public String getTestamentName(UserSession us, string testament_id)
        {
            if (testament_id == "0")
                return "Old Testament";
            else if (testament_id == "1")
                return "New Testament";
            else
                return "";
        }

        public String getEmotion(UserSession us, string emo_id)
        {
            return VerseTagManager.getInstance().getEmotionFromEmotionID(Int32.Parse(emo_id));
        }


        //this is sort of hack, but the message functions should always have user session as first variable. 
        public String getCategoryName(UserSession us, string category_id)
        {
            return BibleTopicManager.getInstance().getCategory(Int32.Parse(category_id)).name;
        }

        //this is sort of hack, but the message functions should always have user session as first variable. 
        public String getThemeName(UserSession us, string dummy)
        {
            UserColourTheme uct = UserColourTheme.getColourTheme(us.user_profile.user_profile_custom.colour_theme);
            if (uct == null)
                return "No Theme";
            return uct.getThemeName();
        }

       
        public String getSearchObject(UserSession us, string dummy)
        {
           

            String search_book = us.getVariable(SearchHandler.BOOK_SEARCH_VAR_NAME);
            if (search_book != null)
            {
               return search_book;
            }

            String search_testament = us.getVariable(SearchTestamentHandler.SEARCH_TESTAMENT_VAR_NAME);
            if (search_testament != null)
            {
                return "the " + getTestamentName(us, search_testament);
            }

            return SEARCH_BIBLE_DISPLAY;


        }

        
        //this is sort of hack, but the message functions should always have user session as first variable. 
        public String getShortCodesStringList(UserSession us, string full_book_name)
        {
            return BibleHelper.getShortCodeStringList(full_book_name);
        }

        /*replace variables with session variables*/
        protected string parseMessage(UserSession us, string message)
        {
            int b_index = message.IndexOf('[');
            if (b_index != -1)
            {
                int e_index = message.IndexOf(']');
                string variable_name = message.Substring(b_index+1, e_index - b_index - 1);
                //we have to call the function on the variable to get the desired message
                if (variable_name.Contains(':'))
                {
                    String[] method_and_var = variable_name.Split(':');
                    if (method_and_var.Count() != 2)
                        throw new Exception("INVALID MESSAGE VARIABLE NAME [" + variable_name + "] DEFINED");

                    String methodName = method_and_var[0];
                    MethodInfo magicMethod = typeof(AScreenOutputAdapter).GetMethod(methodName);
                    String current_trans = us.getVariable(method_and_var[1]);
                    object stringVal = magicMethod.Invoke(this, new object[] { us, current_trans });
                    message = parseMessage(us, message.Replace('[' + variable_name + ']', (String)stringVal));
                }
                else
                {
                    message = parseMessage(us, message.Replace('[' + variable_name + ']', us.getVariable(variable_name)));
                }
            }
            else
            {
                return message;
            }
            return message;
        }

        public IMessageElement createMessageLink(string name, string display, string reply)
        {
            IMessageElement link = MessageBuilder.Elements.CreateLink(name,          // Optional
                                                          display,             // Compulsory
                                                          null,  // Optional
                                                          reply);        // Optional 
            return link;
        }

        public virtual void addLinksToMessageFromList(
            UserSession us,
            List<MenuOptionItem> list,
            MessageToSend ms)
        {
            int count = (us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT) + 1;

            int starting_index = us.current_menu_page * MenuDefinition.PAGE_ITEM_COUNT;
            MenuOptionItem an_option;
            for (int i = starting_index;
                i < list.Count && i < starting_index + MenuDefinition.PAGE_ITEM_COUNT;
                i++)
            {
                an_option = list.ElementAt(i);
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", an_option.link_val));
                ms.Append(an_option.display_text + "\r\n");
                count++;
            }
        }


        public virtual void addLinksToMessageFromList(
            UserSession us,
            List<MenuOptionItem> list,
            MessageToSend ms,
            int count_per_page)
        {
            int count = (us.current_menu_page * count_per_page) + 1;

            int starting_index = us.current_menu_page * count_per_page;
            MenuOptionItem an_option;
            for (int i = starting_index;
                i < list.Count && i < starting_index + count_per_page;
                i++)
            {
                an_option = list.ElementAt(i);
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", an_option.link_val));
                ms.Append(an_option.display_text + "\r\n");
                count++;
            }
        }

        /*adds refresh link on new line*/
        public void appendRefreshLink(MessageToSend ms)
        {
            ms.Append(createMessageLink(MENU_LINK_NAME, "Refresh", "Refresh"));
            ms.AppendLine();
        }

        //this adds pagination links depending on the count passed into it and the current page the user
        //is on
        public void appendPaginateLinks(
            UserSession us,
            MessageToSend ms,
            int count)
        {
            int cur_page_limit = 0;
            if (!us.current_menu_loc.Equals(MenuDefinition.ROOT_MENU_ID))
            {
                cur_page_limit = MenuDefinition.PAGE_ITEM_COUNT * (us.current_menu_page+1);

                if ((cur_page_limit > MenuDefinition.PAGE_ITEM_COUNT))
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Previous", "PREV"));

                if ((cur_page_limit > MenuDefinition.PAGE_ITEM_COUNT)
                    && (cur_page_limit < count))
                ms.Append(" | ");

                if ((cur_page_limit < count))
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Next", "NEXT"));

                if (count > MenuDefinition.PAGE_ITEM_COUNT)
                    ms.Append("\r\n");
            }
        }

        //this adds pagination links depending on the count passed into it and the current page the user
        //is on
        public virtual void appendPaginateLinks(
            UserSession us,
            MessageToSend ms,
            int count,
            int count_per_page)
        {
            int cur_page_limit = 0;
            int num_pages = 0;
            if (!us.current_menu_loc.Equals(MenuDefinition.ROOT_MENU_ID))
            {
                cur_page_limit = count_per_page * (us.current_menu_page + 1);
                int rem = count % count_per_page;
                num_pages = count / count_per_page ;
                if(rem > 0)
                    num_pages += 1;
                if ((cur_page_limit > count_per_page))
                {
                    if (num_pages > 2 && us.current_menu_page > 0)
                    {
                        ms.Append(createMessageLink(MENU_LINK_NAME, "First", AInputHandler.FIRST_PAGE));
                        ms.Append(" | ");
                    }
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Previous", AInputHandler.PREV_PAGE));
                }
                if ((cur_page_limit > count_per_page)
                    && (cur_page_limit < count))
                ms.Append(" | ");

                if ((cur_page_limit < count))
                {
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Next", AInputHandler.NEXT_PAGE));
                    if (num_pages > 2 && us.current_menu_page < num_pages - 1)
                    {
                        ms.Append(" | ");
                        ms.Append(createMessageLink(MENU_LINK_NAME, "Last", AInputHandler.LAST_PAGE + "_" + (num_pages-1)));
                    }
                }
                if (count > count_per_page)
                    ms.Append("\r\n");
            }
        }

        public void appendBackMainLinks(UserSession us, MessageToSend ms)
        {
            if (!us.current_menu_loc.Equals(MenuDefinition.ROOT_MENU_ID))
            {
                MenuManager mm = MenuManager.getInstance();
                MenuPage mp;
                mp = mm.menu_def.getMenuPage(us.current_menu_loc);
                int count_links = 0;
                
                if (mp.isBackLinkEnabled())
                    count_links++;
                if (mp.isMainLinkEnabled())
                    count_links++;
                if (mp.hasHelpPage())
                    count_links++;

                if (mp.isBackLinkEnabled())
                {
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Back", "BACK"));
                    if (mp.isMainLinkEnabled() || mp.hasHelpPage())
                        ms.Append(" | ");
                }
                if (mp.isMainLinkEnabled())
                {
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Main", "MAIN"));
                }
                if (mp.hasHelpPage())
                {
                    if (mp.isMainLinkEnabled())
                        ms.Append(" | ");
                    ms.Append(createMessageLink(MENU_LINK_NAME, "Help", "HELP"));
                }
            }
        }

        public static void appendInitialMessageConfig(UserProfile up, MessageToSend message)
        {
            IMessageElement chatScreenConfig;
            IClientColors clientColors = MessageBuilder.Elements.CreateClientColors(); //Create the colour scheme you want to 
            UserColourTheme uct = UserColourTheme.getColourTheme(up.user_profile_custom.colour_theme);
            if (uct != null)
            {
                clientColors[ClientColorType.Background] = uct.getBackGroundColour();
                clientColors[ClientColorType.Text] = uct.getForeGroundColour();
                clientColors[ClientColorType.Link] = uct.getLinkColour();
                chatScreenConfig = MessageBuilder.Elements.CreateChatScreenConfig(
                    ChatScreenBehaviourType.ShowProgress |
                    ChatScreenBehaviourType.NoPrefix,
                    clientColors);
            }
            else
            {

                //clientColors[ClientColorType.Background] = Color.Empty; //System.Drawing.ColorTranslator.FromHtml("#??????");
                //clientColors[ClientColorType.Text] = Color.Empty; //System.Drawing.ColorTranslator.FromHtml("#??????");
                //clientColors[ClientColorType.Link] = Color.Empty; //System.Drawing.ColorTranslator.FromHtml("#??????");
                chatScreenConfig = MessageBuilder.Elements.CreateChatScreenConfig(
                    ChatScreenBehaviourType.ShowProgress |
                    ChatScreenBehaviourType.NoPrefix);

            }
            
            message.Append(chatScreenConfig);
            /*else
            {
                chatScreenConfig = MessageBuilder.Elements.CreateChatScreenConfig(
                    ChatScreenBehaviourType.ShowProgress |
                    ChatScreenBehaviourType.NoPrefix);
            }
            ms.Append(chatScreenConfig);*/
        }

        public void appendMessageConfig(bool clear_screen,  MessageToSend ms)
        {
            /*IMessageElement chatScreenConfig;
            if (clear_screen)
            {
                chatScreenConfig = MessageBuilder.Elements.CreateChatScreenConfig(
                    ChatScreenBehaviourType.Clear |
                    ChatScreenBehaviourType.ShowProgress |
                    ChatScreenBehaviourType.NoPrefix);
            }
            else
            {
                chatScreenConfig = MessageBuilder.Elements.CreateChatScreenConfig(
                    ChatScreenBehaviourType.ShowProgress |
                    ChatScreenBehaviourType.NoPrefix);
            }
            ms.Append(chatScreenConfig);*/
        }

        public Boolean displayMessage(UserSession us, MessageToSend ms, InputHandlerResult ihr)
        {
            if (ihr.action == InputHandlerResult.DISPLAY_MESSAGE)
            {
                ms.Append(MessageBuilder.Elements.CreateClearScreen());
                ms.Append(ihr.message + "\r\n");
                appendBackMainLinks(us, ms);
                appendMessageConfig(true, ms);
                us.setVariable(AInputHandler.DISPLAY_MESSAGE , "Message sent");//you must be sure to remove this from hash table in handler. 
                return true;
            }
            return false;
        }

        public const string MENU_LINK_NAME = "menu_link";
        public const string SEARCH_BIBLE_DISPLAY = "the Bible";
        public const string COLOUR_CHANGED = "COLOUR_CHANGED";
    }
}
