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

namespace MxitTestApp
{
    class TaggedVersesScreenOutputAdapter : DynScreenOutputAdapter
    {

        public override void addLinksToMessageFromList(
        UserSession us,
        List<MenuOptionItem> list,
        MessageToSend ms)
        {
            int count = (us.current_menu_page * PAGE_ITEM_COUNT) + 1;

            int starting_index = us.current_menu_page * PAGE_ITEM_COUNT;
            ms.AppendLine();

            for (int i = starting_index;
                i < list.Count && i < starting_index + PAGE_ITEM_COUNT;
                i++)
            {
                TaggedVerseMenuOptionItem an_option = (TaggedVerseMenuOptionItem) list[i];
                ms.Append(createMessageLink(MENU_LINK_NAME, count + ") ", an_option.link_val));
                VerseTag verse_tag = an_option.verse_tag;
                String start_verse = verse_tag.start_verse;
                String end_verse = verse_tag.end_verse;

                /*if (verse_tag.datetime != null && verse_tag.datetime != DateTime.MinValue && !"".Equals(verse_tag.datetime))
                {
                    date_tagged = verse_tag.datetime.ToString("dd/MM/yy");
                }
                else
                {
                    date_tagged = "";
                }*/

                bool is_liked = verse_tag.isLikedByUser(us.user_profile.id);
                int like_count = verse_tag.getLikeCount();
                String like_string;
                if (like_count == 0)
                {
                    like_string = "";
                }
                else if (is_liked && like_count == 1)
                {
                    like_string = "(you like this)";
                }
                else if (!is_liked && like_count == 1)
                {
                    like_string = "(1 person likes this)";
                }
                else if (is_liked && like_count == 2)
                {
                    like_string = "(you and " + (like_count-1) + " other person like this)";
                }
                else if (is_liked && like_count > 2)
                {
                    like_string = "(you and " + (like_count-1) + " other people like this)";
                }
                else if (!is_liked && like_count > 1)
                {
                    like_string = "("+like_count + " people likes this)";
                }
                else
                {
                    like_string = "";
                }
                String rel_date = DateUtils.RelativeDate(verse_tag.datetime);

                Verse s_v = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), start_verse);
                Verse e_v;
                if (s_v != null)
                {
                    if (end_verse == null || "".Equals(end_verse) || end_verse.Equals(start_verse))
                        e_v = null;
                    else if ("NULL".Equals(end_verse))
                        e_v = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(s_v);
                    else
                        e_v = Verse_Handler.getStartingVerse(us.user_profile.getDefaultTranslationId(), end_verse);
                    String verse_summ = BibleContainer.getSummaryOfVerse(s_v, 7);
                    String user_name = UserNameManager.getInstance().getUserName(verse_tag.user_id);
                    String verse_ref = BibleHelper.getVerseSectionReferenceWithoutTranslation(s_v, e_v);
                    if (start_verse == null || end_verse == null)
                    {
                        ms.Append("N/A");
                    }
                    else
                    {
                        ms.Append(verse_ref,TextMarkup.Bold); 
                        ms.Append(" ("); 
                        ms.Append(rel_date,TextMarkup.Italics); 
                        ms.Append(") - " + verse_summ + "... ");
                        ms.Append(like_string, TextMarkup.Bold);
                        if (!is_liked)
                        {
                            ms.Append(createMessageLink(MENU_LINK_NAME, " [+]", LIKE_TAG + verse_tag.id));
                        }
                        ms.AppendLine("");
                        ms.Append("Tagged by: "); 
                        ms.AppendLine(user_name,TextMarkup.Bold);

                        //ms.Append(like_string);
                    }
                }
                else
                {
                    ms.Append("Currently not available in your chosen translation (this could be due to a S/W bug).");
                }
                ms.Append("\r\n");
                count++;
            }
            appendRefreshLink(ms);
        }

        protected override void appendPaginatedLinksForMenu(UserSession us, MessageToSend ms, List<MenuOptionItem> dyn_options)
        {
            appendPaginateLinks(us, ms, dyn_options.Count, PAGE_ITEM_COUNT);
        }

        public const String LIKE_TAG = "LIKE_";
        public const int PAGE_ITEM_COUNT = 10;
    }
}
