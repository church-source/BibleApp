using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class VerseEmotionTagAlreadyPresentException : Exception
    {
        public VerseEmotionTagAlreadyPresentException(String test) : base(test)
        {

        }
    }
}
