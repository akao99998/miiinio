using Kampai.Util;

namespace Kampai.Game
{
	public interface IBuildingWithCooldown : Building, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		int GetCooldown();
	}
}
