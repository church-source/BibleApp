using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace MxitTestApp
{
    class VerseEmotionOptionSet : AMenuDynamicOptionSet
    {
        List<MenuOptionItem> list = new List<MenuOptionItem>();
        private String target_page = "";


        public VerseEmotionOptionSet(String target_page)
        {
            this.target_page = target_page;
        }

        public override void init()
        {

        }

        public override List<MenuOptionItem> getOptionList(UserSession us)
        {
            List<VerseTagEmotion> emotion_list = VerseTagManager.getInstance().getListOfEmotions();

            if (emotion_list != null)
            {
                
                List<MenuOptionItem> final_list = new List<MenuOptionItem>();
                foreach(var emotion in emotion_list)
                {
                    int tag_count = VerseTagManager.getInstance().getVerseTagCountOnEmotion(emotion.id);
                    String tag_m = "";
                    if(tag_count > 0)
                        tag_m += " (" + tag_count + " tag";
                    if (tag_count > 1)
                        tag_m += "s";
                    if (tag_count > 0)
                        tag_m += ")";

                     MenuOptionItem m_o = new MenuOptionItem(
                                          "*",
                                          (emotion.id).ToString(),
                                          target_page,
                                          emotion.emotion + tag_m);
                            final_list.Add(m_o);
               }

               return final_list;
            }
 
            return null;
        }
        //too many returns in this method 
        public override string parseInput(String input, UserSession us)
        {
            return input;
        }
    }
}
