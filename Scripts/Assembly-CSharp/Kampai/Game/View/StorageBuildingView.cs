using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class StorageBuildingView : KampaiView
	{
		private StorageBuilding storageBuilding;

		private Renderer grindRewardRenderer;

		private Renderer pendingSaleRenderer;

		private Renderer repairMarketplaceRenderer;

		private Renderer marketplaceRenderer;

		public void Init(Building building)
		{
			storageBuilding = building as StorageBuilding;
			if (storageBuilding != null)
			{
				Transform transform = base.transform.Find("Unique_Storage_LOD0/Unique_Storage:Unique_Storage/Unique_Storage:unique_Storage_GrindReward_Mesh");
				if (transform != null)
				{
					grindRewardRenderer = transform.GetComponent<Renderer>();
				}
				Transform transform2 = base.transform.Find("Unique_Storage_LOD0/Unique_Storage:Unique_Storage/Unique_Storage:unique_Storage_PendingSale_Mesh");
				if (transform2 != null)
				{
					pendingSaleRenderer = transform2.GetComponent<Renderer>();
				}
				Transform transform3 = base.transform.Find("Unique_Storage_LOD0/Unique_Storage:Unique_Storage/Unique_Storage:unique_Storage_RepairState_Mesh");
				if (transform3 != null)
				{
					repairMarketplaceRenderer = transform3.GetComponent<Renderer>();
				}
				Transform transform4 = base.transform.Find("Unique_Storage_LOD0/Unique_Storage:Unique_Storage/Unique_Storage:unique_Storage_MarketPlace_Mesh");
				if (transform4 != null)
				{
					marketplaceRenderer = transform4.GetComponent<Renderer>();
				}
			}
		}

		public void SetMarketplaceEnabled(bool isEnabled)
		{
			if (repairMarketplaceRenderer != null)
			{
				repairMarketplaceRenderer.enabled = !isEnabled;
			}
			if (marketplaceRenderer != null)
			{
				marketplaceRenderer.enabled = isEnabled;
			}
		}

		public bool ToggleGrindReward(bool isEnabled)
		{
			bool result = false;
			if (grindRewardRenderer != null)
			{
				result = grindRewardRenderer.enabled != isEnabled;
				grindRewardRenderer.enabled = isEnabled;
			}
			return result;
		}

		public bool TogglePendingSale(bool isEnabled)
		{
			bool result = false;
			if (pendingSaleRenderer != null)
			{
				result = pendingSaleRenderer.enabled != isEnabled;
				pendingSaleRenderer.enabled = isEnabled;
			}
			return result;
		}
	}
}
