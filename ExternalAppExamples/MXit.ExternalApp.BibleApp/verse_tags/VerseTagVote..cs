using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseTagVote
    {
        
        public long id { get; private set; }
        public long emo_tag_id { get; private set; }
        public long user_id { get; private set; }
        public byte vote { get; private set; } //either 1 or -1.

        public VerseTagVote(
            long id,
            long emo_tag_id,
            long user_id,
            byte vote
            )
        {
            this.id = id;
            this.emo_tag_id = emo_tag_id;
            this.user_id = user_id;
            this.vote = vote;
        }

    }
}
