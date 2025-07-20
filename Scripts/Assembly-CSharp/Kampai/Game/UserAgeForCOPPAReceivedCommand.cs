using System.Collections;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class UserAgeForCOPPAReceivedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UserAgeForCOPPAReceivedCommand") as IKampaiLogger;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public Tuple<int, int> birthdateYearMonth { get; set; }

		[Inject]
		public CoppaCompletedSignal coppaCompletedSignal { get; set; }

		[Inject]
		public SocialInitAllServicesSignal socialInitSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public LoadMarketplaceOverridesSignal loadMarketPlaceOverridesSignal { get; set; }

		[Inject]
		public LoadMTXStoreSignal loadMTXStoreSignal { get; set; }

		[Inject]
		public LoadServerSalesSignal loadServerSalesSignal { get; set; }

		public override void Execute()
		{
			int item = birthdateYearMonth.Item1;
			int item2 = birthdateYearMonth.Item2;
			logger.Debug("User age for COPPA has been received: birthdate {0}-{1}", item, item2);
			coppaCompletedSignal.Dispatch();
			loadMarketPlaceOverridesSignal.Dispatch();
			loadMTXStoreSignal.Dispatch();
			loadServerSalesSignal.Dispatch();
			routineRunner.StartCoroutine(SocialInit());
			routineRunner.StartCoroutine(MarketplaceSlotsInit());
		}

		private IEnumerator SocialInit()
		{
			yield return new WaitForSeconds(1f);
			if (userSessionService.UserSession != null && !string.IsNullOrEmpty(userSessionService.UserSession.SessionID))
			{
				socialInitSignal.Dispatch();
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "User Session was never initilized so social services will not be initialized");
			}
		}

		private IEnumerator MarketplaceSlotsInit()
		{
			yield return new WaitForSeconds(1f);
			gameContext.injectionBinder.GetInstance<InitializeMarketplaceSlotsSignal>().Dispatch();
		}
	}
}
