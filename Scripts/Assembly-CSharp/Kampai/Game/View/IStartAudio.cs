using Kampai.Main;

namespace Kampai.Game.View
{
	public interface IStartAudio
	{
		void InitAudio(BuildingState creationState, PlayLocalAudioSignal playLocalAudioSignal);

		void NotifyBuildingState(BuildingState newState);
	}
}
