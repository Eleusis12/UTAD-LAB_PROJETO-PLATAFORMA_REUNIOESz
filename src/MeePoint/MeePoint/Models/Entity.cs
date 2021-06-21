using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeePoint.Models
{
	public class Entity
	{
		public int EntityID { get; set; }

		[Display(Name = "NIF")]
		public int NIF { get; set; }

		[Display(Name = "Nome da entidade/empresa")]
		public string Name { get; set; }

		[StringLength(1000)]
		[Display(Name = "Descrição")]
		public string Description { get; set; }

		[StringLength(20)]
		[Display(Name = "Telefone")]
		public string PhoneNumber { get; set; }

		[Display(Name = "Nome do Responsável")]
		public string ManagerName { get; set; }

		[Display(Name = "Estado da subscrição: Aprovado/Por Aprovar")]
		public bool StatusEntity { get; set; }

		[StringLength(8)]
		[RegularExpression(@"^\d{4}(-\d{3})$", ErrorMessage = "Introduza um código Postal válido")]
		[Display(Name = "Código Postal")]
		public string PostalCode { get; set; }

		[StringLength(600)]
		[Display(Name = "Morada")]
		public string Address { get; set; }

		//An entity can be created withou a manager already specified. This can be done later by the admin
		[ForeignKey("User")]
		public int? Manager { get; set; }

		public RegisteredUser User { get; set; }
		public virtual ICollection<Group> Groups { get; set; }
	}
}