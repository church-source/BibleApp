using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    class XInvalidVerseSection : Exception
    {
        public XInvalidVerseSection (String message) : base(message)
        {
        }
    }
}
