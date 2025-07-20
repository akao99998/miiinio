namespace Kampai.UI.View
{
	public abstract class AbstractChildWayFinderView : AbstractQuestWayFinderView, IChildWayFinderView, IWayFinderView, IWorldToGlassView
	{
		public int ParentWayFinderTrackedId { get; set; }
	}
}
