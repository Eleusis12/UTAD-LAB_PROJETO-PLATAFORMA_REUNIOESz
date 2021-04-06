using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeePoint.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Username { get; set; }

        public virtual ICollection<GroupMember> Groups { get; set; }
        public virtual ICollection<Convocation> Convocations { get; set; }
    }
}