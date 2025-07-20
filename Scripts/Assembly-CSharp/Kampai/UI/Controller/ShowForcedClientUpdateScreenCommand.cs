using Kampai.Splash;
using UnityEngine.SceneManagement;
using strange.extensions.command.impl;

namespace Kampai.UI.Controller
{
	public class ShowForcedClientUpdateScreenCommand : Command
	{
		[Inject]
		public IDownloadService downloadService { get; set; }

		public override void Execute()
		{
			if (downloadService != null)
			{
				downloadService.Shutdown();
			}
			SceneManager.LoadScene("ForcedUpgrade");
		}
	}
}
