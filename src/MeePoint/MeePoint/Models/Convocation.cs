using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeePoint.Models
{
	public class Convocation
	{
		public int MeetingID { get; set; }

		public int UserID { get; set; }

		public virtual Meeting Meeting { get; set; }

		public virtual RegisteredUser User { get; set; }

		public bool Answer { get; set; }
		public string Justification { get; set; }
	}
}