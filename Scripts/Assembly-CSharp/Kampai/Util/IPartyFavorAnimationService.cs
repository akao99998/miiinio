using System.Collections.Generic;
using Kampai.Game.View;

namespace Kampai.Util
{
	public interface IPartyFavorAnimationService
	{
		void CreateRandomPartyFavor(int minionId = -1);

		HashSet<int> GetAllPartyFavorItems();

		List<int> GetAvailablePartyFavorItems();

		void AddAvailablePartyFavorItem(int ID);

		void ReleasePartyFavor(int partyFavorId);

		void AddMinionsToPartyFavor(int partyFavorId, MinionObject minion);

		bool PlayRandomIncidentalAnimation(int minionID);

		void RemoveAllPartyFavorAnimations();
	}
}
