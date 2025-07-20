using System.Collections.Generic;
using Kampai.Game.View;

namespace Kampai.Game
{
	public class TemporaryMinionsService
	{
		private IDictionary<int, MinionObject> temporaryMinions = new Dictionary<int, MinionObject>();

		public void addTemporaryMinion(MinionObject mo)
		{
			if (mo != null)
			{
				mo.isTemporaryMinion = true;
				temporaryMinions.Add(mo.ID, mo);
			}
		}

		public void removeTemporaryMinion(int minionId)
		{
			temporaryMinions.Remove(minionId);
		}

		public IDictionary<int, MinionObject> getTemporaryMinions()
		{
			return new Dictionary<int, MinionObject>(temporaryMinions);
		}

		public MinionObject GetMinion(int minionID)
		{
			MinionObject value;
			temporaryMinions.TryGetValue(minionID, out value);
			return value;
		}
	}
}
