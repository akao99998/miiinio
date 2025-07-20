using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CancelAllNotificationCommand : Command
	{
		[Inject]
		public INotificationService service { get; set; }

		public override void Execute()
		{
			service.CancelAllNotifications();
		}
	}
}
