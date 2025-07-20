using System.Collections;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupEndMinionPartyTimerCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupEndMinionPartyTimerCommand") as IKampaiLogger;

		private IEnumerator EndPartyCoroutine;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public EndMinionPartySignal endMinionPartySignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IGuestOfHonorService guestOfHonorService { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			int partyDuration = minionPartyInstance.Definition.GetPartyDuration(guestOfHonorService.PartyShouldProduceBuff());
			EndPartyCoroutine = EndMinionParty(partyDuration);
			routineRunner.StartCoroutine(EndPartyCoroutine);
			endMinionPartySignal.AddListener(EndMinionPartyThroughSkip);
		}

		private void EndMinionPartyThroughSkip(bool isSkipping)
		{
			endMinionPartySignal.RemoveListener(EndMinionPartyThroughSkip);
			routineRunner.StopCoroutine(EndPartyCoroutine);
			EndPartyCoroutine = null;
		}

		private IEnumerator EndMinionParty(int sequenceLength)
		{
			yield return new WaitForSeconds(sequenceLength);
			endMinionPartySignal.RemoveListener(EndMinionPartyThroughSkip);
			logger.Debug("Dispatching the EndMinionPartySignal at the end of the party coroutine");
			endMinionPartySignal.Dispatch(false);
			EndPartyCoroutine = null;
		}
	}
}
