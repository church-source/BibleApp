using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class TranslationOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();

        private String target_page = "";
        static ArrayList tran_list = new ArrayList();
        static TranslationOptionSet(){
            tran_list = BibleContainer.getTranslations();
        }

        public TranslationOptionSet(String target_page) 
        {
            this.target_page = target_page;
            init();
        }

        public override void init()
        {
            Translation t;
            if (tran_list != null)
            {
                for (int i = 0; i < tran_list.Count; i++)
                {
                    t = ((Bible)tran_list[i]).translation;
                    list.Add(
                        new MenuOptionItem(
                            (i+1).ToString(),
                            (t.translation_id).ToString(),
                            target_page,
                            t.name));
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
