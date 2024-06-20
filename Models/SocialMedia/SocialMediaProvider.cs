using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using Mafia_panel.Models.SocialMedia.Discord;
using Mafia_panel.Models.SocialMedia.Telegram;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Mafia_panel.Models.SocialMedia;
public class SocialMediaProvider : NotifyPropertyChanged, ISocialMediaProvider
{
    ObservableCollection<ISocialMediaProviderWithSettings> _socialMediaProviders;
    public ObservableCollection<ISocialMediaProviderWithSettings> Providers
    {
        get => _socialMediaProviders;
        set => SetValue(ref _socialMediaProviders, value);
    }
    ObservableCollection<ISocialMediaProviderWithSettings> _activeProviders;
    public ObservableCollection<ISocialMediaProviderWithSettings> ActiveProviders
    {
        get => _activeProviders;
        set => SetValue(ref _activeProviders, value);
    }
    bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetValue(ref _isActive, value);
    }

    public SocialMediaProvider(DiscordClientModel discordProvider, TelegramClientModel telegramProvider)
    {
        Providers = new ObservableCollection<ISocialMediaProviderWithSettings>
        {
            discordProvider,
            telegramProvider
        };
        discordProvider.PropertyChanged += UpdateActivity;
        UpdateActivity();
    }

    void UpdateActivity(object? sender = null, PropertyChangedEventArgs args = null)
    {
        if (args != null && !(args.PropertyName == nameof(IsActive))) return;
        IsActive = Providers.Where(provider => provider.IsActive).Any();
        ActiveProviders = new ObservableCollection<ISocialMediaProviderWithSettings>(Providers.Where(provider => provider.IsActive));
    }

    public Task SendLog(string message)
    {
        foreach (var provider in Providers.Where(provider => provider.IsActive))
        {
            provider.SendLog(message);
        }
        return Task.CompletedTask;
    }

    public Task SendToChat(string message)
    {
        foreach (var provider in Providers.Where(provider => provider.IsActive))
        {
            provider.SendToChat(message);
        }
        return Task.CompletedTask;
    }
}
