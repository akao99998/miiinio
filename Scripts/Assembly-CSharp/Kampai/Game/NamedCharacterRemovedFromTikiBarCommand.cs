using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class NamedCharacterRemovedFromTikiBarCommand : Command
	{
		[Inject]
		public NamedCharacterObject namedCharacterObject { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public AddStuartToStageSignal addStuartToStageSignal { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public PointBobLandExpansionSignal pointBobLandExpansionSignal { get; set; }

		[Inject]
		public StuartTunesGuitarSignal stuartTunesGuitarSignal { get; set; }

		public override void Execute()
		{
			NamedCharacter byInstanceId = playerService.GetByInstanceId<NamedCharacter>(namedCharacterObject.ID);
			NamedCharacterDefinition definition = byInstanceId.Definition;
			if (byInstanceId is StuartCharacter)
			{
				StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
				if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.IsBuildingRepaired())
				{
					Prestige firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<Prestige>(40003);
					if (firstInstanceByDefinitionId2.CurrentPrestigeLevel == 1)
					{
						base.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, BuildingZoomType.TIKIBAR, null, false));
						addStuartToStageSignal.Dispatch(StuartStageAnimationType.IDLEOFFSTAGE);
						stuartTunesGuitarSignal.Dispatch();
					}
					else
					{
						frolicSignal.Dispatch(byInstanceId.ID);
					}
				}
				else
				{
					frolicSignal.Dispatch(byInstanceId.ID);
				}
			}
			else if (byInstanceId is BobCharacter)
			{
				pointBobLandExpansionSignal.Dispatch();
			}
			else if (definition.Location != null)
			{
				Location location = definition.Location;
				namedCharacterObject.transform.position = new Vector3(location.x, 0f, location.y);
			}
		}
	}
}
