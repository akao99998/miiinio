using Elevation.Logging;
using Kampai.Game;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Main
{
	public class SetupDeepLinkCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("SetupDeepLinkCommand") as IKampaiLogger;

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		[Inject]
		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void Execute()
		{
			GameObject gameObject = new GameObject("DeepLink");
			gameObject.transform.parent = managers.transform;
			gameObject.SetActive(false);
			DeepLinkHandler deepLinkHandler = gameObject.AddComponent<DeepLinkHandler>();
			deepLinkHandler.logger = logger;
			deepLinkHandler.moveBuildMenuSignal = moveBuildMenuSignal;
			deepLinkHandler.showMTXStoreSignal = showMTXStoreSignal;
			deepLinkHandler.cloneUserFromEnvSignal = gameContext.injectionBinder.GetInstance<CloneUserFromEnvSignal>();
			gameObject.SetActive(true);
		}
	}
}
