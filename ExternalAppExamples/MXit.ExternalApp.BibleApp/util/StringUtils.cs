using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class StringUtils
    {
        public static String getTextSummary(String text, int n)
        {
            if(text!= null || text!="")
            {
                return GetFirstNWords(text, n);
            }       
            return text;
        }

        public static string GetFirstNWords(string text, int maxWordCount)
        {
            int wordCounter = 0;
            int stringIndex = 0;
            char[] delimiters = new[] { '\n', ' ', ',', '.' };

            while (wordCounter < maxWordCount)
            {
                stringIndex = text.IndexOfAny(delimiters, stringIndex + 1);
                if (stringIndex == -1)
                    return text;

                ++wordCounter;
            }

            return text.Substring(0, stringIndex);
        }
    }
}
