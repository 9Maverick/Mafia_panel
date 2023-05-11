using Discord;
using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_panel.Models.SocialMedia.Discord
{
	public class DiscordUser : ViewModelBase, ISocialMediaUser
	{
		IUser _user;
		string _name;
		public string Name => _name;
		long _id;
		public long Id => _id;
		public DiscordUser(IUser user)
		{
			_user = user;
			_id = (long)user.Id;
			_name = user.Username;
		}

		public void SendMessage(string message) => _user.SendMessageAsync(message);
	}
}
