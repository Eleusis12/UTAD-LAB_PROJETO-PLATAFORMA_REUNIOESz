using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeePoint.Models
{
	public class Meeting
	{
		public int MeetingID { get; set; }

		public int GroupID { get; set; }

		[Display(Name = "Nome da Reunião")]
		public string Name { get; set; }

		public Group Group { get; set; }

		public int Quorum { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime MeetingDate { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime MeetingStarted { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime MeetingEnded { get; set; }

		public float ExpectedDuration { get; set; }

		public bool Recurring { get; set; }

		public virtual ICollection<Convocation> Convocations { get; set; }
		public virtual ICollection<Document> Documents { get; set; }
	}
}