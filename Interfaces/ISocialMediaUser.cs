using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_panel.Interfaces
{
	public interface ISocialMediaUser
	{
		string Name { get;}
		long Id { get;}
		void SendMessage(string message);
	}
}
