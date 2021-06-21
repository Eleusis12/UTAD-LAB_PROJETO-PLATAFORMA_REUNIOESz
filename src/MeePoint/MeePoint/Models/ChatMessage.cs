using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.Models
{
    public class ChatMessage
    {
        public int MessageID { get; set; }

        public string Sender { get; set; }

        public int MeetingID { get; set; }

        public Meeting Meeting { get; set; }
        public string Text { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
