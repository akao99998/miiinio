using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class ModalSettings
	{
		public Vector2 startPosition = GameConstants.UI.DEFAULT_POPUP_ANCHOR_POINT;

		public Vector2 endPosition = GameConstants.UI.DEFAULT_POPUP_ANCHOR_POINT;

		public bool enableRushButtons = true;

		public bool enableHarvestButtons = true;

		public bool enableCallButtons = true;

		public bool enableRushThrob;

		public bool enableCallThrob;

		public bool enableCollectThrob;

		public bool enableGotoThrob;

		public bool enableDeliverThrob;

		public bool enablePurchaseButtons = true;

		public bool enableCraftingThrob;

		public bool enableTicketThrob;

		public bool enableLockedButtons = true;

		public bool enableLockedThrob;
	}
}
