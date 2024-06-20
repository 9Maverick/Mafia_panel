using System.Threading.Tasks;

namespace Mafia_panel.Interfaces;

public interface ISocialMediaProvider
{
	/// <summary>
	/// Defines whenever this social media turned on
	/// </summary>
	bool IsActive { get; set; }
	/// <summary>
	/// Sends message to Log channel or/and to Game master if able
	/// </summary>
	/// <param name="message">message to send</param>
	public Task SendLog(string message);
	/// <summary>
	/// Sends message to Chat if able
	/// </summary>
	/// <param name="message">message to send</param>
	public Task SendToChat(string message);
}
