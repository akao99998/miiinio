using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CancelNotificationCommand : Command
	{
		[Inject]
		public INotificationService service { get; set; }

		[Inject]
		public string type { get; set; }

		public override void Execute()
		{
			service.CancelLocalNotification(type);
		}
	}
}
