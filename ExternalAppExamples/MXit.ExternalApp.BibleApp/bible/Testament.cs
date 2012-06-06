using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class Testament
    {
        
        public int testament_id { get; private set; }
        public string testament_name { get; private set; }
        //must be careful when getting the hashtable since this class is now mutable
        public Hashtable books { get; private set; }
        //watch out when getting the Bible since it may not be loaded completely yet.
        public Bible bible { get; private set; }
        public Translation translation { get; private set; }
        int book_count=0;

        public Testament(int testament_id, string testament_name)
        {
            this.testament_id = testament_id;
            this.testament_name = testament_name;
            books = new Hashtable();
        }

        public void addBook(ref Book book)
        {
            book_count++;
            books.Add(book.name.ToUpper(), book);
        }

        public Book getBook(string name)
        {
            return (Book)books[name.ToUpper()];
        }

        public int getBookCount()
        {
            return book_count;
        }

        public void setBible(Bible aBible)
        {
            this.bible = bible;
        }

        public void setTranslation(Translation trans)
        {
            this.translation = trans;
        }

        public const int OLD_TESTAMENT = 0;
        public const int NEW_TESTAMENT = 1;

        public const string OLD_TESTAMENT_NAME = "Old Testament";
        public const string NEW_TESTAMENT_NAME = "New Testament";
    }
}
