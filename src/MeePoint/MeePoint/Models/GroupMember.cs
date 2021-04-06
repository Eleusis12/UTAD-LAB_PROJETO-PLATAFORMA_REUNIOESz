using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeePoint.Models
{
    public class GroupMember
    {
        public int GroupID { get; set; }
        public int UserID { get; set; }

        public virtual Group Group { get; set; }
        public virtual User User { get; set; }

        [Required]
        public string Role { get; set; }
    }
}