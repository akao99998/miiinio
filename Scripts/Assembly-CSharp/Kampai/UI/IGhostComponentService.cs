using Kampai.Game;
using Kampai.Game.View;

namespace Kampai.UI
{
	public interface IGhostComponentService
	{
		BuildingObject DisplayGhostBuilding(int componentDefID, GhostBuildingDisplayType displayType);

		void DisplayAllSelectablePlanComponents();

		void DisplayComponentMarkedAsInProgress(MasterPlanComponent component);

		bool DisplayAutoCloseGhostComponent(int componentBuildingDefID, float fadeTime, float openDuration);

		void DisplayZoomedInComponent(int componentID, bool isRegularBuilding);

		void ClearGhostComponentBuildings(bool alsoClearComponentsInProgress = false, bool immediate = false);

		void GhostBuildingAutoRemoved(int id, GhostComponentFadeHelperObject helper);

		void RunBeginGhostComponentFunctionFromDefinition(GhostComponentFunctionType functionType, int defID = 0);

		void RunEndGhostComponentFunctionFromDefinition(GhostFunctionCloseType closeType);
	}
}
