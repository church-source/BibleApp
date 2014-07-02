using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GotQuestionsLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            GotQuestionsLoader.loadTopics("C:\\Users\\rpillay\\Documents\\Visual Studio 2010\\Projects\\GotQuestionsLoader\\BibleTopicLoader\\goTQuestions\\AllQuestionsAndAnswersHTMLPlain2.xml");
            Console.ReadKey();
        }
    }
}
