using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeePoint.Models
{
	public class RegisteredUser
	{
		public int RegisteredUserID { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string Username { get; set; }

		public string Name { get; set; }

		[StringLength(500)]
		public string Foto { get; set; }

		public virtual ICollection<GroupMember> Groups { get; set; }
		public virtual ICollection<Convocation> Convocations { get; set; }
	}
}