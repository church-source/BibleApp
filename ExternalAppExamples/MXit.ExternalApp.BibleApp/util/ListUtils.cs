using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class ListUtils
    {
        //try and use generics here rather
        public static List<VerseMessageParticipant> convertVMPDictionaryToList(Dictionary<long, VerseMessageParticipant> dictionary)
        {
            List<KeyValuePair<long, VerseMessageParticipant>> list;
            list = dictionary.ToList();
            List<VerseMessageParticipant> verse_message_participant = new List<VerseMessageParticipant>();
            foreach (var vmp_kvp in list)
            {
                VerseMessageParticipant vmp = vmp_kvp.Value;
                verse_message_participant.Add(vmp);
            }
            return verse_message_participant;
        }

        //try and use generics here rather
        public static List<UserColourTheme> convertColourThemeDictionaryToList(Dictionary<int, UserColourTheme> dictionary)
        {
            List<KeyValuePair<int, UserColourTheme>> list;
            list = dictionary.ToList();
            List<UserColourTheme> final_list = new List<UserColourTheme>();
            foreach (var obj in list)
            {
                UserColourTheme o = obj.Value;
                final_list.Add(o);
            }
            return final_list;
        }

        //try and use generics here rather
        public static List<Category> convertTopicCategoryDictionaryToList(Dictionary<int, Category> dictionary)
        {
            List<KeyValuePair<int, Category>> list;
            list = dictionary.ToList();
            List<Category> final_list = new List<Category>();
            foreach (var obj in list)
            {
                Category o = obj.Value;
                final_list.Add(o);
            }
            return final_list;
        }


        public static List<Object> convertDictionaryToList(Dictionary<int, Object> dictionary)
        {
            List<KeyValuePair<int, Object>> list;
            list = dictionary.ToList();
            List<Object> final_list = new List<Object>();
            foreach (var obj in list)
            {
                Object o = obj.Value;
                final_list.Add(o);
            }
            return final_list;
        }


        public static List<VerseTagEmotion> convertEmotionDictionaryToList(Dictionary<int, VerseTagEmotion> dictionary)
        {
            List<KeyValuePair<int, VerseTagEmotion>> list;
            list = dictionary.ToList();
            List<VerseTagEmotion> final_list = new List<VerseTagEmotion>();
            foreach (var obj in list)
            {
                VerseTagEmotion o = obj.Value;
                final_list.Add(o);
            }
            return final_list;
        }
    }
}
