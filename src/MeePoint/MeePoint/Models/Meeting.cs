using System;
using System.Collections.Generic;

namespace MeePoint.Models
{
    public class Meeting
    {
        public int MeetingID { get; set; }

        public int GroupID { get; set; }

        public Group Group { get; set; }

        public int Quorum { get; set; }

        public DateTime MeetingDate { get; set; }

        public virtual ICollection<Convocation> Convocations { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
    }
}