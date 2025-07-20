using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateDoobersFromGiftBoxCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CreateDoobersFromGiftBoxCommand") as IKampaiLogger;

		[Inject]
		public IPartyService partyService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject NamedCharacterManager { get; set; }

		[Inject]
		public StartPartyFavorAnimationSignal startPartyFavorSignal { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			int currentPartyIndex = minionPartyInstance.CurrentPartyIndex;
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			TransactionDefinition transaction;
			if (partyService.IsInspirationParty(quantity, currentPartyIndex))
			{
				transaction = RewardUtil.GetRewardTransaction(definitionService, playerService, quantity);
			}
			else
			{
				transaction = RewardUtil.GetPartyTransaction(definitionService, playerService, quantity);
				startPartyFavorSignal.Dispatch();
			}
			NamedCharacterObject namedCharacterObject = NamedCharacterManager.GetComponent<NamedCharacterManagerView>().Get(78);
			Vector3 position = namedCharacterObject.gameObject.transform.position;
			position.y += 1f;
			List<RewardQuantity> rewardQuantityFromTransaction = RewardUtil.GetRewardQuantityFromTransaction(transaction, definitionService, playerService);
			foreach (RewardQuantity item in rewardQuantityFromTransaction)
			{
				if (item.IsReward)
				{
					switch (item.ID)
					{
					case 0:
						spawnDooberSignal.Dispatch(position, DestinationType.GRIND, 0, true);
						break;
					case 1:
						spawnDooberSignal.Dispatch(position, DestinationType.PREMIUM, 1, true);
						break;
					}
				}
			}
			if (guestService.PartyShouldProduceBuff())
			{
				BuffDefinition recentBuffDefinition = guestService.GetRecentBuffDefinition(true);
				if (recentBuffDefinition != null)
				{
					spawnDooberSignal.Dispatch(position, DestinationType.BUFF, recentBuffDefinition.ID, true);
				}
				else
				{
					logger.Fatal(FatalCode.BS_NULL_BUFF_DEFINITION);
				}
			}
		}
	}
}
