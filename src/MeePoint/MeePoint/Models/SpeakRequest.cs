using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.Models
{
    public class SpeakRequest
    {
        public int SpeakRequestID { get; set; }

        public RegisteredUser WhoRequested { get; set; }

        public int MeetingID { get; set; }

        public Meeting Meeting { get; set; }

    }
}
