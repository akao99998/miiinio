using Kampai.Util;

namespace Kampai.Game
{
	public interface IMinionPartyBuilding : Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		string GetPartyPrefab(MinionPartyType partyType);
	}
}
