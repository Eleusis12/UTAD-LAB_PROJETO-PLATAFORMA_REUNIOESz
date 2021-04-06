using System.ComponentModel.DataAnnotations;

namespace MeePoint.Models
{
    public class Document
    {
        public int DocumentID { get; set; }

        public int MeetingID { get; set; }

        public Meeting Meeting { get; set; }
        public string DocumentPath { get; set; }
    }
}