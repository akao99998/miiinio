using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateMinionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CreateMinionCommand") as IKampaiLogger;

		[Inject]
		public Minion minion { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public InitCharacterObjectSignal initSignal { get; set; }

		[Inject]
		public AddMinionSignal addMinionSignal { get; set; }

		public override void Execute()
		{
			int costumeId = minion.GetCostumeId(playerService, definitionService);
			CostumeItemDefinition costumeItemDefinition = definitionService.Get<CostumeItemDefinition>(99);
			if (costumeItemDefinition == null)
			{
				logger.Fatal(FatalCode.PS_MISSING_DEFAULT_COSTUME, "ERROR: Minion costume ID: {0} - Could not create default costume!!!", costumeId);
			}
			CostumeItemDefinition costume = costumeItemDefinition;
			if (costumeId > 0 && costumeId != 99)
			{
				CostumeItemDefinition costumeItemDefinition2 = definitionService.Get<CostumeItemDefinition>(costumeId);
				if (costumeItemDefinition2 != null)
				{
					costume = costumeItemDefinition2;
				}
				else
				{
					logger.Warning("Minion costume ID: {0} is not a costume item, Reverting back to generic minion", costumeId);
				}
			}
			MinionObject minionObject = minionBuilder.BuildMinion(costume, "asm_minion_movement");
			initSignal.Dispatch(minionObject, minion);
			minionObject.transform.parent = minionManager.transform;
			minionObject.transform.position = pathFinder.RandomPosition(minion.Partying);
			addMinionSignal.Dispatch(minionObject);
			MinionPartyDefinition minionPartyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
			if (minion.IsInMinionParty)
			{
				minionObject.EnterMinionParty((Vector3)minionPartyDefinition.Center, minionPartyDefinition.PartyRadius, minionPartyDefinition.partyAnimationRestMin, minionPartyDefinition.partyAnimationRestMax);
			}
		}
	}
}
