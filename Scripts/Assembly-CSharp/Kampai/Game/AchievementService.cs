namespace Kampai.Game
{
	public class AchievementService : IAchievementService
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(SocialServices.GOOGLEPLAY)]
		public ISocialService gpService { get; set; }

		public void ShowAchievements()
		{
			gpService.ShowAchievements();
		}

		public void UpdateIncrementalAchievement(int defID, int stepsCompleted)
		{
			AchievementDefinition achievementDefinitionFromDefinitionID = definitionService.GetAchievementDefinitionFromDefinitionID(defID);
			if (achievementDefinitionFromDefinitionID != null)
			{
				int steps = achievementDefinitionFromDefinitionID.Steps;
				int num = 0;
				Achievement firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Achievement>(achievementDefinitionFromDefinitionID.ID);
				if (firstInstanceByDefinitionId != null)
				{
					num = firstInstanceByDefinitionId.Progress;
				}
				playerService.CreateAndRunCustomTransaction(achievementDefinitionFromDefinitionID.ID, stepsCompleted, TransactionTarget.NO_VISUAL);
				num += stepsCompleted;
				float num2 = (float)num / (float)steps * 100f;
				if (num2 > 100f)
				{
					num2 = 100f;
				}
				AchievementID achievementID = achievementDefinitionFromDefinitionID.AchievementID;
				if (achievementID != null)
				{
					IncrementAchievement(achievementID, stepsCompleted, num2);
				}
			}
		}

		private void IncrementAchievement(AchievementID achievementID, int stepsCompleted, float percentComplete)
		{
			gpService.incrementAchievement(achievementID.GooglePlayID, stepsCompleted);
		}
	}
}
