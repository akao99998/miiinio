namespace Kampai.Game.View
{
	public class TaskingCharacterObject
	{
		private CharacterObject characterObject;

		public int RoutingIndex;

		public int ID
		{
			get
			{
				return characterObject.ID;
			}
		}

		public CharacterObject Character
		{
			get
			{
				return characterObject;
			}
		}

		public TaskingCharacterObject(CharacterObject delegateCharacterObject, int routingIndex)
		{
			characterObject = delegateCharacterObject;
			RoutingIndex = routingIndex;
		}
	}
}
