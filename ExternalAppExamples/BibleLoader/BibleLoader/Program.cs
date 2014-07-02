using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BibleLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            //Loading existing Bibles into memory
            Console.Write("Loading Existing Bibles into memory...");
            Console.WriteLine(BibleContainer.getInstance().getBible(0).testaments[1].getBook("John").getChapter(3).getVerse(16).text);
            Console.WriteLine(BibleContainer.getInstance().getBible(1).testaments[1].getBook("John").getChapter(3).getVerse(16).text);
            Console.WriteLine("Complete");
            Console.WriteLine("Loading Net Bible into DB...");
            NETBibleLoader.loadNetBible("C:\\NetBibleLoad\\NetLoad\\NetLoad\\");
            Console.WriteLine("Complete");
            Console.Read();
        }
    }
}
