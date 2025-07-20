namespace Kampai.Game
{
	public class CameraMovementSettings
	{
		public enum Settings
		{
			None = 0,
			Default = 1,
			ShowMenu = 2,
			Quest = 3,
			KeepUIOpen = 4
		}

		public Settings settings;

		public Building building;

		public Quest quest;

		public float cameraSpeed;

		public bool bypassModal;

		public CameraMovementSettings(Settings settings, Building building, Quest quest)
		{
			this.settings = settings;
			this.building = building;
			this.quest = quest;
			cameraSpeed = 0.8f;
		}

		public CameraMovementSettings(Settings settings, Building building, Quest quest, bool bypassModal)
		{
			this.settings = settings;
			this.building = building;
			this.quest = quest;
			cameraSpeed = 0.8f;
			this.bypassModal = bypassModal;
		}
	}
}
