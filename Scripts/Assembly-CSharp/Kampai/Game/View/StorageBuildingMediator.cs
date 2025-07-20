using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class StorageBuildingMediator : Mediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("StorageBuildingMediator") as IKampaiLogger;

		private StorageBuilding storageBuilding;

		private int trackedId;

		private SetStorageCapacitySignal setStorageCapacitySignal;

		[Inject]
		public StorageBuildingView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public UpdateMarketplaceRepairStateSignal updateRepairStateSignal { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public StartMarketplaceOnboardingSignal startMarketplaceOnboardingSignal { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public GetWayFinderSignal getWayFinderSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable UIContext { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			storageBuilding = GetStorageBuilding();
			trackedId = storageBuilding.ID;
			view.Init(storageBuilding);
			updateRepairStateSignal.AddListener(UpdateRepairState);
			updateSoldItemsSignal.AddListener(UpdateSoldSales);
			awardLevelSignal.AddListener(LeveledUp);
			loginSuccess.AddListener(OnLoginSuccess);
			view.SetMarketplaceEnabled(marketplaceService.IsUnlocked());
			view.ToggleGrindReward(marketplaceService.AreThereSoldItems());
			view.TogglePendingSale(marketplaceService.AreThereSoldItems());
			setStorageCapacitySignal = UIContext.injectionBinder.GetInstance<SetStorageCapacitySignal>();
			setStorageCapacitySignal.AddListener(OnSetStorageCapacity);
			StartCoroutine(WaitAFrame());
		}

		public override void OnRemove()
		{
			base.OnRemove();
			updateRepairStateSignal.RemoveListener(UpdateRepairState);
			updateSoldItemsSignal.RemoveListener(UpdateSoldSales);
			awardLevelSignal.RemoveListener(LeveledUp);
			loginSuccess.RemoveListener(OnLoginSuccess);
			setStorageCapacitySignal.RemoveListener(OnSetStorageCapacity);
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			OnSetStorageCapacity();
		}

		private void OnSetStorageCapacity()
		{
			bool flag = IsMissingItemToExpand();
			ToggleWayFinder(!flag, false);
		}

		private bool IsMissingItemToExpand()
		{
			int currentStorageBuildingLevel = storageBuilding.CurrentStorageBuildingLevel;
			IList<StorageUpgradeDefinition> storageUpgrades = storageBuilding.Definition.StorageUpgrades;
			if (currentStorageBuildingLevel >= storageUpgrades.Count)
			{
				return true;
			}
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(storageUpgrades[currentStorageBuildingLevel].TransactionId);
			if (transactionDefinition != null)
			{
				return playerService.IsMissingItemFromTransaction(transactionDefinition);
			}
			return true;
		}

		public void UpdateRepairState()
		{
			view.SetMarketplaceEnabled(marketplaceService.IsUnlocked());
		}

		public void UpdateSoldSales(bool showVFX)
		{
			bool flag = marketplaceService.AreThereSoldItems();
			bool flag2 = marketplaceService.AreTherePendingItems();
			bool flag3 = view.ToggleGrindReward(flag);
			bool flag4 = view.TogglePendingSale(flag2);
			ToggleWayFinder(flag, true);
			if (flag4 && !flag2 && showVFX)
			{
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(storageBuilding.ID);
				if (buildingObject != null && flag)
				{
					buildingObject.SetVFXState("PendingSale_Disappear");
				}
			}
			else if (flag3 && showVFX)
			{
				BuildingManagerView component2 = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject2 = component2.GetBuildingObject(storageBuilding.ID);
				if (flag)
				{
					buildingObject2.SetVFXState("GrindReward_Appear");
				}
				else
				{
					buildingObject2.SetVFXState("GrindReward_Disappear");
				}
			}
		}

		private void OnLoginSuccess(ISocialService socialService)
		{
			if (socialService.type == SocialServices.FACEBOOK)
			{
				UpdateSoldSales(true);
			}
		}

		private StorageBuilding GetStorageBuilding()
		{
			StorageBuildingObject component = GetComponent<StorageBuildingObject>();
			if (component != null)
			{
				return component.storageBuilding;
			}
			logger.Error("StorageBuildingMediator: could not find StorageBuilding for building " + base.gameObject.name);
			return null;
		}

		public void LeveledUp(TransactionDefinition unlockTransaction)
		{
			if (TransactionUtil.ExtractQuantityFromTransaction(unlockTransaction, 312) >= 1)
			{
				localPersistence.PutDataPlayer("MarketSurfacing", storageBuilding.ID.ToString());
				startMarketplaceOnboardingSignal.Dispatch(storageBuilding.ID);
			}
		}

		internal void ToggleWayFinder(bool enable, bool soldItemWayFinder)
		{
			getWayFinderSignal.Dispatch(trackedId, delegate(int wayFinderId, IWayFinderView wayFinderView)
			{
				if (wayFinderView == null)
				{
					if (enable)
					{
						createWayFinderSignal.Dispatch(new WayFinderSettings(trackedId));
					}
				}
				else
				{
					StorageBuildingWayFinderView storageBuildingWayFinderView = wayFinderView as StorageBuildingWayFinderView;
					if (!(storageBuildingWayFinderView == null))
					{
						if (enable)
						{
							if (soldItemWayFinder)
							{
								storageBuildingWayFinderView.SetIconToItemSold();
							}
						}
						else if (soldItemWayFinder)
						{
							if (IsMissingItemToExpand())
							{
								removeWayFinderSignal.Dispatch(wayFinderId);
							}
							else
							{
								storageBuildingWayFinderView.SetIconToDefault();
							}
						}
						else if (marketplaceService.AreThereSoldItems())
						{
							storageBuildingWayFinderView.SetIconToItemSold();
						}
						else
						{
							removeWayFinderSignal.Dispatch(wayFinderId);
						}
					}
				}
			});
		}
	}
}
