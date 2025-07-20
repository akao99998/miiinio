using UnityEngine;

namespace Kampai.UI.View
{
	public class SpawnDooberModel
	{
		public const float TOKEN_DOOBER_WIDTH_OFFSET = 4f;

		public Vector3 TikiBarWorldPosition { get; set; }

		public Vector3 XPGlassPosition { get; set; }

		public Vector3 PremiumGlassPosition { get; set; }

		public Vector3 GrindGlassPosition { get; set; }

		public Vector3 StorageGlassPosition { get; set; }

		public Vector3 TokenInfoHUDPosition { get; set; }

		public Vector3 InspirationGlassPosition { get; set; }

		public Vector3 DiscoBallGlassPosition { get; set; }

		public Vector3 StoreGlassPosition { get; set; }

		public Vector3 MiscGlassPosition { get; set; }

		public int DooberCounter { get; set; }

		public int PendingPopulationDoober { get; set; }

		public bool RewardedAdDooberMode { get; set; }

		public Vector3 expScreenPosition { get; set; }

		public Vector3 premiumScreenPosition { get; set; }

		public Vector3 grindScreenPosition { get; set; }

		public Vector3 itemScreenPosition { get; set; }

		public Vector3 defaultDooberSpawnLocation { get; set; }

		public Vector3 rewardedAdDooberSpawnLocation { get; set; }

		public SpawnDooberModel()
		{
			expScreenPosition = new Vector3(0.4f, 0.6f, 0f);
			premiumScreenPosition = new Vector3(0.6f, 0.6f, 0f);
			grindScreenPosition = new Vector3(0.4f, 0.4f, 0f);
			itemScreenPosition = new Vector3(0.6f, 0.4f, 0f);
			defaultDooberSpawnLocation = new Vector3(0.5f, 0.3f, 0f);
			MiscGlassPosition = (StoreGlassPosition = (DiscoBallGlassPosition = (InspirationGlassPosition = (StorageGlassPosition = (GrindGlassPosition = (PremiumGlassPosition = (XPGlassPosition = (TikiBarWorldPosition = (rewardedAdDooberSpawnLocation = Vector3.zero)))))))));
		}
	}
}
