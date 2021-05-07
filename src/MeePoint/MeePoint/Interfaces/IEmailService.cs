using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.Interfaces
{
	public interface IEmailService
	{
		void Send(string from, string to, string subject, string html);

		void SendAccountCreated(string from, string to, string entity, string accountPassword);
	}
}