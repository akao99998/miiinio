using System.Collections;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class AllMinionLoadedCommand : Command
	{
		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public ITriggerService triggerService { get; set; }

		[Inject]
		public OrderBoardUpdateTicketOnBoardSignal updateTicketOnBoardSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public SetPartyStatesSignal setPartyStatesSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PartySignal partySignal { get; set; }

		[Inject]
		public UpdatePrestigeListSignal updatePrestigeList { get; set; }

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		[Inject]
		public RemoveInvalidOneOffCraftableSignal removeInvalidOneOffCraftableSignal { get; set; }

		[Inject]
		public SetupSpecialEventCharacterSignal setupSpecialEventCharacterSignal { get; set; }

		[Inject]
		public RestoreMasterPlanSignal restoreMasterPlanSignal { get; set; }

		public override void Execute()
		{
			routineRunner.StartCoroutine(FinishStartup());
		}

		private IEnumerator FinishStartup()
		{
			while (coroutineProgressMonitor.HasRunningTasks())
			{
				yield return null;
			}
			TimeProfiler.StartSection("AllMinionLoadedCommand");
			TimeProfiler.StartSection("Quests");
			questService.Initialize();
			TimeProfiler.EndSection("Quests");
			TimeProfiler.StartSection("Triggers");
			triggerService.Initialize();
			TimeProfiler.EndSection("Triggers");
			yield return null;
			TimeProfiler.StartSection("Set up special event characters");
			setupSpecialEventCharacterSignal.Dispatch(-1);
			TimeProfiler.EndSection("update prestige");
			TimeProfiler.StartSection("update prestige");
			updatePrestigeList.Dispatch();
			TimeProfiler.EndSection("update prestige");
			TimeProfiler.StartSection("select level band");
			updateTicketOnBoardSignal.Dispatch();
			TimeProfiler.EndSection("select level band");
			TimeProfiler.StartSection("time service");
			telemetryService.GameStarted();
			TimeProfiler.EndSection("time service");
			TimeProfiler.StartSection("party states");
			setPartyStatesSignal.Dispatch(true);
			TimeProfiler.EndSection("party states");
			TimeProfiler.StartSection("remove one off craftable");
			removeInvalidOneOffCraftableSignal.Dispatch();
			TimeProfiler.EndSection("remove one off craftable");
			TimeProfiler.StartSection("Restore Master Plans");
			restoreMasterPlanSignal.Dispatch();
			TimeProfiler.EndSection("Restore Master Plans");
			TimeProfiler.EndSection("AllMinionLoadedCommand");
			routineRunner.StartTimer("StartingPartyOver", (float)definitionService.Get<MinionPartyDefinition>(80000).GetPartyDuration(true) + 1f, delegate
			{
				partySignal.Dispatch();
			});
		}
	}
}
