using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.UI
{
	public class BuildMenuLocalState
	{
		public IList<StoreItemType> UncheckedTabs;

		public IDictionary<StoreItemType, List<int>> NewUnlockedItemOnTabs;

		public IDictionary<StoreItemType, IDictionary<int, bool>> UncheckedInventoryItemOnTabs;

		public BuildMenuLocalState()
		{
			UncheckedTabs = new List<StoreItemType>();
			NewUnlockedItemOnTabs = new Dictionary<StoreItemType, List<int>>();
			UncheckedInventoryItemOnTabs = new Dictionary<StoreItemType, IDictionary<int, bool>>();
		}
	}
}
