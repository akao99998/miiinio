using System.Collections.Generic;

namespace Kampai.UI.View
{
	public interface IParentWayFinderView : IWayFinderView, IWorldToGlassView
	{
		Dictionary<int, IChildWayFinderView> ChildrenWayFinders { get; }

		void AddChildWayFinder(IChildWayFinderView childWayFinderView);

		void RemoveChildWayFinder(int childTrackedId);

		void UpdateWayFinderIcon();
	}
}
