using Kampai.Util;

namespace Kampai.Game
{
	public interface ZoomableBuilding : Building, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		ZoomableBuildingDefinition ZoomableDefinition { get; }
	}
}
