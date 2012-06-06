using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    //this class represents a relationship between two users. 
    public class FriendRelation
    {
        public long friendship_id { get; private set; }
        public long id_a { get; private set; }
        public long id_b { get; private set; }
        public int status { get; private set; }
        public DateTime datetime_created { get; private set; }
        public DateTime datetime_accepted { get; private set; }

        public FriendRelation(
            long friendship_id,
            long id_a,
            long id_b,
            int status,
            DateTime datetime,
            DateTime datetime_accepted)
        {
            this.friendship_id = friendship_id;
            this.id_a = id_a;
            this.id_b = id_b;
            this.status = status;
            this.datetime_created = datetime_created;
            this.datetime_accepted = datetime_accepted;
        }

        public void setStatus(int status)
        {
            this.status = status;
        }

        public void setDateTimeAccepted(DateTime dt)
        {
            this.datetime_accepted = dt;
        }


        public const int STATUS_PENDING = 0x00;
        public const int STATUS_ACCEPTED = 0x01;
        public const int STATUS_REJECTED = 0x02;
        public const int STATUS_BLOCKED_A = 0x03;
        public const int STATUS_BLOCKED_B = 0x04;

        //these are not represented in the database but gives a simplified view of the blocked status relative to the current user
        public const int FRIEND_BLOCKED_BY_YOU = 98;
        public const int FRIEND_BLOCKED_YOU = 99;

    }
}
