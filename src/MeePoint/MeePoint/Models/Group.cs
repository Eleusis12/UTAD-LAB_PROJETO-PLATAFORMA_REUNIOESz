using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeePoint.Models
{
	public class Group
	{
		public int GroupID { get; set; }
		public string Name { get; set; }

		public int EntityID { get; set; }

		public Entity Entity { get; set; }

		public enum GroupRoles
        {
			Manager,
			CoManager,
			Participant
        }

		public virtual ICollection<GroupMember> Members { get; set; }

		public virtual ICollection<Meeting> Meetings { get; set; }
	}
}