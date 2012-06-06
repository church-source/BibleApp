using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public interface IDailyVerseObserver
    {
        void updateDailyVerse(DailyVerse daily_verse);
    }
}
