using System.Collections.Generic;

namespace Kampai.Util
{
	public static class IdentifiableUtils
	{
		public static T FindIdentifiable<T>(IEnumerable<T> bucket, int id) where T : Identifiable
		{
			if (bucket != null)
			{
				foreach (T item in bucket)
				{
					if (item.ID == id)
					{
						return item;
					}
				}
			}
			return default(T);
		}

		public static Dictionary<int, T> MapIdentifiables<T>(IEnumerable<T> bucket) where T : Identifiable
		{
			Dictionary<int, T> dictionary = new Dictionary<int, T>();
			if (bucket != null)
			{
				foreach (T item in bucket)
				{
					dictionary.Add(item.ID, item);
				}
			}
			return dictionary;
		}

		public static void SortById<T>(IList<T> source, bool ascending = true) where T : Identifiable
		{
			(source as List<T>).Sort((T p1, T p2) => (!ascending) ? (p2.ID - p1.ID) : (p1.ID - p2.ID));
		}

		public static IList<T> ListIdentifiables<T>(IDictionary<int, T> map) where T : Identifiable
		{
			return new List<T>(map.Values);
		}
	}
}
