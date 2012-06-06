using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class Bible : ABible
    {
        public Translation translation {get; private set;}
        public List<Testament> testaments { get; private set; }
        public Boolean is_fully_loaded {get; private set;}

        public Bible(Translation translation)
        {
            this.translation = translation;
            is_fully_loaded = false;
            loadBible();
        }

        public override void loadBible()
        {
            testaments = BibleHelper.getTestaments();
            Translation trans  = this.translation;
            for (int i = 0; i < testaments.Count; i++)
            {
                Testament test = testaments[i];
                BibleHelper.addTestamentBooks(ref test);

                Hashtable books = test.books;
                ArrayList book_list = convertHashTableToArrayList(books);
                for (int j = 0; j < book_list.Count; j++)
                {
                    Book aBook = (Book)book_list[j];
                    BibleHelper.addBookChapters(ref aBook);

                    Hashtable chapters = aBook.chapters;
                    ArrayList chapter_list = convertHashTableToArrayList(chapters);
                    for (int k = 0; k < chapter_list.Count; k++)
                    {
                        Chapter aChapter = (Chapter)chapter_list[k];
                        BibleHelper.addChapterVerses(ref aChapter, ref trans );
                    }
                }
                test.setBible(this); //this is dangerous because the Bible isnt really fully loaded at this point. 
                test.setTranslation(translation);
                is_fully_loaded = true;
            }
           /* Book b = testaments[0].getBook("Genesis");
            do
            {
                //Console.WriteLine(b.name);
                b = b.next_book;
            } while (b != null);*/
        }

        private ArrayList convertHashTableToArrayList(Hashtable hashtable)
        {
            ArrayList list = new ArrayList();
            foreach (Object o in hashtable.Values)
            {
                list.Add(o);
            }
            return list;
        }

        public override Testament getTestament(string t_name)
        {
            for (int i = 0; i < testaments.Count; i++)
            {
                if (testaments[i].testament_name == t_name)
                {
                    return testaments[i];
                }
            }
            return null;
        }


        public override Testament getTestament(int index)
        {
            if (testaments != null && index < (testaments.Count()))
            {
                return testaments[index];
            }
            else
            {
                return null;
            }
        }
    }
}
