using System.Collections;
using Kampai.Game.Trigger;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateTSMCharacterWithTriggerCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CreateNamedCharacterViewSignal createNamedCharacterViewSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public TSMStartIntroAnimation tsmStartIntroAnimation { get; set; }

		[Inject]
		public ITriggerService triggerService { get; set; }

		[Inject]
		public IRoutineRunner RoutineRunner { get; set; }

		public override void Execute()
		{
			TSMCharacter byInstanceId = playerService.GetByInstanceId<TSMCharacter>(301);
			if (byInstanceId != null)
			{
				if (byInstanceId.Created)
				{
					tsmStartIntroAnimation.Dispatch(IsTreasureChestIntro());
				}
				else
				{
					RoutineRunner.StartCoroutine(ShowTSMCharacter(byInstanceId));
				}
			}
		}

		private IEnumerator ShowTSMCharacter(TSMCharacter tsmCharacter)
		{
			yield return null;
			if (tsmCharacter != null && !tsmCharacter.Created)
			{
				createNamedCharacterViewSignal.Dispatch(tsmCharacter);
				createWayFinderSignal.Dispatch(new WayFinderSettings(301));
				RoutineRunner.StartCoroutine(StartTSMIntroAnimation());
			}
		}

		private IEnumerator StartTSMIntroAnimation()
		{
			yield return new WaitForEndOfFrame();
			tsmStartIntroAnimation.Dispatch(IsTreasureChestIntro());
		}

		private bool IsTreasureChestIntro()
		{
			TriggerInstance activeTrigger = triggerService.ActiveTrigger;
			if (activeTrigger != null)
			{
				return activeTrigger.Definition.TreasureIntro;
			}
			return false;
		}
	}
}
