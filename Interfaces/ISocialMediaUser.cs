namespace Mafia_panel.Interfaces;

public interface ISocialMediaUser
{
	string Name { get;}
	long Id { get;}
	void SendMessage(string message);
}
