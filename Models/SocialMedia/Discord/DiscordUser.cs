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
	public class DiscordUser : NotifyPropertyChanged, ISocialMediaUser
	{
		IUser _user;
		public string Name => _user.Username;
		public long Id => (long)_user.Id;
		public DiscordUser(IUser user)
		{
			_user = user;
		}

		public void SendMessage(string message) => _user.SendMessageAsync(message);
	}
}
