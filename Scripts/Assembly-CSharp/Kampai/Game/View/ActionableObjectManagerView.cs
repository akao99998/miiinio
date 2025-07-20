using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.View
{
	public abstract class ActionableObjectManagerView : KampaiView
	{
		protected static readonly Dictionary<int, ActionableObject> allObjects = new Dictionary<int, ActionableObject>();

		public static ActionableObject GetFromAllObjects(int objectid)
		{
			ActionableObject value;
			allObjects.TryGetValue(objectid, out value);
			return value;
		}

		public static void ClearAllObjects()
		{
			allObjects.Clear();
		}
	}
}
