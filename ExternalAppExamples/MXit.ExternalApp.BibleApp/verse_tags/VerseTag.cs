using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class VerseTag : IComparable
    {
        public long id { get; set; }
        public int emotion_id { get; private set; }
        public long user_id { get; private set; }
        public DateTime datetime { get; private set; }
        public String start_verse { get; private set; }
        public String end_verse { get; private set; }
        public String description { get; private set; }
        private Dictionary<long, VerseTagEmotionLike> likes;
        private Object thisLock = new Object();

        public VerseTag(
            long id,
            int emotion_id,
            long user_id,
            DateTime datetime,
            String start_verse,
            String end_verse,
            String description)
        {
            this.id = id;
            this.emotion_id = emotion_id;
            this.user_id = user_id;
            this.datetime = datetime;
            this.start_verse = start_verse;
            this.end_verse = end_verse;
            this.description = description;
            likes = new Dictionary<long, VerseTagEmotionLike>();
        }

        int IComparable.CompareTo(Object obj)
        {
            if (obj == null)
                return -1;
            else
            {
                VerseTag vt = (VerseTag)obj;
                long like_count_source = this.getLikeCount();
                long like_count_target = vt.getLikeCount();
                if (like_count_source > like_count_target)
                {
                    return -1;
                }
                if (like_count_source == like_count_target)
                {
                    if (this.datetime > vt.datetime)
                    {
                        return -1;
                    }
                    else if (this.datetime == vt.datetime)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 1;
                }
            }
        }

        public void addLikeLoadedFromDB(VerseTagEmotionLike like)
        {
            lock (thisLock)
            {
                if (like != null)
                {
                    long key = /*like.id + "|" + */ like.user_id;
                    if (!likes.ContainsKey(key))
                    {
                        this.likes.Add(key, like);
                    }
                }
            }
        }

        public void addNewLike(VerseTagEmotionLike like)
        {
            lock (thisLock)
            {
                if (like != null)
                {
                    long key = /*like.id + "|" + */ like.user_id;
                    if (!likes.ContainsKey(key))
                    {
                        this.likes.Add(key, like);
                        new TaggedVerseLikeTask(like).AddTaggedVerseLike();
                    }
                }
            }
        }

        public void unLike(VerseTagEmotionLike like)
        {
            lock (thisLock)
            {
                if (like != null)
                {
                    long key = /*like.id + "|" + */ like.user_id;
                    if (likes.ContainsKey(key))
                    {
                        this.likes.Remove(key);
                    }
                }
            }
        }

        public void unLike(long key)
        {
            lock (thisLock)
            {
                if (likes.ContainsKey(key))
                {
                    this.likes.Remove(key);
                }
                
            }
        }

        public int getLikeCount()
        {
            lock (thisLock)
            {
                return this.likes.Count();
            }
        }

        public bool isLikedByUser(long user_id)
        {
            lock (thisLock)
            {
                return (likes.ContainsKey(user_id));
            }
        }
    }


}
