using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseTagEmotionLike
    {
        
        public long id { get; set; }
        public long emotion_tag_id { get; private set; }
        public long user_id { get; private set; }
        public DateTime datetime { get; private set; }
        
        public VerseTagEmotionLike(
            long id,
            long emotion_tag_id,
            long user_id,
            DateTime datetime
            )
        {
            this.id = id;
            this.emotion_tag_id = emotion_tag_id;
            this.user_id = user_id;
            this.datetime = datetime;
        }

    }
}
