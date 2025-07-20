namespace Kampai.UI
{
	public interface IPlayerTrainingService
	{
		bool HasSeen(int playerTrainingDefinitionId, PlayerTrainingVisiblityType visibilityType);

		void MarkSeen(int playerTrainingDefinitionId, PlayerTrainingVisiblityType visibilityType);
	}
}
