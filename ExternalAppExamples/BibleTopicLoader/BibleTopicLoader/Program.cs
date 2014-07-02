using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BibleTopicLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            BibleTopicLoader.loadTopics("C:\\my stuff\\");
            Console.ReadKey();
        }
    }
}
