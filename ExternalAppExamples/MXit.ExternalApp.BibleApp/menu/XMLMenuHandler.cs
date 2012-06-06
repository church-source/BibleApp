using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MxitTestApp
{
    class XMLMenuHandler
    {
        List<MenuPage> mp;
        //takes on file name of menu definition
        public XMLMenuHandler(String file_name)
        {
            XDocument xmlDoc = XDocument.Load(file_name);
            mp = new List<MenuPage>();
            parseXml(xmlDoc);
        }

        //parse xDoc and generate menu definition
        private int parseXml(XDocument xdoc)
        {
            /*var xml_root = xdoc.Element("xml");
            //only one main element for menu definition
            var mxit_root = xml_root.Element("MxitApp");*/
            var menu_items = xdoc.Descendants("Menu");
            foreach (var menu_item in menu_items)
            {
                
                string id = menu_item.Attribute("id").Value;
                Console.WriteLine("Loading menu with id: " + id);
                string input_handler = menu_item.Attribute("input_handler").Value;
                string screen_adapter = menu_item.Attribute("screen_adapter").Value;
                string help_page_id = "";
                if(menu_item.Attribute("help_page") != null)
                    help_page_id = menu_item.Attribute("help_page").Value;

                string back_link_enabled = "";
                string main_link_enabled = "";

                bool is_back_link_enabled = true;
                bool is_main_link_enabled = true;

                if(menu_item.Attribute("back_link_enabled") != null)
                    back_link_enabled = menu_item.Attribute("back_link_enabled").Value;

                if (back_link_enabled != null && back_link_enabled.ToUpper() == "FALSE")
                {
                    is_back_link_enabled = false; //only if set to false do we make it false explicitly
                }

                if (menu_item.Attribute("main_link_enabled") != null)
                    main_link_enabled = menu_item.Attribute("main_link_enabled").Value;

                if (main_link_enabled != null && main_link_enabled.ToUpper() == "FALSE")
                {
                    is_main_link_enabled = false; //only if set to false do we make it false explicitly
                }

                string title = menu_item.Element("Title").Value;
                string message = menu_item.Element("Message").Value;
                if (menu_item.Attribute("type").Value.Equals("std_page")) //TODO: make this constant
                {

                    var options = menu_item.Descendants("Option");
                    List<MenuOptionItem> mois = new List<MenuOptionItem>();
                    string option_id;
                    string link_val;
                    string select_action;
                    string display_text;
                    foreach (var option in options)
                    {
                        option_id = option.Attribute("id").Value;
                        link_val = option.Attribute("link_val").Value;
                        select_action = option.Attribute("select_action").Value;
                        display_text = option.Value;
                        mois.Add(new MenuOptionItem(option_id, link_val, select_action, display_text));
                    }
                    OptionMenuPage omp = new OptionMenuPage(
                                                       id,
                                                       title,
                                                       message,
                                                       screen_adapter,
                                                       input_handler,
                                                       help_page_id,
                                                       mois);
                    omp.setBackLinkEnabled(is_back_link_enabled);
                    omp.setMainLinkEnabled(is_main_link_enabled);
                    mp.Add(omp);
                }
                if (menu_item.Attribute("type").Value.Equals("verse_select_page")) //TODO: make this constant
                {

                    var options = menu_item.Descendants("Option");
                    List<MenuOptionItem> mois = new List<MenuOptionItem>();
                    string option_id;
                    string link_val;
                    string select_action;
                    string display_text;
                    foreach (var option in options)
                    {
                        option_id = option.Attribute("id").Value;
                        link_val = option.Attribute("link_val").Value;
                        select_action = option.Attribute("select_action").Value;
                        display_text = option.Value;
                        mois.Add(new MenuOptionItem(option_id, link_val, select_action, display_text));
                    }

                    var inputs = menu_item.Descendants("Input");
                    MenuInputItem mis = null;
                    string input_id;
                    string target_page;

                    //should always only be one 
                    foreach (var input in inputs)
                    {
                        input_id = input.Attribute("id").Value;
                        target_page = input.Attribute("target_page").Value;
                        display_text = input.Value;
                        mis = new MenuInputItem(input_id, target_page, display_text);
                    }
                    VerseMenuPage vmp = new VerseMenuPage(
                        id, 
                        title, 
                        message, 
                        screen_adapter, 
                        input_handler, 
                        help_page_id,
                        mois, 
                        mis);
                    vmp.setBackLinkEnabled(is_back_link_enabled);
                    vmp.setMainLinkEnabled(is_main_link_enabled);
                    mp.Add(vmp);
                }
                if (menu_item.Attribute("type").Value.Equals("dyn_page")) //TODO: make this constant
                {
                    string output_var = menu_item.Attribute("output_var").Value;
                    var options = menu_item.Descendants("Option");
                    List<MenuOptionItem> mois = new List<MenuOptionItem>();
                    string option_id;
                    string link_val;
                    string select_action;
                    string display_text;

                    foreach (var option in options)
                    {
                        option_id = option.Attribute("id").Value;
                        link_val = option.Attribute("link_val").Value;
                        select_action = option.Attribute("select_action").Value;
                        display_text = option.Value;
                        mois.Add(new MenuOptionItem(option_id, link_val, select_action, display_text));
                    }

                    var inputs = menu_item.Descendants("DynamicList");
                    AMenuDynamicOptionSet lg = null;
                    //string input_id;
                    string target_page;
                    string list_generator;
                    String extra_commands = "";
                    //should always only be one 
                    foreach (var input in inputs)
                    {
                        list_generator = input.Attribute("list_generator").Value;
                        target_page = input.Attribute("target_page").Value;
                        if (input.Attribute("extra_commands") != null)
                            extra_commands = input.Attribute("extra_commands").Value;
                        lg = DynListGeneratorFactory.getDynamicListGenerator(
                            list_generator,
                            target_page);
                        lg.setExtraCommandString(extra_commands);
                        var children = inputs.Descendants("EmptyListMessage");
                        //there should only be one, so fix this. 
                        foreach (var child in children)
                        {
                            lg.setListEmptyMessage(child.Value);
                        }
                    }
                    DynMenuPage dmp = new DynMenuPage(
                                            id, 
                                            title,
                                            message, 
                                            screen_adapter, 
                                            input_handler, 
                                            help_page_id,
                                            mois, 
                                            lg, 
                                            output_var);
                    dmp.setBackLinkEnabled(is_back_link_enabled);
                    dmp.setMainLinkEnabled(is_main_link_enabled);
                    mp.Add(dmp);
                }

            }

            /* var menu_items = from menu in xdoc.Descendants("Menu")
                              select new
                              {
                                  Title = menu.Element("Title").Value,
                                  Message = menu.Element("Message").Value
                              };*/

            return 0;
        }

        public List<MenuPage> getMenuPages()
        {
            return this.mp;
        }

    }
}
