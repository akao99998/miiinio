using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.Util
{
	public static class ItemUtil
	{
		public static IList<T> SortQIByQuantity<T>(IEnumerable<T> items, bool ascending = true) where T : QuantityItem
		{
			List<T> list = new List<T>(items);
			list.Sort((T p1, T p2) => (int)((!ascending) ? (p2.Quantity - p1.Quantity) : (p1.Quantity - p2.Quantity)));
			return list;
		}

		public static IList<T> SortItemsByQuantity<T>(IEnumerable<T> items, bool ascending = true) where T : Item
		{
			List<T> list = new List<T>(items);
			list.Sort((T p1, T p2) => (int)((!ascending) ? (p2.Quantity - p1.Quantity) : (p1.Quantity - p2.Quantity)));
			return list;
		}

		public static bool CompareSKU(string SKU, string ExternalIdentifier)
		{
			return SKU.Trim().ToLower().Equals(ExternalIdentifier.Trim().ToLower());
		}
	}
}
