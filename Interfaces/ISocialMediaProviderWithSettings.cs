using Mafia_panel.Models.SocialMedia;
using System.Collections.ObjectModel;

namespace Mafia_panel.Interfaces;

public interface ISocialMediaProviderWithSettings : ISocialMediaProvider
{
	string Name { get; }
	ReadOnlyDictionary<string, SocialMediaSetting> Settings { get; }
}
