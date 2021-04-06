using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeePoint.Models
{
    public class Entity
    {
        public int EntityID { get; set; }
        public int NIF { get; set; }
        public string Name { get; set; }

        //An entity can be created withou a manager already specified. This can be done later by the admin
        [ForeignKey("User")]
        public int? Manager { get; set; }

        public RegisteredUser User { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
    }
}