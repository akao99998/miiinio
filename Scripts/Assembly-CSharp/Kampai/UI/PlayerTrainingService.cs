namespace Kampai.UI
{
	public class PlayerTrainingService : IPlayerTrainingService
	{
		private const string SettingsLearnSystemsCategoryItem = "SettingsLearnSystemsCategoryItem-{0}";

		private const string LearnSystemsCategoryItem = "LearnSystemsCategoryItem-{0}";

		[Inject]
		public ILocalPersistanceService persistanceService { get; set; }

		public bool HasSeen(int playerTrainingDefinitionId, PlayerTrainingVisiblityType visibilityType)
		{
			return persistanceService.GetDataIntPlayer(string.Format(GetKeyForVisiblityType(visibilityType), playerTrainingDefinitionId)) == 1;
		}

		public void MarkSeen(int playerTrainingDefinitionId, PlayerTrainingVisiblityType visibilityType)
		{
			if (!HasSeen(playerTrainingDefinitionId, visibilityType))
			{
				persistanceService.PutDataIntPlayer(string.Format(GetKeyForVisiblityType(visibilityType), playerTrainingDefinitionId), 1);
			}
		}

		private string GetKeyForVisiblityType(PlayerTrainingVisiblityType visiblityType)
		{
			if (visiblityType == PlayerTrainingVisiblityType.SETTINGS)
			{
				return "SettingsLearnSystemsCategoryItem-{0}";
			}
			return "LearnSystemsCategoryItem-{0}";
		}
	}
}
