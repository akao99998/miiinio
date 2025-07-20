using Kampai.Game;

namespace Kampai.UI.View
{
	public class StoreTab
	{
		public string LocalizedName { get; set; }

		public StoreItemType Type { get; set; }

		public StoreTab(string localizedName, StoreItemType type)
		{
			LocalizedName = localizedName;
			Type = type;
		}
	}
}
