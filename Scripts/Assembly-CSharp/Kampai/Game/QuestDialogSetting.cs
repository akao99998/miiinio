using Kampai.UI.View;

namespace Kampai.Game
{
	public class QuestDialogSetting
	{
		public int definitionID { get; set; }

		public string dialogSound { get; set; }

		public string additionalStringParameter { get; set; }

		public QuestDialogType type { get; set; }

		public QuestDialogSetting()
		{
			dialogSound = string.Empty;
			type = QuestDialogType.NORMAL;
			additionalStringParameter = string.Empty;
		}
	}
}
