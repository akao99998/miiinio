using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Splash;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class UpsellService : IUpsellService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UpsellService") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public StartSaleSignal startSaleSignal { get; set; }

		[Inject]
		public EndSaleSignal endSaleSignal { get; set; }

		public QuantityItem GetItemDef(int index, SalePackDefinition saleDefinition)
		{
			if (saleDefinition == null || saleDefinition.TransactionDefinition == null || saleDefinition.TransactionDefinition.Outputs == null)
			{
				return null;
			}
			return GetItemDef(index, saleDefinition.TransactionDefinition.Outputs);
		}

		public QuantityItem GetItemDef(int index, IList<QuantityItem> outputs)
		{
			if (outputs == null || outputs.Count == 0)
			{
				return null;
			}
			int num = 0;
			for (int i = 0; i < outputs.Count; i++)
			{
				QuantityItem quantityItem = outputs[i];
				if (quantityItem == null)
				{
					continue;
				}
				Definition definition = definitionService.Get(quantityItem.ID);
				if (definition != null && definition.ID != 2 && !(definition is UnlockDefinition))
				{
					if (num == index)
					{
						return quantityItem;
					}
					num++;
				}
			}
			return null;
		}

		public uint SumOutput(IList<QuantityItem> inputs, int id)
		{
			if (inputs == null)
			{
				return 0u;
			}
			uint num = 0u;
			for (int i = 0; i < inputs.Count; i++)
			{
				QuantityItem quantityItem = inputs[i];
				if (quantityItem != null && quantityItem.ID == id)
				{
					num += quantityItem.Quantity;
				}
			}
			return num;
		}

		public int GetInputsCount(SalePackDefinition saleDefinition)
		{
			if (saleDefinition == null || saleDefinition.TransactionDefinition == null || saleDefinition.TransactionDefinition.Inputs == null)
			{
				return 0;
			}
			return saleDefinition.TransactionDefinition.Inputs.Count;
		}

		public void SetupFreeItemButton(LocalizeView localizeView, ButtonView buttonView, string buttonLocKey, RuntimeAnimatorController freeCollectButtonAnimator)
		{
			if (freeCollectButtonAnimator == null)
			{
				freeCollectButtonAnimator = KampaiResources.Load<RuntimeAnimatorController>("asm_buttonClick_Tertiary");
			}
			if (buttonView == null || localizeView == null)
			{
				if (buttonView == null)
				{
					logger.Error("Purchase Currency Button GameObject is null");
				}
				if (localizeView == null)
				{
					logger.Error("Purchase Button Cost GameObject is null");
				}
			}
			else
			{
				localizeView.LocKey = buttonLocKey;
				Animator component = buttonView.GetComponent<Animator>();
				if (component == null || freeCollectButtonAnimator == null)
				{
					logger.Error(string.Format("Button animator is null: {0}", buttonView));
				}
				else
				{
					component.runtimeAnimatorController = freeCollectButtonAnimator;
				}
			}
		}

		public Sale GetSaleInstanceFromID(List<Sale> playerSales, int id)
		{
			if (playerSales == null)
			{
				return null;
			}
			Sale sale = null;
			for (int i = 0; i < playerSales.Count; i++)
			{
				sale = playerSales[i];
				if (sale != null && sale.Definition.ID == id)
				{
					return sale;
				}
			}
			return null;
		}

		public void SetupBuyButton(IList<QuantityItem> inputs, KampaiImage inputItemIcon, Definition definition)
		{
			if (inputs == null || inputs.Count == 0)
			{
				return;
			}
			QuantityItem quantityItem = inputs[0];
			if (quantityItem == null)
			{
				logger.Error(string.Format("Item input is null for sale pack definition: {0}", definition));
				return;
			}
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(quantityItem.ID);
			if (itemDefinition == null)
			{
				logger.Error(string.Format("Item input is not an item definition for definition: {0}", definition));
				return;
			}
			inputItemIcon.gameObject.SetActive(true);
			UIUtils.SetItemIcon(inputItemIcon, itemDefinition);
		}

		public SalePackDefinition GetSalePackDefinition(int upsellDefId)
		{
			if (definitionService == null)
			{
				return null;
			}
			SalePackDefinition definition = null;
			definitionService.TryGet<SalePackDefinition>(upsellDefId, out definition);
			return definition;
		}

		public void ScheduleSales(IList<Sale> saleInstances)
		{
			if (saleInstances != null && saleInstances.Count != 0)
			{
				for (int i = 0; i < saleInstances.Count; i++)
				{
					ScheduleSale(saleInstances[i]);
				}
			}
		}

		public Sale AddNewSaleInstance(SalePackDefinition salePackDefinition)
		{
			if (salePackDefinition == null)
			{
				return null;
			}
			logger.Debug("Adding a new Sale to the player Data. ID = " + salePackDefinition.ID);
			Sale sale = new Sale(salePackDefinition);
			sale.Purchased = false;
			playerService.Add(sale);
			return sale;
		}

		public void ScheduleSale(Sale saleInstance)
		{
			if (saleInstance == null)
			{
				logger.Error("Unable to find sale instance");
			}
			else
			{
				if (saleInstance.Finished || !IsSaleAssetsFullyDownloaded(saleInstance.Definition))
				{
					return;
				}
				if (!saleInstance.Started)
				{
					if (CanSaleProceed(saleInstance))
					{
						int eventTime = saleInstance.Definition.UTCStartDate - timeService.CurrentTime();
						timeEventService.AddEvent(saleInstance.ID, timeService.CurrentTime(), eventTime, startSaleSignal);
					}
				}
				else if (!timeEventService.HasEventID(saleInstance.ID) && saleInstance.Definition.Duration > 0)
				{
					timeEventService.AddEvent(saleInstance.ID, saleInstance.UTCUserStartTime, saleInstance.Definition.Duration, endSaleSignal);
				}
			}
		}

		public bool IsSaleUpsellInstance(Sale saleInstance)
		{
			return saleInstance != null && !saleInstance.Finished && !PackUtil.HasPurchasedEnough(saleInstance.Definition, playerService) && IsSaleAssetsFullyDownloaded(saleInstance.Definition);
		}

		private bool IsSaleAssetsFullyDownloaded(SalePackDefinition saleDefinition)
		{
			if (saleDefinition.TransactionDefinition == null || saleDefinition.TransactionDefinition.ID == 0)
			{
				return true;
			}
			TransactionDefinition transactionDefinition = saleDefinition.TransactionDefinition.ToDefinition();
			if (transactionDefinition != null)
			{
				foreach (QuantityItem output in transactionDefinition.Outputs)
				{
					if (output.ID == 1 || output.ID == 0 || output.ID == 9 || output.ID == 6 || output.ID == 700 || output.ID == 701 || output.ID == 702)
					{
						continue;
					}
					Definition definition = definitionService.Get<Definition>(output.ID);
					if (!(definition is MinionDefinition))
					{
						ItemDefinition itemDefinition = definition as ItemDefinition;
						if (itemDefinition != null && !string.IsNullOrEmpty(itemDefinition.Image) && !string.IsNullOrEmpty(itemDefinition.Mask) && (!KampaiResources.FileDownloaded(itemDefinition.Image, dlcModel) || !KampaiResources.FileDownloaded(itemDefinition.Mask, dlcModel)))
						{
							return false;
						}
						BuildingDefinition buildingDefinition = definition as BuildingDefinition;
						if (buildingDefinition != null && !KampaiResources.FileDownloaded(buildingDefinition.Prefab, dlcModel))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool CanSaleProceed(Sale saleInstance)
		{
			SalePackDefinition definition = saleInstance.Definition;
			int unlockLevel = definition.UnlockLevel;
			SalePackType type = definition.Type;
			if (unlockLevel > 0)
			{
				if (!questService.IsQuestCompleted(definition.UnlockQuestId) || playerService.GetQuantity(StaticItem.LEVEL_ID) < unlockLevel)
				{
					return false;
				}
			}
			else if (type != SalePackType.Upsell && playerService.GetHighestFtueCompleted() < 999999)
			{
				return false;
			}
			return true;
		}
	}
}
