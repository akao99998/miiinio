using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class QuantityItemTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1181;
			}
		}

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.QuantityItem;
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			if (base.transaction == null || context == null)
			{
				return;
			}
			ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
			bool flag = BuildingPacksHelper.UpdateTransactionUnlocksList(base.transaction, injectionBinder);
			TransactionDefinition def = base.transaction.ToDefinition();
			bool flag2 = HandleLandExpansionUI(def, injectionBinder);
			IPlayerService instance = injectionBinder.GetInstance<IPlayerService>();
			uint storageCount = instance.GetStorageCount();
			instance.RunEntireTransaction(def, TransactionTarget.AUTOMATIC, null, new TransactionArg
			{
				InstanceId = 301,
				Source = "TSMTrigger"
			});
			if (storageCount != instance.GetStorageCount())
			{
				injectionBinder.GetInstance<SetStorageCapacitySignal>().Dispatch();
			}
			if (flag)
			{
				injectionBinder.GetInstance<UpdateUIButtonsSignal>().Dispatch(false);
				if (!flag2)
				{
					OpenBuildingStoreUI(base.transaction, injectionBinder);
				}
			}
		}

		private void OpenBuildingStoreUI(TransactionInstance transaction, ICrossContextInjectionBinder binder)
		{
			IDefinitionService instance = binder.GetInstance<IDefinitionService>();
			foreach (QuantityItem output in transaction.Outputs)
			{
				BuildingDefinition definition;
				if (instance.TryGet<BuildingDefinition>(output.ID, out definition))
				{
					binder.GetInstance<UpdateUIButtonsSignal>().Dispatch(false);
					binder.GetInstance<OpenStoreHighlightItemSignal>().Dispatch(definition.ID, true);
					break;
				}
			}
		}

		private bool HandleLandExpansionUI(TransactionDefinition def, ICrossContextInjectionBinder binder)
		{
			IDefinitionService instance = binder.GetInstance<IDefinitionService>();
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("QuantityItemTriggerRewardDefinition") as IKampaiLogger;
			List<LandExpansionConfig> list = new List<LandExpansionConfig>();
			PurchasedLandExpansion byInstanceId = binder.GetInstance<IPlayerService>().GetByInstanceId<PurchasedLandExpansion>(354);
			LandExpansionConfig landExpansionConfig = null;
			foreach (QuantityItem output in def.Outputs)
			{
				if (output.Quantity < 1)
				{
					continue;
				}
				int iD = output.ID;
				LandExpansionConfig definition;
				if (!instance.TryGet<LandExpansionConfig>(iD, out definition))
				{
					continue;
				}
				if (byInstanceId.PurchasedExpansions.Contains(definition.expansionId))
				{
					kampaiLogger.Info("Already owned: {0}\n", iD);
					continue;
				}
				if (landExpansionConfig == null)
				{
					landExpansionConfig = definition;
				}
				list.Add(definition);
			}
			if (list.Count > 0)
			{
				RunExpansionUI(list, landExpansionConfig, binder, instance);
			}
			return list.Count > 0;
		}

		private void RunExpansionUI(List<LandExpansionConfig> expansions, LandExpansionConfig focusConfig, ICrossContextInjectionBinder binder, IDefinitionService definitionService)
		{
			if (focusConfig != null)
			{
				ILandExpansionService instance = binder.GetInstance<ILandExpansionService>();
				Vector3 type = default(Vector3);
				if (instance.HasForSaleSign(focusConfig.expansionId))
				{
					GameObject forSaleSign = instance.GetForSaleSign(focusConfig.expansionId);
					type = forSaleSign.transform.position;
				}
				else
				{
					foreach (LandExpansionDefinition item in definitionService.GetAll<LandExpansionDefinition>())
					{
						if (item.ExpansionID == focusConfig.expansionId)
						{
							type = item.Location.ToVector3();
						}
					}
				}
				binder.GetInstance<HighlightLandExpansionSignal>().Dispatch(focusConfig.expansionId, true);
				binder.GetInstance<CameraAutoMoveSignal>().Dispatch(type, null, new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), false);
			}
			foreach (LandExpansionConfig expansion in expansions)
			{
				binder.GetInstance<PurchaseLandExpansionSignal>().Dispatch(expansion.expansionId, focusConfig == expansion);
			}
		}
	}
}
