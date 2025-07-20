using System.Collections.Generic;

namespace Kampai.UI
{
	public class CurrencyStoreLocalState
	{
		public IDictionary<int, List<int>> ItemsViewedMap;

		public CurrencyStoreLocalState()
		{
			ItemsViewedMap = new Dictionary<int, List<int>>();
		}
	}
}
