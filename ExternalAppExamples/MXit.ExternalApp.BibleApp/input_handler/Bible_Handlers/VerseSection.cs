using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class VerseSection
    {
        public Verse start_verse { get; private set; }
        public Verse end_verse { get; private set; }
        public Boolean span_multiple_chapters { get; private set; }
        public VerseSection(Verse start_verse, Verse end_verse) 
        {
            if (start_verse == null)
            {
                throw new XInvalidVerseSection(ERROR_MESSAGE_INVALID_START_VERSE);
            }
            int start_verse_id = start_verse.verse_id;
            int start_chapter_id = start_verse.chapter.chapter_id;
          
            if (end_verse != null)
            {
                int end_verse_id = end_verse.verse_id;
                int end_chapter_id = end_verse.chapter.chapter_id;
                if (start_verse.book.name != end_verse.book.name)
                    throw new XInvalidVerseSection(ERROR_MESSAGE_DIFFERENT_BOOKS);
                if (start_chapter_id == end_chapter_id)
                {
                    span_multiple_chapters = false;
                    if (end_verse_id - start_verse_id > MAX_SECTION_VERSES)
                        throw new XInvalidVerseSection(ERROR_MESSAGE_MAX_VERSES);
                    else if (end_verse_id - start_verse_id < 0)
                        throw new XInvalidVerseSection(ERROR_MESSAGE_END_BEFORE_START);
                }
                else //if start chapter is not equal to end chapter. 
                {
                    span_multiple_chapters = true;
                    if (end_chapter_id - start_chapter_id < 0)
                        throw new XInvalidVerseSection(ERROR_MESSAGE_END_BEFORE_START);

                    //now count how many verse chosen in total. 
                    int verse_count = start_verse.chapter.getNumVersesInChapter() - start_verse_id;
                    Chapter next_chapter = start_verse.chapter.next_chapter;
                    int safety_count = 30;
                    do
                    {
                        if (next_chapter == end_verse.chapter)
                        {
                            verse_count += end_verse_id;
                        }
                        else
                        {
                            verse_count += next_chapter.getNumVersesInChapter();
                        }
                        safety_count--;
                        if (safety_count == 0)
                            throw new XInvalidVerseSection("Something went wrong in checking verse selection. Please check that your request is in the correct format and that you have not requested more than 30 verses. Read the help for more information.");
                    } while (next_chapter != end_verse.chapter);
                    if (verse_count > MAX_SECTION_VERSES)
                    {
                        throw new XInvalidVerseSection(ERROR_MESSAGE_MAX_VERSES);
                    }
                }
            }
            this.start_verse = start_verse;
            this.end_verse = end_verse;
        }

        public const int MAX_SECTION_VERSES = 30;
        public const string ERROR_MESSAGE_END_BEFORE_START 
            = "The starting verse has to be before the ending verse in the selection.";
        public const string ERROR_MESSAGE_DIFFERENT_BOOKS
            = "The start and end verses you chose has to be from the same Book.";
        public const string ERROR_MESSAGE_MAX_VERSES
            = "Unfortunately you can't request more than 30 verses at a time.";
        public const string ERROR_MESSAGE_INVALID_START_VERSE
            = "Invalid start verse requested.";
        
    }
}
