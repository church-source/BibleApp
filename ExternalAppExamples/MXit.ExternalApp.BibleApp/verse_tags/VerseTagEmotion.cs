using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseTagEmotion
    {
        
        public int id { get; private set; }
        public String emotion { get; private set; }

        public VerseTagEmotion(
            int id,
            String emotion
            )
        {
            this.id = id;
            this.emotion = emotion;
        }

    }
}
