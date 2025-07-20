using System.Collections.Generic;

namespace Kampai.Game
{
	public class HelpTipTrackingService : IHelpTipTrackingService
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		public void TrackHelpTipShown(int tipDefinitionId)
		{
			playerService.TrackHelpTipShown(tipDefinitionId, timeService.CurrentTime());
		}

		public bool GetHelpTipWasShown(int tipDefinitionId)
		{
			return GetHelpTipShowCount(tipDefinitionId) > 0;
		}

		public int GetHelpTipShowCount(int tipDefinitionId)
		{
			List<Player.HelpTipTrackingItem> helpTipsTrackingData = playerService.helpTipsTrackingData;
			if (helpTipsTrackingData == null)
			{
				return 0;
			}
			for (int i = 0; i < helpTipsTrackingData.Count; i++)
			{
				if (helpTipsTrackingData[i].tipDifinitionId == tipDefinitionId)
				{
					return helpTipsTrackingData[i].showsCount;
				}
			}
			return 0;
		}

		public int GetSecondsSinceHelpTipShown(int tipDefinitionId)
		{
			List<Player.HelpTipTrackingItem> helpTipsTrackingData = playerService.helpTipsTrackingData;
			if (helpTipsTrackingData == null)
			{
				return int.MaxValue;
			}
			for (int i = 0; i < helpTipsTrackingData.Count; i++)
			{
				if (helpTipsTrackingData[i].tipDifinitionId == tipDefinitionId)
				{
					return helpTipsTrackingData[i].lastShownTime - timeService.CurrentTime();
				}
			}
			return int.MaxValue;
		}
	}
}
