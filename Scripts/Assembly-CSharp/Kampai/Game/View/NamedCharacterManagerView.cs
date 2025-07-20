namespace Kampai.Game.View
{
	public class NamedCharacterManagerView : ObjectManagerView
	{
		public new NamedCharacterObject Get(int objectId)
		{
			ActionableObject value;
			objects.TryGetValue(objectId, out value);
			return value as NamedCharacterObject;
		}
	}
}
