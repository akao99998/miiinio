namespace Kampai.UI.View
{
	public interface IChildWayFinderView : IWayFinderView, IWorldToGlassView
	{
		int ParentWayFinderTrackedId { get; set; }
	}
}
