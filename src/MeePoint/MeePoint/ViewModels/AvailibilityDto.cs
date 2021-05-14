using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.ViewModels
{
	public class AvailibilityDto
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Desc { get; set; }
		public string Start_Date { get; set; }
		public string End_Date { get; set; }
	}
}