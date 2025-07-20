using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PointBobLandExpansionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PointBobLandExpansionCommand") as IKampaiLogger;

		[Inject]
		public BobPointsAtSignSignal pointAtSignSignal { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public BobReturnToTownSignal bobReturnToTown { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			BobCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<BobCharacter>(70002);
			bool flag = false;
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(firstInstanceByDefinitionId);
			if (prestigeFromMinionInstance == null)
			{
				return;
			}
			switch (prestigeFromMinionInstance.state)
			{
			case PrestigeState.Prestige:
			case PrestigeState.InQueue:
				if (prestigeFromMinionInstance.CurrentPrestigeLevel == 0)
				{
					return;
				}
				break;
			case PrestigeState.Locked:
			case PrestigeState.PreUnlocked:
			case PrestigeState.Questing:
				return;
			}
			if (playerService.HasTargetExpansion())
			{
				int targetExpansion = playerService.GetTargetExpansion();
				if (!byInstanceId.HasPurchased(targetExpansion))
				{
					logger.Info("Bob is already pointing at expansion: {0}", targetExpansion);
					PointAtExpansion(firstInstanceByDefinitionId.ID, targetExpansion);
					return;
				}
				logger.Info("Bob needs a new place to point");
				flag = true;
				playerService.ClearTargetExpansion();
			}
			IList<int> expansionIds = landExpansionConfigService.GetExpansionIds();
			List<int> list = FindAspirationalExpansions(expansionIds, byInstanceId);
			if (list.Count > 0)
			{
				int index = randomService.NextInt(list.Count);
				int num = list[index];
				playerService.SetTargetExpansion(num);
				logger.Info("Setting Bob to point at expansion: {0}", num);
			}
			if (playerService.HasTargetExpansion())
			{
				int targetExpansion2 = playerService.GetTargetExpansion();
				PointAtExpansion(firstInstanceByDefinitionId.ID, targetExpansion2);
				return;
			}
			logger.Info("No aspirational expansions for Bob to point at :-( ");
			if (flag)
			{
				bobReturnToTown.Dispatch();
			}
			frolicSignal.Dispatch(firstInstanceByDefinitionId.ID);
			removeWayFinderSignal.Dispatch(firstInstanceByDefinitionId.ID);
		}

		private List<int> FindAspirationalExpansions(IList<int> configIds, PurchasedLandExpansion purchasedLandExpansions)
		{
			List<int> list = new List<int>(8);
			foreach (int configId in configIds)
			{
				LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(configId);
				if (!purchasedLandExpansions.IsUnpurchasedAdjacentExpansion(expansionConfig.expansionId))
				{
					continue;
				}
				TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(expansionConfig.transactionId);
				foreach (QuantityItem output in transactionDefinition.Outputs)
				{
					LandExpansionConfig definition;
					if (definitionService.TryGet<LandExpansionConfig>(output.ID, out definition) && definition.containedAspirationalBuildings.Count > 0)
					{
						logger.Debug("Config: {0} Expansion: {1} Aspirational: {2}", configId, definition.expansionId, definition.containedAspirationalBuildings.Count);
						list.Add(configId);
					}
				}
			}
			return list;
		}

		private void PointAtExpansion(int bobID, int targetExpansionId)
		{
			LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(targetExpansionId);
			Vector3 type = new Vector3(expansionConfig.routingSlot.x, 0f, expansionConfig.routingSlot.y);
			pointAtSignSignal.Dispatch(type);
			createWayFinderSignal.Dispatch(new WayFinderSettings(bobID));
		}
	}
}
