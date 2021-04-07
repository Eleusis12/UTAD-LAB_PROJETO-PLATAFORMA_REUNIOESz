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

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime SubscriptionDateStart { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

		public DateTime SubscriptionDateEnd { get; set; }

		[NotMapped]
		[Display(Name = "Dias de Subscri��o")]
		public int SubscriptionDays { get; set; }

		[Display(Name = "Nome da entidade/empresa")]

		public string Name { get; set; }

		[StringLength(1000)]
		[Display(Name = "Descri��o")]
		public string Description { get; set; }

		[StringLength(20)]
		[Display(Name = "Telefone")]
		public string PhoneNumber { get; set; }

		[Display(Name = "Nome do Respons�vel")]
		public string ManagerName { get; set; }

		[Display(Name = "Estado da subscri��o: Aprovado/Por Aprovar")]
		public bool StatusEntity { get; set; }

		[Display(Name = "N�mero m�ximo de utilizadores oferecidos no plano de subscri��o")]
		public int MaxUsers { get; set; }

		[StringLength(8)]
		[RegularExpression(@"^\d{4}(-\d{3})$", ErrorMessage = "Must be a valid Postal Code")]
		[Display(Name = "C�digo Postal")]
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