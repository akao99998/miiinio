using Kampai.Game;

namespace Kampai.UI.View
{
	public interface IWayFinderView : IWorldToGlassView
	{
		bool ClickedOnce { get; set; }

		bool Snappable { get; set; }

		bool AvoidsHUD { get; set; }

		Prestige Prestige { get; }

		bool IsTargetObjectVisible();

		void SimulateClick();
	}
}
