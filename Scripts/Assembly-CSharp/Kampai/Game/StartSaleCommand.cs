using System;
using System.Collections;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartSaleCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StartSaleCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public EndSaleSignal endSaleSignal { get; set; }

		[Inject]
		public UpdateSaleBadgeSignal updateSaleBadgeSignal { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpSellModalSignal { get; set; }

		[Inject]
		public int instanceId { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public override void Execute()
		{
			logger.Debug("Sale Started: {0}", instanceId);
			Sale item = playerService.GetByInstanceId<Sale>(instanceId);
			if (item == null)
			{
				return;
			}
			bool started = item.Started;
			item.Started = true;
			if (!item.Viewed)
			{
				updateSaleBadgeSignal.Dispatch();
			}
			item.UTCUserStartTime = timeService.CurrentTime();
			int duration = item.Definition.Duration;
			if (duration > 0)
			{
				timeEventService.AddEvent(instanceId, item.UTCUserStartTime, duration, endSaleSignal);
			}
			if (item.Definition.Type == SalePackType.Upsell && !started)
			{
				routineRunner.StartCoroutine(WaitAFrame(delegate
				{
					openUpSellModalSignal.Dispatch(item.Definition, "Automatic", false);
				}));
			}
		}

		private IEnumerator WaitAFrame(Action a)
		{
			yield return null;
			a();
		}
	}
}
