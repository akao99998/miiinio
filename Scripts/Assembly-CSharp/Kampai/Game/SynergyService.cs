using System.Collections.Generic;

namespace Kampai.Game
{
	public class SynergyService : ISynergyService
	{
		private NimbleBridge_NotificationListener synergyEnvListener;

		[Inject]
		public UpdateUserSignal updateUserSignal { get; set; }

		public string userID
		{
			get
			{
				if (NimbleBridge_SynergyEnvironment.GetComponent().IsDataAvailable())
				{
					return NimbleBridge_SynergyEnvironment.GetComponent().GetSynergyId();
				}
				if (synergyEnvListener == null)
				{
					synergyEnvListener = new NimbleBridge_NotificationListener(OnNimbleSynergyEnvNotification);
					NimbleBridge_NotificationCenter.RegisterListener("nimble.environment.notification.startup_requests_finished", synergyEnvListener);
				}
				return string.Empty;
			}
		}

		private void OnNimbleSynergyEnvNotification(string name, Dictionary<string, object> userData, NimbleBridge_NotificationListener listener)
		{
			if (name == "nimble.environment.notification.startup_requests_finished")
			{
				string synergyId = NimbleBridge_SynergyEnvironment.GetComponent().GetSynergyId();
				updateUserSignal.Dispatch(synergyId);
			}
		}
	}
}
