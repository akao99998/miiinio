namespace Kampai.Game.View
{
	public class VillainManagerView : ObjectManagerView
	{
		public new VillainView Get(int objectId)
		{
			ActionableObject value;
			objects.TryGetValue(objectId, out value);
			return value as VillainView;
		}
	}
}
